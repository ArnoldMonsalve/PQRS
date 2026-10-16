using Microsoft.EntityFrameworkCore;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Enums;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Infrastructure.Services;

public class CatalogoService : ICatalogoService
{
    private readonly PqrsDbContext _db;

    public CatalogoService(PqrsDbContext db) => _db = db;

    public async Task<IReadOnlyList<CatalogoItemDto>> ObtenerTiposSolicitudAsync(CancellationToken ct = default) =>
        await _db.TiposSolicitud.AsNoTracking().Where(t => t.Activo)
            .OrderBy(t => t.Nombre).Select(t => new CatalogoItemDto(t.Id, t.Nombre)).ToListAsync(ct);

    public async Task<IReadOnlyList<CatalogoItemDto>> ObtenerCategoriasAsync(CancellationToken ct = default) =>
        await _db.Categorias.AsNoTracking().Where(c => c.Activo)
            .OrderBy(c => c.Nombre).Select(c => new CatalogoItemDto(c.Id, c.Nombre)).ToListAsync(ct);

    public async Task<IReadOnlyList<CatalogoItemDto>> ObtenerAreasAsync(CancellationToken ct = default) =>
        await _db.Areas.AsNoTracking().Where(a => a.Activo)
            .OrderBy(a => a.Nombre).Select(a => new CatalogoItemDto(a.Id, a.Nombre)).ToListAsync(ct);

    public async Task<string?> ObtenerParametroAsync(string clave, CancellationToken ct = default) =>
        await _db.Parametros.AsNoTracking().Where(p => p.Clave == clave && p.Activo)
            .Select(p => p.Valor).FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<CatalogoItemDto>> ObtenerResponsablesAsync(CancellationToken ct = default) =>
        await _db.Usuarios.AsNoTracking()
            .Where(u => u.Activo && u.Rol != RolUsuario.Radicador)
            .OrderBy(u => u.NombreCompleto).Select(u => new CatalogoItemDto(u.Id, u.NombreCompleto)).ToListAsync(ct);
}
