using PQRS.Domain.Common;
using PQRS.Domain.Enums;

namespace PQRS.Domain.Entities;

public class Usuario : EntidadBase
{
    public string NombreCompleto { get; set; } = string.Empty;
    public string NombreUsuario { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public int? AreaId { get; set; }
    public Area? Area { get; set; }
}
