using PQRS.Application.Dtos;
using PQRS.Domain.Enums;

namespace PQRS.Application.Interfaces;

/// <summary>Usuario que tiene la sesión abierta en la aplicación de escritorio.</summary>
public interface ISesionActual
{
    UsuarioSesion? Usuario { get; }
    bool EstaAutenticado { get; }
    void Iniciar(UsuarioSesion usuario);
    void Cerrar();
    bool TieneRol(params RolUsuario[] roles);
}
