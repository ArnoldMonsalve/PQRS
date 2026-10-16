using PQRS.Domain.Enums;

namespace PQRS.Application.Dtos;

public record UsuarioAdminDto(
    int Id, string NombreCompleto, string NombreUsuario, string Correo,
    RolUsuario Rol, int? AreaId, string? Area, bool Activo);

/// <summary>Id = 0 crea el usuario. Clave: obligatoria al crear; al editar, vacía significa "no cambiar".</summary>
public record GuardarUsuarioRequest(
    int Id, string NombreCompleto, string NombreUsuario, string Correo,
    RolUsuario Rol, int? AreaId, bool Activo, string? Clave);

public record TipoSolicitudAdminDto(
    int Id, string Nombre, string? Descripcion, int DiasRespuesta, string Prefijo, bool Activo);

public record CategoriaAdminDto(
    int Id, string Nombre, string? Descripcion, int? AreaPredeterminadaId, string? AreaPredeterminada, bool Activo);

public record AreaAdminDto(int Id, string Nombre, string? CorreoNotificacion, bool Activo);
