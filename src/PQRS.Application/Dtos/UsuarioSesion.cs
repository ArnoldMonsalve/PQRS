using PQRS.Domain.Enums;

namespace PQRS.Application.Dtos;

public record UsuarioSesion(int Id, string NombreCompleto, string NombreUsuario, RolUsuario Rol, int? AreaId);
