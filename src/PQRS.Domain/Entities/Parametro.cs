using PQRS.Domain.Common;

namespace PQRS.Domain.Entities;

/// <summary>Configuración clave/valor para adaptar el sistema a cualquier organización (nombre, logo, correo, etc.).</summary>
public class Parametro : EntidadBase
{
    public string Clave { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}
