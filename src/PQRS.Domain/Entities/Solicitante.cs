using PQRS.Domain.Common;

namespace PQRS.Domain.Entities;

public class Solicitante : EntidadBase
{
    public string TipoDocumento { get; set; } = "CC";
    public string NumeroDocumento { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string? Correo { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}
