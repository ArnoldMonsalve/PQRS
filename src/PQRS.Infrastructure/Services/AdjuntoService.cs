using Microsoft.EntityFrameworkCore;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Entities;
using PQRS.Domain.Enums;
using PQRS.Domain.Exceptions;
using PQRS.Domain.Services;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Infrastructure.Services;

public class AdjuntoService : IAdjuntoService
{
    public const long TamanoMaximoBytes = 10L * 1024 * 1024;
    private const int MaximoPorSolicitud = 20;

    // Lista blanca: se rechaza cualquier otra extensión (ejecutables, scripts, etc.).
    private static readonly HashSet<string> ExtensionesPermitidas = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", ".txt", ".csv",
        ".jpg", ".jpeg", ".png", ".gif", ".zip"
    };

    private readonly PqrsDbContext _db;
    private readonly AlmacenamientoOptions _opciones;

    public AdjuntoService(PqrsDbContext db, AlmacenamientoOptions opciones)
    {
        _db = db;
        _opciones = opciones;
    }

    public async Task<AdjuntoDto> AgregarAsync(int solicitudId, string nombreArchivo, Stream contenido, int usuarioId, CancellationToken ct = default)
    {
        var nombre = Path.GetFileName(nombreArchivo ?? string.Empty).Trim();
        var extension = Path.GetExtension(nombre);

        if (nombre.Length == 0) throw new ReglaNegocioException("El archivo no tiene nombre.");
        if (nombre.Length > 200) throw new ReglaNegocioException("El nombre del archivo es demasiado largo (máximo 200 caracteres).");
        if (!ExtensionesPermitidas.Contains(extension))
            throw new ReglaNegocioException($"Tipo de archivo no permitido ({(extension.Length == 0 ? "sin extensión" : extension)}). " +
                                            $"Permitidos: {string.Join(", ", ExtensionesPermitidas.Order())}.");
        if (contenido.Length == 0) throw new ReglaNegocioException("El archivo está vacío.");
        if (contenido.Length > TamanoMaximoBytes)
            throw new ReglaNegocioException($"El archivo supera el tamaño máximo de {TamanoMaximoBytes / (1024 * 1024)} MB.");

        var solicitud = await _db.Solicitudes.FirstOrDefaultAsync(s => s.Id == solicitudId, ct)
            ?? throw new ReglaNegocioException("La solicitud no existe.");
        if (TransicionesEstado.EsEstadoFinal(solicitud.Estado))
            throw new ReglaNegocioException($"La solicitud {solicitud.Radicado} está {solicitud.Estado} y no admite cambios.");

        await ExigirUsuarioActivoAsync(usuarioId, ct);

        if (await _db.Adjuntos.CountAsync(a => a.SolicitudId == solicitudId && a.Activo, ct) >= MaximoPorSolicitud)
            throw new ReglaNegocioException($"Una solicitud admite hasta {MaximoPorSolicitud} adjuntos.");

        // Nombre en disco generado por el sistema: evita colisiones y cualquier ruta enviada por el usuario.
        var rutaRelativa = Path.Combine(solicitudId.ToString(), $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}");
        var rutaAbsoluta = ResolverRuta(rutaRelativa);
        Directory.CreateDirectory(Path.GetDirectoryName(rutaAbsoluta)!);

        await using (var destino = new FileStream(rutaAbsoluta, FileMode.CreateNew, FileAccess.Write))
            await contenido.CopyToAsync(destino, ct);

        var adjunto = new Adjunto
        {
            SolicitudId = solicitudId,
            NombreArchivo = nombre,
            RutaArchivo = rutaRelativa,
            TipoContenido = null,
            TamanoBytes = contenido.Length
        };
        _db.Adjuntos.Add(adjunto);
        _db.Seguimientos.Add(new Seguimiento
        {
            SolicitudId = solicitudId,
            UsuarioId = usuarioId,
            EstadoAnterior = solicitud.Estado,
            EstadoNuevo = solicitud.Estado,
            Comentario = $"Adjunto agregado: {nombre}"
        });

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch
        {
            File.Delete(rutaAbsoluta);   // no dejar huérfano el archivo si falla el registro
            throw;
        }

        return new AdjuntoDto(adjunto.Id, adjunto.NombreArchivo, adjunto.TamanoBytes, adjunto.FechaCreacion);
    }

    public async Task<string> PrepararParaAbrirAsync(int adjuntoId, CancellationToken ct = default)
    {
        var adjunto = await _db.Adjuntos.AsNoTracking().FirstOrDefaultAsync(a => a.Id == adjuntoId && a.Activo, ct)
            ?? throw new ReglaNegocioException("El adjunto no existe.");

        var origen = ResolverRuta(adjunto.RutaArchivo);
        if (!File.Exists(origen))
            throw new ReglaNegocioException("El archivo no se encuentra en el almacenamiento. Contacte al administrador.");

        var carpeta = Path.Combine(Path.GetTempPath(), "PQRS", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(carpeta);
        var copia = Path.Combine(carpeta, Path.GetFileName(adjunto.NombreArchivo));
        File.Copy(origen, copia);
        return copia;
    }

    public async Task EliminarAsync(int adjuntoId, int usuarioId, CancellationToken ct = default)
    {
        var rol = await _db.Usuarios.AsNoTracking()
            .Where(u => u.Id == usuarioId && u.Activo).Select(u => (RolUsuario?)u.Rol).FirstOrDefaultAsync(ct);
        if (rol is not (RolUsuario.Administrador or RolUsuario.Supervisor or RolUsuario.Responsable))
            throw new ReglaNegocioException("No tiene permisos para realizar esta acción.");

        var adjunto = await _db.Adjuntos.Include(a => a.Solicitud)
            .FirstOrDefaultAsync(a => a.Id == adjuntoId && a.Activo, ct)
            ?? throw new ReglaNegocioException("El adjunto no existe.");

        var solicitud = adjunto.Solicitud!;
        if (TransicionesEstado.EsEstadoFinal(solicitud.Estado))
            throw new ReglaNegocioException($"La solicitud {solicitud.Radicado} está {solicitud.Estado} y no admite cambios.");

        adjunto.Activo = false;
        _db.Seguimientos.Add(new Seguimiento
        {
            SolicitudId = solicitud.Id,
            UsuarioId = usuarioId,
            EstadoAnterior = solicitud.Estado,
            EstadoNuevo = solicitud.Estado,
            Comentario = $"Adjunto eliminado: {adjunto.NombreArchivo}",
            EsInterno = true
        });
        await _db.SaveChangesAsync(ct);
    }

    /// <summary>Combina la ruta base con la relativa y garantiza que no se salga de la carpeta de adjuntos.</summary>
    private string ResolverRuta(string rutaRelativa)
    {
        var baseFull = Path.GetFullPath(_opciones.RutaAdjuntos);
        var completa = Path.GetFullPath(Path.Combine(baseFull, rutaRelativa));

        if (!completa.StartsWith(baseFull + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new ReglaNegocioException("Ruta de archivo no válida.");

        return completa;
    }

    private async Task ExigirUsuarioActivoAsync(int usuarioId, CancellationToken ct)
    {
        if (!await _db.Usuarios.AsNoTracking().AnyAsync(u => u.Id == usuarioId && u.Activo, ct))
            throw new ReglaNegocioException("No tiene permisos para realizar esta acción.");
    }
}
