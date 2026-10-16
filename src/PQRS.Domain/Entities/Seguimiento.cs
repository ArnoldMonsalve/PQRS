using PQRS.Domain.Common;
using PQRS.Domain.Enums;

namespace PQRS.Domain.Entities;

/// <summary>Historial de cambios y comentarios de una solicitud (trazabilidad).</summary>
public class Seguimiento : EntidadBase
{
    public int SolicitudId { get; set; }
    public Solicitud? Solicitud { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public EstadoSolicitud? EstadoAnterior { get; set; }
    public EstadoSolicitud EstadoNuevo { get; set; }

    public string Comentario { get; set; } = string.Empty;

    /// <summary>Si es true, solo lo ve el personal interno.</summary>
    public bool EsInterno { get; set; }
}
