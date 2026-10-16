using PQRS.Application.Dtos;

namespace PQRS.Application.Interfaces;

public interface IAuthService
{
    /// <summary>Devuelve el usuario si las credenciales son válidas y está activo; si no, null.</summary>
    Task<UsuarioSesion?> IniciarSesionAsync(string nombreUsuario, string clave, CancellationToken ct = default);
}
