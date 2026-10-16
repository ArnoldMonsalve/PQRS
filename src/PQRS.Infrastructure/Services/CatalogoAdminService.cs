using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Entities;
using PQRS.Domain.Exceptions;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Infrastructure.Services;

public partial class CatalogoAdminService : ICatalogoAdminService
{
    private static readonly string[] ParametrosPermitidos = ["NombreOrganizacion", "CorreoRemitente"];

    private readonly PqrsDbContext _db;

    public CatalogoAdminService(PqrsDbContext db) => _db = db;

    [GeneratedRegex(@"^[A-Z0-9]{1,5}$")]
    private static partial Regex PatronPrefijo();

    // ---------- Tipos de solicitud ----------

    public async Task<IReadOnlyList<TipoSolicitudAdminDto>> ListarTiposAsync(int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);
        return await _db.TiposSolicitud.AsNoTracking().OrderBy(t => t.Nombre)
            .Select(t => new TipoSolicitudAdminDto(t.Id, t.Nombre, t.Descripcion, t.DiasRespuesta, t.Prefijo, t.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> GuardarTipoAsync(TipoSolicitudAdminDto dto, int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);

        var nombre = dto.Nombre?.Trim() ?? string.Empty;
        var prefijo = dto.Prefijo?.Trim().ToUpperInvariant() ?? string.Empty;

        if (nombre.Length is 0 or > 100) throw new ReglaNegocioException("El nombre es obligatorio (máximo 100 caracteres).");
        if (!PatronPrefijo().IsMatch(prefijo))
            throw new ReglaNegocioException("El prefijo debe tener de 1 a 5 letras o números (se usa en el radicado).");
        if (dto.DiasRespuesta is < 1 or > 365)
            throw new ReglaNegocioException("Los días de respuesta deben estar entre 1 y 365.");
        if (await _db.TiposSolicitud.AnyAsync(t => t.Id != dto.Id && (t.Nombre == nombre || t.Prefijo == prefijo), ct))
            throw new ReglaNegocioException("Ya existe un tipo con ese nombre o prefijo.");

        var tipo = dto.Id == 0
            ? _db.TiposSolicitud.Add(new TipoSolicitud()).Entity
            : await _db.TiposSolicitud.FirstOrDefaultAsync(t => t.Id == dto.Id, ct)
                ?? throw new ReglaNegocioException("El tipo de solicitud no existe.");

        tipo.Nombre = nombre;
        tipo.Descripcion = Vacio(dto.Descripcion);
        tipo.DiasRespuesta = dto.DiasRespuesta;
        tipo.Prefijo = prefijo;
        tipo.Activo = dto.Activo;

        await _db.SaveChangesAsync(ct);
        return tipo.Id;
    }

    // ---------- Categorías ----------

    public async Task<IReadOnlyList<CategoriaAdminDto>> ListarCategoriasAsync(int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);
        return await _db.Categorias.AsNoTracking().OrderBy(c => c.Nombre)
            .Select(c => new CategoriaAdminDto(c.Id, c.Nombre, c.Descripcion, c.AreaPredeterminadaId,
                c.AreaPredeterminada != null ? c.AreaPredeterminada.Nombre : null, c.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> GuardarCategoriaAsync(CategoriaAdminDto dto, int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);

        var nombre = dto.Nombre?.Trim() ?? string.Empty;
        if (nombre.Length is 0 or > 150) throw new ReglaNegocioException("El nombre es obligatorio (máximo 150 caracteres).");
        if (dto.AreaPredeterminadaId is { } areaId && !await _db.Areas.AnyAsync(a => a.Id == areaId, ct))
            throw new ReglaNegocioException("El área seleccionada no existe.");
        if (await _db.Categorias.AnyAsync(c => c.Id != dto.Id && c.Nombre == nombre, ct))
            throw new ReglaNegocioException("Ya existe una categoría con ese nombre.");

        var categoria = dto.Id == 0
            ? _db.Categorias.Add(new Categoria()).Entity
            : await _db.Categorias.FirstOrDefaultAsync(c => c.Id == dto.Id, ct)
                ?? throw new ReglaNegocioException("La categoría no existe.");

        categoria.Nombre = nombre;
        categoria.Descripcion = Vacio(dto.Descripcion);
        categoria.AreaPredeterminadaId = dto.AreaPredeterminadaId;
        categoria.Activo = dto.Activo;

        await _db.SaveChangesAsync(ct);
        return categoria.Id;
    }

    // ---------- Áreas ----------

    public async Task<IReadOnlyList<AreaAdminDto>> ListarAreasAsync(int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);
        return await _db.Areas.AsNoTracking().OrderBy(a => a.Nombre)
            .Select(a => new AreaAdminDto(a.Id, a.Nombre, a.CorreoNotificacion, a.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> GuardarAreaAsync(AreaAdminDto dto, int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);

        var nombre = dto.Nombre?.Trim() ?? string.Empty;
        var correo = Vacio(dto.CorreoNotificacion);

        if (nombre.Length is 0 or > 150) throw new ReglaNegocioException("El nombre es obligatorio (máximo 150 caracteres).");
        if (correo is not null && (correo.Length > 200 || !MailAddress.TryCreate(correo, out _)))
            throw new ReglaNegocioException("El correo de notificación no es válido.");
        if (await _db.Areas.AnyAsync(a => a.Id != dto.Id && a.Nombre == nombre, ct))
            throw new ReglaNegocioException("Ya existe un área con ese nombre.");

        var area = dto.Id == 0
            ? _db.Areas.Add(new Area()).Entity
            : await _db.Areas.FirstOrDefaultAsync(a => a.Id == dto.Id, ct)
                ?? throw new ReglaNegocioException("El área no existe.");

        area.Nombre = nombre;
        area.CorreoNotificacion = correo;
        area.Activo = dto.Activo;

        await _db.SaveChangesAsync(ct);
        return area.Id;
    }

    // ---------- Parámetros ----------

    public async Task GuardarParametroAsync(string clave, string valor, int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);

        if (!ParametrosPermitidos.Contains(clave))
            throw new ReglaNegocioException($"El parámetro '{clave}' no es editable.");
        valor = valor?.Trim() ?? string.Empty;
        if (valor.Length > 500) throw new ReglaNegocioException("El valor es demasiado largo (máximo 500 caracteres).");
        if (clave == "NombreOrganizacion" && valor.Length == 0)
            throw new ReglaNegocioException("El nombre de la organización es obligatorio.");
        if (clave == "CorreoRemitente" && valor.Length > 0 && !MailAddress.TryCreate(valor, out _))
            throw new ReglaNegocioException("El correo remitente no es válido.");

        var parametro = await _db.Parametros.FirstOrDefaultAsync(p => p.Clave == clave, ct);
        if (parametro is null)
            _db.Parametros.Add(new Parametro { Clave = clave, Valor = valor });
        else
            parametro.Valor = valor;

        await _db.SaveChangesAsync(ct);
    }

    private static string? Vacio(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
