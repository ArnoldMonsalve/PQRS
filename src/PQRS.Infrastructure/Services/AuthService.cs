using Microsoft.EntityFrameworkCore;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly PqrsDbContext _db;
    private readonly IPasswordHasher _hasher;

    public AuthService(PqrsDbContext db, IPasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task<UsuarioSesion?> IniciarSesionAsync(string nombreUsuario, string clave, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrEmpty(clave))
            return null;

        var usuario = await _db.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario.Trim() && u.Activo, ct);

        if (usuario is null || !_hasher.Verificar(clave, usuario.PasswordHash))
            return null;

        return new UsuarioSesion(usuario.Id, usuario.NombreCompleto, usuario.NombreUsuario, usuario.Rol, usuario.AreaId);
    }
}
