using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using PQRS.Application.Interfaces;
using PQRS.Desktop.ViewModels.Admin;
using PQRS.Domain.Enums;

namespace PQRS.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _services;
    private readonly ISesionActual _sesion;
    private readonly ICatalogoService _catalogo;

    public MainViewModel(IServiceProvider services, ISesionActual sesion, ICatalogoService catalogo)
    {
        _services = services;
        _sesion = sesion;
        _catalogo = catalogo;

        NavegarBandeja();
        _ = CargarNombreOrganizacionAsync();
    }

    /// <summary>Se dispara al cerrar sesión para que la aplicación vuelva al login.</summary>
    public event Action? SesionCerrada;

    public string UsuarioActual =>
        _sesion.Usuario is { } u ? $"{u.NombreCompleto} ({u.Rol})" : string.Empty;

    public bool EsAdministrador => _sesion.TieneRol(RolUsuario.Administrador);
    public bool PuedeVerReportes => _sesion.TieneRol(RolUsuario.Administrador, RolUsuario.Supervisor);

    [ObservableProperty]
    private string _nombreOrganizacion = "Sistema PQRS";

    // La vista que corresponde a cada ViewModel se define en App.xaml (DataTemplates).
    [ObservableProperty]
    private object? _vistaActual;

    [RelayCommand]
    private void NavegarBandeja()
    {
        var bandeja = _services.GetRequiredService<BandejaSolicitudesViewModel>();
        bandeja.SolicitudAbierta += async id => await AbrirDetalleAsync(id);
        VistaActual = bandeja;
    }

    [RelayCommand]
    private void NavegarRadicar() =>
        VistaActual = _services.GetRequiredService<RadicarSolicitudViewModel>();

    [RelayCommand]
    private void NavegarReportes()
    {
        if (!PuedeVerReportes) return;
        VistaActual = _services.GetRequiredService<ReportesViewModel>();
    }

    [RelayCommand]
    private void NavegarAdministracion()
    {
        if (!EsAdministrador) return;

        var administracion = _services.GetRequiredService<AdministracionViewModel>();
        administracion.Organizacion.NombreActualizado += nombre => NombreOrganizacion = nombre;
        VistaActual = administracion;
    }

    private async Task AbrirDetalleAsync(int solicitudId)
    {
        var detalle = _services.GetRequiredService<DetalleSolicitudViewModel>();
        detalle.Volver += NavegarBandeja;
        await detalle.CargarAsync(solicitudId);
        VistaActual = detalle;
    }

    private async Task CargarNombreOrganizacionAsync()
    {
        try
        {
            var nombre = await _catalogo.ObtenerParametroAsync("NombreOrganizacion");
            if (!string.IsNullOrWhiteSpace(nombre)) NombreOrganizacion = nombre;
        }
        catch
        {
            // El nombre es solo decorativo: si falla se conserva el valor por defecto.
        }
    }

    [RelayCommand]
    private void CerrarSesion()
    {
        _sesion.Cerrar();
        SesionCerrada?.Invoke();
    }
}
