using PQRS.Application.Dtos;

namespace PQRS.Application.Interfaces;

/// <summary>Mantenimiento de catálogos y parámetros. Solo para administradores.
/// Los registros no se borran: se desactivan, para conservar el historial de las solicitudes.</summary>
public interface ICatalogoAdminService
{
    Task<IReadOnlyList<TipoSolicitudAdminDto>> ListarTiposAsync(int actorId, CancellationToken ct = default);
    Task<int> GuardarTipoAsync(TipoSolicitudAdminDto tipo, int actorId, CancellationToken ct = default);

    Task<IReadOnlyList<CategoriaAdminDto>> ListarCategoriasAsync(int actorId, CancellationToken ct = default);
    Task<int> GuardarCategoriaAsync(CategoriaAdminDto categoria, int actorId, CancellationToken ct = default);

    Task<IReadOnlyList<AreaAdminDto>> ListarAreasAsync(int actorId, CancellationToken ct = default);
    Task<int> GuardarAreaAsync(AreaAdminDto area, int actorId, CancellationToken ct = default);

    Task GuardarParametroAsync(string clave, string valor, int actorId, CancellationToken ct = default);
}
