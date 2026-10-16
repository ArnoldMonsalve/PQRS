using PQRS.Domain.Common;
using PQRS.Domain.Enums;

namespace PQRS.Domain.Entities;

public class Solicitud : EntidadBase
{
    /// <summary>Número único de radicado, ej: P-2026-000001.</summary>
    public string Radicado { get; set; } = string.Empty;

    public int TipoSolicitudId { get; set; }
    public TipoSolicitud? TipoSolicitud { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public int? AreaId { get; set; }
    public Area? Area { get; set; }

    /// <summary>Null cuando la solicitud es anónima.</summary>
    public int? SolicitanteId { get; set; }
    public Solicitante? Solicitante { get; set; }

    public int? ResponsableId { get; set; }
    public Usuario? Responsable { get; set; }

    public string Asunto { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;

    public CanalRecepcion Canal { get; set; } = CanalRecepcion.Presencial;
    public Prioridad Prioridad { get; set; } = Prioridad.Media;
    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Radicada;

    public DateTime FechaRadicacion { get; set; } = DateTime.Now;
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaCierre { get; set; }

    public ICollection<Seguimiento> Seguimientos { get; set; } = new List<Seguimiento>();
    public ICollection<Respuesta> Respuestas { get; set; } = new List<Respuesta>();
    public ICollection<Adjunto> Adjuntos { get; set; } = new List<Adjunto>();

    public bool EstaVencida =>
        Estado is not (EstadoSolicitud.Respondida or EstadoSolicitud.Cerrada or EstadoSolicitud.Anulada)
        && DateTime.Now > FechaVencimiento;
}
