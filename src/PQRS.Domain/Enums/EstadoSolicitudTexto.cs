namespace PQRS.Domain.Enums;

public static class EstadoSolicitudTexto
{
    /// <summary>Nombre del estado para mostrar al usuario (con tildes y espacios).</summary>
    public static string ATexto(this EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.EnTramite => "En trámite",
        EstadoSolicitud.PendienteInformacion => "Pendiente de información",
        _ => estado.ToString()
    };
}
