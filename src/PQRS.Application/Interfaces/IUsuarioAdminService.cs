using PQRS.Application.Dtos;

namespace PQRS.Application.Interfaces;

/// <summary>Gestión de usuarios. Solo para administradores (se verifica en la base de datos).</summary>
public interface IUsuarioAdminService
{
    Task<IReadOnlyList<UsuarioAdminDto>> ListarAsync(int actorId, CancellationToken ct = default);

    /// <summary>Crea o actualiza (según Id). Devuelve el Id del usuario guardado.</summary>
    Task<int> GuardarAsync(GuardarUsuarioRequest request, int actorId, CancellationToken ct = default);
}
