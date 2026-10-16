using PQRS.Application.Dtos;

namespace PQRS.Application.Interfaces;

public interface ICatalogoService
{
    Task<IReadOnlyList<CatalogoItemDto>> ObtenerTiposSolicitudAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CatalogoItemDto>> ObtenerCategoriasAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CatalogoItemDto>> ObtenerAreasAsync(CancellationToken ct = default);

    /// <summary>Valor de un parámetro de configuración (ej. NombreOrganizacion) o null si no existe.</summary>
    Task<string?> ObtenerParametroAsync(string clave, CancellationToken ct = default);

    /// <summary>Usuarios que pueden atender solicitudes (administradores, supervisores y responsables).</summary>
    Task<IReadOnlyList<CatalogoItemDto>> ObtenerResponsablesAsync(CancellationToken ct = default);
}
