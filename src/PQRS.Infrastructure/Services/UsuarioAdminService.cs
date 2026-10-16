using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Entities;
using PQRS.Domain.Enums;
using PQRS.Domain.Exceptions;
using PQRS.Domain.Services;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Infrastructure.Services;

public partial class UsuarioAdminService : IUsuarioAdminService
{
    private readonly PqrsDbContext _db;
    private readonly IPasswordHasher _hasher;

    public UsuarioAdminService(PqrsDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    [GeneratedRegex(@"^[A-Za-z0-9._-]{3,50}$")]
    private static partial Regex PatronUsuario();

    public async Task<IReadOnlyList<UsuarioAdminDto>> ListarAsync(int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);

        return await _db.Usuarios.AsNoTracking()
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new UsuarioAdminDto(u.Id, u.NombreCompleto, u.NombreUsuario, u.Correo,
                u.Rol, u.AreaId, u.Area != null ? u.Area.Nombre : null, u.Activo))
            .ToListAsync(ct);
    }

    public async Task<int> GuardarAsync(GuardarUsuarioRequest r, int actorId, CancellationToken ct = default)
    {
        await Permisos.ExigirAdministradorAsync(_db, actorId, ct);

        var nombre = r.NombreCompleto?.Trim() ?? string.Empty;
        var login = r.NombreUsuario?.Trim() ?? string.Empty;
        var correo = r.Correo?.Trim() ?? string.Empty;

        if (nombre.Length is 0 or > 200) throw new ReglaNegocioException("El nombre es obligatorio (máximo 200 caracteres).");
        if (!PatronUsuario().IsMatch(login))
            throw new ReglaNegocioException("El usuario debe tener entre 3 y 50 caracteres: letras, números, punto, guion o guion bajo.");
        if (correo.Length > 200 || (correo.Length > 0 && !MailAddress.TryCreate(correo, out _)))
            throw new ReglaNegocioException("El correo electrónico no es válido.");
        if (!Enum.IsDefined(r.Rol)) throw new ReglaNegocioException("Seleccione un rol válido.");

        if (r.AreaId is { } areaId && !await _db.Areas.AnyAsync(a => a.Id == areaId, ct))
            throw new ReglaNegocioException("El área seleccionada no existe.");

        if (await _db.Usuarios.AnyAsync(u => u.NombreUsuario == login && u.Id != r.Id, ct))
            throw new ReglaNegocioException($"Ya existe un usuario con el nombre '{login}'.");

        Usuario usuario;
        if (r.Id == 0)
        {
            var errorClave = PoliticaClaves.Validar(r.Clave);
            if (errorClave is not null) throw new ReglaNegocioException(errorClave);

            usuario = new Usuario { PasswordHash = _hasher.Hash(r.Clave!) };
            _db.Usuarios.Add(usuario);
        }
        else
        {
            usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == r.Id, ct)
                ?? throw new ReglaNegocioException("El usuario no existe.");

            await ProtegerAdministradoresAsync(usuario, r, actorId, ct);

            if (!string.IsNullOrEmpty(r.Clave))
            {
                var errorClave = PoliticaClaves.Validar(r.Clave);
                if (errorClave is not null) throw new ReglaNegocioException(errorClave);
                usuario.PasswordHash = _hasher.Hash(r.Clave);
            }
        }

        usuario.NombreCompleto = nombre;
        usuario.NombreUsuario = login;
        usuario.Correo = correo;
        usuario.Rol = r.Rol;
        usuario.AreaId = r.AreaId;
        usuario.Activo = r.Activo;

        await _db.SaveChangesAsync(ct);
        return usuario.Id;
    }

    /// <summary>Evita quedarse sin administradores o que alguien se quite el acceso a sí mismo por error.</summary>
    private async Task ProtegerAdministradoresAsync(Usuario actual, GuardarUsuarioRequest r, int actorId, CancellationToken ct)
    {
        var dejaDeSerAdminActivo = actual is { Rol: RolUsuario.Administrador, Activo: true }
            && (r.Rol != RolUsuario.Administrador || !r.Activo);
        if (!dejaDeSerAdminActivo) return;

        if (actual.Id == actorId)
            throw new ReglaNegocioException("No puede desactivar su propio usuario ni quitarse el rol de administrador.");

        var quedaOtro = await _db.Usuarios.AnyAsync(u =>
            u.Id != actual.Id && u.Activo && u.Rol == RolUsuario.Administrador, ct);
        if (!quedaOtro)
            throw new ReglaNegocioException("Debe existir al menos un administrador activo.");
    }
}
