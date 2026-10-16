using PQRS.Domain.Enums;

namespace PQRS.Application.Dtos;

public record SolicitudResumenDto(
    int Id,
    string Radicado,
    string Tipo,
    string Categoria,
    string? Area,
    string Asunto,
    string Solicitante,
    EstadoSolicitud Estado,
    Prioridad Prioridad,
    DateTime FechaRadicacion,
    DateTime FechaVencimiento,
    bool Vencida);

public record SeguimientoDto(
    DateTime Fecha,
    string? Usuario,
    EstadoSolicitud? EstadoAnterior,
    EstadoSolicitud EstadoNuevo,
    string Comentario,
    bool EsInterno);

public record RespuestaDto(DateTime Fecha, string Usuario, string Contenido);

public record SolicitudDetalleDto(
    SolicitudResumenDto Resumen,
    string Descripcion,
    CanalRecepcion Canal,
    string? Responsable,
    SolicitanteDto? Solicitante,
    DateTime? FechaCierre,
    IReadOnlyList<SeguimientoDto> Seguimientos,
    IReadOnlyList<RespuestaDto> Respuestas,
    IReadOnlyList<AdjuntoDto> Adjuntos);

public record FiltroSolicitudes(
    string? Texto = null,               // busca en radicado, asunto y documento del solicitante
    EstadoSolicitud? Estado = null,
    int? TipoSolicitudId = null,
    int? AreaId = null,
    int? ResponsableId = null,
    DateTime? Desde = null,
    DateTime? Hasta = null,
    bool? SoloVencidas = null);

public record CatalogoItemDto(int Id, string Nombre);
