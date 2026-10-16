using PQRS.Domain.Common;

namespace PQRS.Domain.Entities;

/// <summary>Catálogo configurable: Petición, Queja, Reclamo, Sugerencia, Felicitación, etc.</summary>
public class TipoSolicitud : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int DiasRespuesta { get; set; } = 15;
    public string Prefijo { get; set; } = string.Empty;
}
