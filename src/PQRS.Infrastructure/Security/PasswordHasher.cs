using System.Security.Cryptography;
using PQRS.Application.Interfaces;

namespace PQRS.Infrastructure.Security;

/// <summary>PBKDF2-SHA256 con sal aleatoria. Formato guardado: "v1.iteraciones.sal.hash" (Base64).</summary>
public class PasswordHasher : IPasswordHasher
{
    private const int Iteraciones = 210_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    public string Hash(string clave)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);
        return $"v1.{Iteraciones}.{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string clave, string hash)
    {
        var partes = hash.Split('.');
        if (partes.Length != 4 || partes[0] != "v1" || !int.TryParse(partes[1], out var iteraciones))
            return false;

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(clave, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
