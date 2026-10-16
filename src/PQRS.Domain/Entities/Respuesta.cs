using PQRS.Domain.Common;

namespace PQRS.Domain.Entities;

/// <summary>Respuesta oficial entregada al solicitante.</summary>
public class Respuesta : EntidadBase
{
    public int SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaRespuesta { get; set; } = DateTime.Now;
}
