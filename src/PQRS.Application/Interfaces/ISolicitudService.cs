using PQRS.Application.Dtos;
using PQRS.Domain.Enums;

namespace PQRS.Application.Interfaces;

public interface ISolicitudService
{
    /// <summary>Crea la solicitud, genera el radicado y calcula la fecha de vencimiento.</summary>
    Task<SolicitudDetalleDto> RadicarAsync(CrearSolicitudRequest request, CancellationToken ct = default);

    Task<SolicitudDetalleDto?> ObtenerPorRadicadoAsync(string radicado, CancellationToken ct = default);
    Task<SolicitudDetalleDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<SolicitudResumenDto>> BuscarAsync(FiltroSolicitudes filtro, CancellationToken ct = default);

    /// <summary>Asigna un responsable (y el área de este); si estaba Radicada pasa a En trámite.</summary>
    Task AsignarAsync(int solicitudId, int responsableId, int usuarioId, CancellationToken ct = default);

    Task CambiarEstadoAsync(int solicitudId, EstadoSolicitud nuevoEstado, string comentario, int usuarioId, bool esInterno = false, CancellationToken ct = default);

    /// <summary>Registra la respuesta oficial y pasa la solicitud a Respondida.</summary>
    Task ResponderAsync(int solicitudId, string contenido, int usuarioId, CancellationToken ct = default);

    /// <summary>Agrega un comentario al historial sin cambiar el estado.</summary>
    Task ComentarAsync(int solicitudId, string comentario, int usuarioId, bool esInterno, CancellationToken ct = default);
}
