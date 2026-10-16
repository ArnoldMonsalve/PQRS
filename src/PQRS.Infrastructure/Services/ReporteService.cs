using Microsoft.EntityFrameworkCore;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Enums;
using PQRS.Domain.Exceptions;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Infrastructure.Services;

public class ReporteService : IReporteService
{
    private static readonly EstadoSolicitud[] EstadosAbiertos =
        [EstadoSolicitud.Radicada, EstadoSolicitud.EnTramite, EstadoSolicitud.PendienteInformacion];

    private readonly PqrsDbContext _db;

    public ReporteService(PqrsDbContext db) => _db = db;

    public async Task<ResumenReporteDto> ObtenerResumenAsync(DateTime? desde, DateTime? hasta, int actorId, CancellationToken ct = default)
    {
        await ExigirPermisoAsync(actorId, ct);
        ValidarRango(desde, hasta);

        // Solo se traen las columnas necesarias; los cálculos de fechas se hacen en memoria.
        var filas = await FiltrarPorRango(desde, hasta)
            .Select(s => new
            {
                s.Estado,
                s.FechaRadicacion,
                s.FechaVencimiento,
                Tipo = s.TipoSolicitud!.Nombre,
                Categoria = s.Categoria!.Nombre,
                Area = s.Area != null ? s.Area.Nombre : null,
                PrimeraRespuesta = s.Respuestas.Min(r => (DateTime?)r.FechaRespuesta)
            })
            .ToListAsync(ct);

        var ahora = DateTime.Now;
        var abiertas = filas.Where(f => EstadosAbiertos.Contains(f.Estado)).ToList();
        var respondidas = filas.Where(f => f.Estado is EstadoSolicitud.Respondida or EstadoSolicitud.Cerrada).ToList();
        var conRespuesta = filas.Where(f => f.PrimeraRespuesta is not null).ToList();

        double? promedio = conRespuesta.Count == 0
            ? null
            : Math.Round(conRespuesta.Average(f => (f.PrimeraRespuesta!.Value - f.FechaRadicacion).TotalDays), 1);

        // "A tiempo": la primera respuesta se dio a más tardar el día de vencimiento.
        double? aTiempo = conRespuesta.Count == 0
            ? null
            : Math.Round(100.0 * conRespuesta.Count(f => f.PrimeraRespuesta!.Value.Date <= f.FechaVencimiento.Date) / conRespuesta.Count, 1);

        return new ResumenReporteDto(
            Total: filas.Count,
            Abiertas: abiertas.Count,
            Vencidas: abiertas.Count(f => f.FechaVencimiento < ahora),
            Respondidas: respondidas.Count,
            Anuladas: filas.Count(f => f.Estado == EstadoSolicitud.Anulada),
            PromedioDiasRespuesta: promedio,
            PorcentajeATiempo: aTiempo,
            PorEstado: Contar(filas.Select(f => f.Estado.ATexto()), orden: Enum.GetValues<EstadoSolicitud>().Select(e => e.ATexto())),
            PorTipo: Contar(filas.Select(f => f.Tipo)),
            PorCategoria: Contar(filas.Select(f => f.Categoria)),
            PorArea: Contar(filas.Select(f => f.Area ?? "Sin asignar")));
    }

    public async Task<IReadOnlyList<SolicitudResumenDto>> ObtenerListadoAsync(DateTime? desde, DateTime? hasta, int actorId, CancellationToken ct = default)
    {
        await ExigirPermisoAsync(actorId, ct);
        ValidarRango(desde, hasta);

        var lista = await FiltrarPorRango(desde, hasta).AsNoTracking()
            .Include(s => s.TipoSolicitud)
            .Include(s => s.Categoria)
            .Include(s => s.Area)
            .Include(s => s.Solicitante)
            .OrderBy(s => s.FechaRadicacion)
            .ToListAsync(ct);

        return lista.Select(s => new SolicitudResumenDto(
            s.Id, s.Radicado, s.TipoSolicitud?.Nombre ?? string.Empty, s.Categoria?.Nombre ?? string.Empty,
            s.Area?.Nombre, s.Asunto, s.Solicitante?.NombreCompleto ?? "Anónimo",
            s.Estado, s.Prioridad, s.FechaRadicacion, s.FechaVencimiento, s.EstaVencida)).ToList();
    }

    private IQueryable<Domain.Entities.Solicitud> FiltrarPorRango(DateTime? desde, DateTime? hasta)
    {
        var query = _db.Solicitudes.AsNoTracking().AsQueryable();
        if (desde is { } d) query = query.Where(s => s.FechaRadicacion >= d.Date);
        if (hasta is { } h) query = query.Where(s => s.FechaRadicacion < h.Date.AddDays(1));
        return query;
    }

    /// <summary>Cuenta por etiqueta. Con 'orden' se conservan los grupos vacíos en ese orden; si no, de mayor a menor.</summary>
    private static IReadOnlyList<ConteoDto> Contar(IEnumerable<string> etiquetas, IEnumerable<string>? orden = null)
    {
        var lista = etiquetas.ToList();
        var total = lista.Count;
        var conteos = lista.GroupBy(e => e).ToDictionary(g => g.Key, g => g.Count());

        var claves = orden?.ToList() ?? conteos.OrderByDescending(c => c.Value).ThenBy(c => c.Key).Select(c => c.Key).ToList();

        return claves
            .Select(etiqueta =>
            {
                var cantidad = conteos.GetValueOrDefault(etiqueta);
                return new ConteoDto(etiqueta, cantidad, total == 0 ? 0 : Math.Round(100.0 * cantidad / total, 1));
            })
            .ToList();
    }

    private static void ValidarRango(DateTime? desde, DateTime? hasta)
    {
        if (desde is { } d && hasta is { } h && d.Date > h.Date)
            throw new ReglaNegocioException("La fecha inicial no puede ser posterior a la final.");
    }

    private async Task ExigirPermisoAsync(int usuarioId, CancellationToken ct)
    {
        var rol = await _db.Usuarios.AsNoTracking()
            .Where(u => u.Id == usuarioId && u.Activo).Select(u => (RolUsuario?)u.Rol).FirstOrDefaultAsync(ct);

        if (rol is not (RolUsuario.Administrador or RolUsuario.Supervisor))
            throw new ReglaNegocioException("Solo administradores y supervisores pueden ver los reportes.");
    }
}
