namespace PQRS.Domain.Services;

public static class CalculadoraVencimiento
{
    /// <summary>
    /// Suma días hábiles (lunes a viernes) a una fecha. Los festivos no se consideran todavía;
    /// cuando se necesiten se puede recibir una lista de festivos como parámetro.
    /// </summary>
    public static DateTime SumarDiasHabiles(DateTime desde, int dias)
    {
        var fecha = desde;
        var restantes = dias;

        while (restantes > 0)
        {
            fecha = fecha.AddDays(1);
            if (fecha.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                restantes--;
        }

        return fecha;
    }
}
