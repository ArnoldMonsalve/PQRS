namespace PQRS.Domain.Exceptions;

/// <summary>Se lanza cuando una operación incumple una regla del negocio; el mensaje es apto para mostrar al usuario.</summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje) : base(mensaje) { }
}
