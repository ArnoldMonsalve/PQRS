using PQRS.Domain.Common;

namespace PQRS.Domain.Entities;

/// <summary>Dependencia o área responsable de atender solicitudes.</summary>
public class Area : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string? CorreoNotificacion { get; set; }
}
