namespace PQRS.Domain.Services;

public static class PoliticaClaves
{
    public const int LongitudMinima = 8;

    /// <summary>Devuelve el motivo del rechazo o null si la contraseña cumple la política.</summary>
    public static string? Validar(string? clave)
    {
        if (string.IsNullOrEmpty(clave) || clave.Length < LongitudMinima)
            return $"La contraseña debe tener al menos {LongitudMinima} caracteres.";
        if (!clave.Any(char.IsLetter) || !clave.Any(char.IsDigit))
            return "La contraseña debe incluir letras y números.";
        return null;
    }
}
