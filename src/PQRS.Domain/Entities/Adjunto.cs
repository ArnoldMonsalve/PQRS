using PQRS.Domain.Common;

namespace PQRS.Domain.Entities;

public class Adjunto : EntidadBase
{
    public int SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    public string NombreArchivo { get; set; } = string.Empty;
    public string RutaArchivo { get; set; } = string.Empty;
    public string? TipoContenido { get; set; }
    public long TamanoBytes { get; set; }
}
