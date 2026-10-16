using Microsoft.EntityFrameworkCore;
using PQRS.Domain.Enums;
using PQRS.Domain.Exceptions;
using PQRS.Infrastructure.Persistence;

namespace PQRS.Infrastructure.Services;

internal static class Permisos
{
    /// <summary>Verifica en la base de datos que el usuario sea administrador activo.</summary>
    public static async Task ExigirAdministradorAsync(PqrsDbContext db, int usuarioId, CancellationToken ct)
    {
        var esAdmin = await db.Usuarios.AsNoTracking()
            .AnyAsync(u => u.Id == usuarioId && u.Activo && u.Rol == RolUsuario.Administrador, ct);

        if (!esAdmin)
            throw new ReglaNegocioException("Solo un administrador puede realizar esta acción.");
    }
}
