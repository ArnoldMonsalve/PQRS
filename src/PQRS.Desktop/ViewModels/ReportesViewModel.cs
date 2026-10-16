using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Desktop.Services;
using PQRS.Domain.Exceptions;

namespace PQRS.Desktop.ViewModels;

public partial class ReportesViewModel : ObservableObject
{
    private readonly IReporteService _reportes;
    private readonly IExportadorExcel _exportador;
    private readonly ISesionActual _sesion;
    private readonly IDialogosArchivos _dialogos;

    public ReportesViewModel(IReporteService reportes, IExportadorExcel exportador, ISesionActual sesion, IDialogosArchivos dialogos)
    {
        _reportes = reportes;
        _exportador = exportador;
        _sesion = sesion;
        _dialogos = dialogos;
    }

    // Por defecto: lo que va del año.
    [ObservableProperty] private DateTime? _desde = new DateTime(DateTime.Today.Year, 1, 1);
    [ObservableProperty] private DateTime? _hasta = DateTime.Today;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HayResumen))]
    private ResumenReporteDto? _resumen;

    [ObservableProperty] private string _mensajeError = string.Empty;
    [ObservableProperty] private string _mensajeExito = string.Empty;
    [ObservableProperty] private bool _estaOcupado;

    public bool HayResumen => Resumen is not null;

    private int ActorId => _sesion.Usuario?.Id ?? throw new ReglaNegocioException("No hay una sesión activa.");

    [RelayCommand]
    private Task GenerarAsync() => EjecutarAsync(async () =>
    {
        Resumen = await _reportes.ObtenerResumenAsync(Desde, Hasta, ActorId);
    });

    [RelayCommand]
    private async Task ExportarAsync()
    {
        var ruta = _dialogos.PedirRutaGuardar($"Reporte_PQRS_{DateTime.Now:yyyyMMdd_HHmm}.xlsx", "Libro de Excel|*.xlsx");
        if (ruta is null) return;

        await EjecutarAsync(async () =>
        {
            var resumen = await _reportes.ObtenerResumenAsync(Desde, Hasta, ActorId);
            var listado = await _reportes.ObtenerListadoAsync(Desde, Hasta, ActorId);

            // La escritura del archivo es trabajo de CPU/disco: se saca del hilo de la interfaz.
            await Task.Run(() => _exportador.ExportarReporte(ruta, Desde, Hasta, resumen, listado));

            Resumen = resumen;
            MensajeExito = $"Reporte exportado: {ruta}";
            _dialogos.Abrir(ruta);
        });
    }

    private async Task EjecutarAsync(Func<Task> accion)
    {
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
        EstaOcupado = true;
        try
        {
            await accion();
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (IOException ex)
        {
            MensajeError = $"No se pudo escribir el archivo (¿está abierto en Excel?): {ex.Message}";
        }
        catch (Exception ex)
        {
            MensajeError = $"No se pudo completar la acción: {ex.GetBaseException().Message}";
        }
        finally
        {
            EstaOcupado = false;
        }
    }
}
