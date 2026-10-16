using System.Diagnostics;
using Microsoft.Win32;

namespace PQRS.Desktop.Services;

/// <summary>Diálogos de archivo y apertura de documentos, aislados para que los ViewModels no dependan de WPF.</summary>
public interface IDialogosArchivos
{
    /// <summary>Muestra "Abrir archivo" y devuelve la ruta elegida o null si se canceló.</summary>
    string? ElegirArchivo(string titulo);

    /// <summary>Muestra "Guardar como" y devuelve la ruta elegida o null si se canceló.</summary>
    string? PedirRutaGuardar(string nombreSugerido, string filtro);

    /// <summary>Abre el archivo con la aplicación predeterminada de Windows.</summary>
    void Abrir(string ruta);
}

public class DialogosArchivos : IDialogosArchivos
{
    public string? ElegirArchivo(string titulo)
    {
        var dialogo = new OpenFileDialog
        {
            Title = titulo,
            Filter = "Documentos e imágenes|*.pdf;*.doc;*.docx;*.xls;*.xlsx;*.ppt;*.pptx;*.txt;*.csv;*.jpg;*.jpeg;*.png;*.gif;*.zip",
            CheckFileExists = true
        };
        return dialogo.ShowDialog() == true ? dialogo.FileName : null;
    }

    public string? PedirRutaGuardar(string nombreSugerido, string filtro)
    {
        var dialogo = new SaveFileDialog { FileName = nombreSugerido, Filter = filtro, OverwritePrompt = true };
        return dialogo.ShowDialog() == true ? dialogo.FileName : null;
    }

    public void Abrir(string ruta) =>
        Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
}
