using Microsoft.EntityFrameworkCore;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Entities;
using PQRS.Domain.Enums;
using PQRS.Domain.Exceptions;
using PQRS.Domain.Services;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Infrastructure.Services;

public class SolicitudService : ISolicitudService
{
    private const int MaxIntentosRadicado = 3;
    private const int MaxResultadosBusqueda = 500;

    private readonly PqrsDbContext _db;

    public SolicitudService(PqrsDbContext db) => _db = db;

    public async Task<SolicitudDetalleDto> RadicarAsync(CrearSolicitudRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Asunto))
            throw new ReglaNegocioException("El asunto es obligatorio.");
        if (string.IsNullOrWhiteSpace(request.Descripcion))
            throw new ReglaNegocioException("La descripción es obligatoria.");

        var tipo = await _db.TiposSolicitud.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.TipoSolicitudId && t.Activo, ct)
            ?? throw new ReglaNegocioException("El tipo de solicitud no existe o está inactivo.");

        var categoria = await _db.Categorias.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.CategoriaId && c.Activo, ct)
            ?? throw new ReglaNegocioException("La categoría no existe o está inactiva.");

        var solicitanteId = request.Solicitante is null
            ? (int?)null
            : await ObtenerOCrearSolicitanteAsync(request.Solicitante, ct);

        var ahora = DateTime.Now;
        var vencimiento = CalculadoraVencimiento.SumarDiasHabiles(ahora, tipo.DiasRespuesta);

        for (var intento = 1; ; intento++)
        {
            var solicitud = new Solicitud
            {
                Radicado = await SiguienteRadicadoAsync(tipo.Prefijo, ahora.Year, ct),
                TipoSolicitudId = tipo.Id,
                CategoriaId = categoria.Id,
                AreaId = categoria.AreaPredeterminadaId,
                SolicitanteId = solicitanteId,
                Asunto = request.Asunto.Trim(),
                Descripcion = request.Descripcion.Trim(),
                Canal = request.Canal,
                Prioridad = request.Prioridad,
                Estado = EstadoSolicitud.Radicada,
                FechaRadicacion = ahora,
                FechaVencimiento = vencimiento
            };
            solicitud.Seguimientos.Add(new Seguimiento
            {
                UsuarioId = request.UsuarioId,
                EstadoAnterior = null,
                EstadoNuevo = EstadoSolicitud.Radicada,
                Comentario = "Solicitud radicada."
            });

            _db.Solicitudes.Add(solicitud);
            try
            {
                await _db.SaveChangesAsync(ct);
                return (await ObtenerPorIdAsync(solicitud.Id, ct))!;
            }
            catch (DbUpdateException) when (intento < MaxIntentosRadicado)
            {
                // Dos usuarios generaron el mismo radicado a la vez (índice único): se reintenta con el siguiente.
                _db.ChangeTracker.Clear();
            }
        }
    }

    public Task<SolicitudDetalleDto?> ObtenerPorRadicadoAsync(string radicado, CancellationToken ct = default) =>
        ObtenerDetalleAsync(s => s.Radicado == radicado.Trim(), ct);

    public Task<SolicitudDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ObtenerDetalleAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<SolicitudResumenDto>> BuscarAsync(FiltroSolicitudes filtro, CancellationToken ct = default)
    {
        var query = _db.Solicitudes.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(filtro.Texto))
        {
            var texto = filtro.Texto.Trim();
            query = query.Where(s =>
                s.Radicado.Contains(texto) ||
                s.Asunto.Contains(texto) ||
                (s.Solicitante != null && s.Solicitante.NumeroDocumento.Contains(texto)));
        }
        if (filtro.Estado is { } estado) query = query.Where(s => s.Estado == estado);
        if (filtro.TipoSolicitudId is { } tipoId) query = query.Where(s => s.TipoSolicitudId == tipoId);
        if (filtro.AreaId is { } areaId) query = query.Where(s => s.AreaId == areaId);
        if (filtro.ResponsableId is { } respId) query = query.Where(s => s.ResponsableId == respId);
        if (filtro.Desde is { } desde) query = query.Where(s => s.FechaRadicacion >= desde.Date);
        if (filtro.Hasta is { } hasta) query = query.Where(s => s.FechaRadicacion < hasta.Date.AddDays(1));
        if (filtro.SoloVencidas == true)
        {
            var ahora = DateTime.Now;
            query = query.Where(s =>
                s.Estado != EstadoSolicitud.Respondida &&
                s.Estado != EstadoSolicitud.Cerrada &&
                s.Estado != EstadoSolicitud.Anulada &&
                s.FechaVencimiento < ahora);
        }

        var lista = await query
            .Include(s => s.TipoSolicitud)
            .Include(s => s.Categoria)
            .Include(s => s.Area)
            .Include(s => s.Solicitante)
            .OrderByDescending(s => s.FechaRadicacion)
            .Take(MaxResultadosBusqueda)
            .ToListAsync(ct);

        return lista.Select(AResumen).ToList();
    }

    public async Task AsignarAsync(int solicitudId, int responsableId, int usuarioId, CancellationToken ct = default)
    {
        await ExigirRolAsync(usuarioId, ct, RolUsuario.Administrador, RolUsuario.Supervisor);
        var solicitud = await CargarParaEdicionAsync(solicitudId, ct);
        var responsable = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == responsableId && u.Activo, ct)
            ?? throw new ReglaNegocioException("El responsable no existe o está inactivo.");

        solicitud.ResponsableId = responsable.Id;
        if (responsable.AreaId is { } areaId) solicitud.AreaId = areaId;

        var anterior = solicitud.Estado;
        if (anterior == EstadoSolicitud.Radicada) solicitud.Estado = EstadoSolicitud.EnTramite;

        AgregarSeguimiento(solicitud, usuarioId, anterior, $"Asignada a {responsable.NombreCompleto}.", esInterno: true);
        await _db.SaveChangesAsync(ct);
    }

    public async Task CambiarEstadoAsync(int solicitudId, EstadoSolicitud nuevoEstado, string comentario, int usuarioId, bool esInterno = false, CancellationToken ct = default)
    {
        if (nuevoEstado == EstadoSolicitud.Respondida)
            throw new ReglaNegocioException("Para marcar la solicitud como respondida registre una respuesta.");

        await ExigirRolAsync(usuarioId, ct, RolUsuario.Administrador, RolUsuario.Supervisor, RolUsuario.Responsable);

        var solicitud = await CargarParaEdicionAsync(solicitudId, ct);

        if (!TransicionesEstado.EsPermitida(solicitud.Estado, nuevoEstado))
            throw new ReglaNegocioException($"No se puede pasar de '{solicitud.Estado}' a '{nuevoEstado}'.");

        if (nuevoEstado is EstadoSolicitud.Anulada or EstadoSolicitud.PendienteInformacion
            && string.IsNullOrWhiteSpace(comentario))
            throw new ReglaNegocioException("Debe indicar el motivo del cambio de estado.");

        var anterior = solicitud.Estado;
        solicitud.Estado = nuevoEstado;
        if (TransicionesEstado.EsEstadoFinal(nuevoEstado)) solicitud.FechaCierre = DateTime.Now;

        AgregarSeguimiento(solicitud, usuarioId, anterior,
            string.IsNullOrWhiteSpace(comentario) ? $"Cambio de estado a {nuevoEstado}." : comentario.Trim(), esInterno);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ResponderAsync(int solicitudId, string contenido, int usuarioId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(contenido))
            throw new ReglaNegocioException("El contenido de la respuesta es obligatorio.");

        await ExigirRolAsync(usuarioId, ct, RolUsuario.Administrador, RolUsuario.Supervisor, RolUsuario.Responsable);

        var solicitud = await CargarParaEdicionAsync(solicitudId, ct);

        if (!TransicionesEstado.EsPermitida(solicitud.Estado, EstadoSolicitud.Respondida))
            throw new ReglaNegocioException($"No se puede responder una solicitud en estado '{solicitud.Estado}'. Debe estar En trámite.");

        var anterior = solicitud.Estado;
        solicitud.Estado = EstadoSolicitud.Respondida;
        solicitud.Respuestas.Add(new Respuesta { UsuarioId = usuarioId, Contenido = contenido.Trim() });

        AgregarSeguimiento(solicitud, usuarioId, anterior, "Respuesta entregada al solicitante.", esInterno: false);
        await _db.SaveChangesAsync(ct);
    }

    public async Task ComentarAsync(int solicitudId, string comentario, int usuarioId, bool esInterno, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(comentario))
            throw new ReglaNegocioException("El comentario no puede estar vacío.");

        var solicitud = await CargarParaEdicionAsync(solicitudId, ct);
        AgregarSeguimiento(solicitud, usuarioId, solicitud.Estado, comentario.Trim(), esInterno);
        await _db.SaveChangesAsync(ct);
    }

    // ---------- helpers ----------

    /// <summary>Verifica en la base de datos el rol del usuario, sin confiar en lo que muestre la interfaz.</summary>
    private async Task ExigirRolAsync(int usuarioId, CancellationToken ct, params RolUsuario[] rolesPermitidos)
    {
        var rol = await _db.Usuarios.AsNoTracking()
            .Where(u => u.Id == usuarioId && u.Activo)
            .Select(u => (RolUsuario?)u.Rol)
            .FirstOrDefaultAsync(ct);

        if (rol is null || !rolesPermitidos.Contains(rol.Value))
            throw new ReglaNegocioException("No tiene permisos para realizar esta acción.");
    }

    private async Task<Solicitud> CargarParaEdicionAsync(int id, CancellationToken ct)
    {
        var solicitud = await _db.Solicitudes.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new ReglaNegocioException("La solicitud no existe.");

        if (TransicionesEstado.EsEstadoFinal(solicitud.Estado))
            throw new ReglaNegocioException($"La solicitud {solicitud.Radicado} está {solicitud.Estado} y no admite cambios.");

        return solicitud;
    }

    private static void AgregarSeguimiento(Solicitud solicitud, int usuarioId, EstadoSolicitud estadoAnterior, string comentario, bool esInterno) =>
        solicitud.Seguimientos.Add(new Seguimiento
        {
            UsuarioId = usuarioId,
            EstadoAnterior = estadoAnterior,
            EstadoNuevo = solicitud.Estado,
            Comentario = comentario,
            EsInterno = esInterno
        });

    private async Task<int> ObtenerOCrearSolicitanteAsync(SolicitanteDto dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.NumeroDocumento) || string.IsNullOrWhiteSpace(dto.Nombres))
            throw new ReglaNegocioException("El documento y los nombres del solicitante son obligatorios.");

        var tipoDoc = dto.TipoDocumento.Trim();
        var numero = dto.NumeroDocumento.Trim();

        var existente = await _db.Solicitantes
            .FirstOrDefaultAsync(s => s.TipoDocumento == tipoDoc && s.NumeroDocumento == numero, ct);

        if (existente is null)
        {
            existente = new Solicitante { TipoDocumento = tipoDoc, NumeroDocumento = numero };
            _db.Solicitantes.Add(existente);
        }

        // Se actualizan los datos de contacto con lo más reciente que informó la persona.
        existente.Nombres = dto.Nombres.Trim();
        existente.Apellidos = dto.Apellidos?.Trim() ?? string.Empty;
        existente.Correo = dto.Correo?.Trim();
        existente.Telefono = dto.Telefono?.Trim();
        existente.Direccion = dto.Direccion?.Trim();

        await _db.SaveChangesAsync(ct);
        return existente.Id;
    }

    private async Task<string> SiguienteRadicadoAsync(string prefijo, int anio, CancellationToken ct)
    {
        var inicio = $"{prefijo}-{anio}-";
        var ultimo = await _db.Solicitudes.AsNoTracking()
            .Where(s => s.Radicado.StartsWith(inicio))
            .OrderByDescending(s => s.Radicado)   // el consecutivo tiene 6 dígitos con ceros, ordena bien como texto
            .Select(s => s.Radicado)
            .FirstOrDefaultAsync(ct);

        var consecutivo = ultimo is not null && int.TryParse(ultimo[inicio.Length..], out var n) ? n + 1 : 1;
        return $"{inicio}{consecutivo:D6}";
    }

    private async Task<SolicitudDetalleDto?> ObtenerDetalleAsync(System.Linq.Expressions.Expression<Func<Solicitud, bool>> filtro, CancellationToken ct)
    {
        var s = await _db.Solicitudes.AsNoTracking().AsSplitQuery()
            .Include(x => x.TipoSolicitud)
            .Include(x => x.Categoria)
            .Include(x => x.Area)
            .Include(x => x.Solicitante)
            .Include(x => x.Responsable)
            .Include(x => x.Seguimientos).ThenInclude(g => g.Usuario)
            .Include(x => x.Respuestas).ThenInclude(r => r.Usuario)
            .Include(x => x.Adjuntos.Where(a => a.Activo))
            .FirstOrDefaultAsync(filtro, ct);

        if (s is null) return null;

        var solicitante = s.Solicitante is null
            ? null
            : new SolicitanteDto(s.Solicitante.TipoDocumento, s.Solicitante.NumeroDocumento, s.Solicitante.Nombres,
                s.Solicitante.Apellidos, s.Solicitante.Correo, s.Solicitante.Telefono, s.Solicitante.Direccion);

        return new SolicitudDetalleDto(
            AResumen(s),
            s.Descripcion,
            s.Canal,
            s.Responsable?.NombreCompleto,
            solicitante,
            s.FechaCierre,
            s.Seguimientos.OrderBy(g => g.FechaCreacion)
                .Select(g => new SeguimientoDto(g.FechaCreacion, g.Usuario?.NombreCompleto, g.EstadoAnterior, g.EstadoNuevo, g.Comentario, g.EsInterno))
                .ToList(),
            s.Respuestas.OrderBy(r => r.FechaRespuesta)
                .Select(r => new RespuestaDto(r.FechaRespuesta, r.Usuario?.NombreCompleto ?? string.Empty, r.Contenido))
                .ToList(),
            s.Adjuntos.OrderBy(a => a.FechaCreacion)
                .Select(a => new AdjuntoDto(a.Id, a.NombreArchivo, a.TamanoBytes, a.FechaCreacion))
                .ToList());
    }

    private static SolicitudResumenDto AResumen(Solicitud s) => new(
        s.Id,
        s.Radicado,
        s.TipoSolicitud?.Nombre ?? string.Empty,
        s.Categoria?.Nombre ?? string.Empty,
        s.Area?.Nombre,
        s.Asunto,
        s.Solicitante?.NombreCompleto ?? "Anónimo",
        s.Estado,
        s.Prioridad,
        s.FechaRadicacion,
        s.FechaVencimiento,
        s.EstaVencida);
}
