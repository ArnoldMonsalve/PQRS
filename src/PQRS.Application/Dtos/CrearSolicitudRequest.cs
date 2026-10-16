using PQRS.Domain.Enums;

namespace PQRS.Application.Dtos;

public record CrearSolicitudRequest(
    int TipoSolicitudId,
    int CategoriaId,
    string Asunto,
    string Descripcion,
    CanalRecepcion Canal,
    Prioridad Prioridad,
    SolicitanteDto? Solicitante,   // null = solicitud anónima
    int? UsuarioId);               // quién la radica
