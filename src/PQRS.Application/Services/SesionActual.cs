using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Enums;

namespace PQRS.Application.Services;

public class SesionActual : ISesionActual
{
    public UsuarioSesion? Usuario { get; private set; }
    public bool EstaAutenticado => Usuario is not null;

    public void Iniciar(UsuarioSesion usuario) => Usuario = usuario;
    public void Cerrar() => Usuario = null;

    public bool TieneRol(params RolUsuario[] roles) => Usuario is not null && roles.Contains(Usuario.Rol);
}
