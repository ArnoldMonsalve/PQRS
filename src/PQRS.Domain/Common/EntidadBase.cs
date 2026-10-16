namespace PQRS.Domain.Common;

public abstract class EntidadBase
{
    public int Id { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}
