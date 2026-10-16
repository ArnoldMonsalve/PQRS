using PQRS.Application.Dtos;

namespace PQRS.Application.Interfaces;

/// <summary>Reportes y estadísticas. Solo para administradores y supervisores.</summary>
public interface IReporteService
{
    /// <summary>Resumen de las solicitudes radicadas entre las fechas indicadas (ambas inclusive; null = sin límite).</summary>
    Task<ResumenReporteDto> ObtenerResumenAsync(DateTime? desde, DateTime? hasta, int actorId, CancellationToken ct = default);

    /// <summary>Todas las solicitudes del rango, sin el tope de la bandeja (para exportar).</summary>
    Task<IReadOnlyList<SolicitudResumenDto>> ObtenerListadoAsync(DateTime? desde, DateTime? hasta, int actorId, CancellationToken ct = default);
}
