using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PQRS.Application.Interfaces;
using PQRS.Domain.Entities;
using PQRS.Domain.Enums;

namespace PQRS.Infrastructure.Persistence;

/// <summary>Crea el administrador inicial la primera vez que se ejecuta la aplicación sobre una base sin usuarios.</summary>
public class DbInitializer
{
    private readonly PqrsDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IConfiguration _config;

    public DbInitializer(PqrsDbContext db, IPasswordHasher hasher, IConfiguration config)
    {
        _db = db;
        _hasher = hasher;
        _config = config;
    }

    public async Task EjecutarAsync(CancellationToken ct = default)
    {
        if (await _db.Usuarios.AnyAsync(ct)) return;

        var usuario = _config["Seed:AdminUsuario"];
        var clave = _config["Seed:AdminClave"];
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(clave))
            throw new InvalidOperationException("No hay usuarios y falta 'Seed:AdminUsuario' / 'Seed:AdminClave' en appsettings.json.");

        _db.Usuarios.Add(new Usuario
        {
            NombreCompleto = "Administrador",
            NombreUsuario = usuario.Trim(),
            Correo = _config["Seed:AdminCorreo"] ?? string.Empty,
            PasswordHash = _hasher.Hash(clave),
            Rol = RolUsuario.Administrador
        });
        await _db.SaveChangesAsync(ct);
    }
}
