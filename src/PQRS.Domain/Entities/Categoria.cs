using PQRS.Domain.Common;

namespace PQRS.Domain.Entities;

/// <summary>Clasificación por tema, configurable según el ámbito (salud, educación, servicios, etc.).</summary>
public class Categoria : EntidadBase
{
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int? AreaPredeterminadaId { get; set; }
    public Area? AreaPredeterminada { get; set; }
}
