using PQRS.Domain.Enums;

namespace PQRS.Domain.Services;

/// <summary>Define el flujo permitido entre estados de una solicitud.</summary>
public static class TransicionesEstado
{
    private static readonly Dictionary<EstadoSolicitud, EstadoSolicitud[]> Permitidas = new()
    {
        [EstadoSolicitud.Radicada]             = [EstadoSolicitud.EnTramite, EstadoSolicitud.Anulada],
        [EstadoSolicitud.EnTramite]            = [EstadoSolicitud.PendienteInformacion, EstadoSolicitud.Respondida, EstadoSolicitud.Anulada],
        [EstadoSolicitud.PendienteInformacion] = [EstadoSolicitud.EnTramite, EstadoSolicitud.Anulada],
        [EstadoSolicitud.Respondida]           = [EstadoSolicitud.Cerrada, EstadoSolicitud.EnTramite],
        [EstadoSolicitud.Cerrada]              = [],
        [EstadoSolicitud.Anulada]              = []
    };

    public static bool EsPermitida(EstadoSolicitud actual, EstadoSolicitud nuevo) =>
        Permitidas[actual].Contains(nuevo);

    public static IReadOnlyList<EstadoSolicitud> Siguientes(EstadoSolicitud actual) => Permitidas[actual];

    public static bool EsEstadoFinal(EstadoSolicitud estado) =>
        estado is EstadoSolicitud.Cerrada or EstadoSolicitud.Anulada;
}
