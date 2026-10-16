namespace PQRS.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string clave);
    bool Verificar(string clave, string hash);
}
