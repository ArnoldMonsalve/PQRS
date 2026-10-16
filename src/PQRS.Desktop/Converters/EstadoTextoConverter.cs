using System.Globalization;
using System.Windows.Data;
using PQRS.Domain.Enums;

namespace PQRS.Desktop.Converters;

public static class Textos
{
    public static string Estado(EstadoSolicitud estado) => estado.ATexto();
}

/// <summary>Muestra los estados con tildes y espacios en lugar del nombre del enum.</summary>
public class EstadoTextoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is EstadoSolicitud estado ? Textos.Estado(estado) : string.Empty;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

/// <summary>Convierte bytes en un texto legible (KB, MB).</summary>
public class TamanoArchivoConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not long bytes) return string.Empty;
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:0.#} KB",
            _ => $"{bytes / (1024.0 * 1024.0):0.#} MB"
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
