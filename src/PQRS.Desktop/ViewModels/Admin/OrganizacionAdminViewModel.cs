using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Interfaces;

namespace PQRS.Desktop.ViewModels.Admin;

public partial class OrganizacionAdminViewModel : AdminViewModelBase
{
    private readonly ICatalogoAdminService _admin;
    private readonly ICatalogoService _catalogo;

    public OrganizacionAdminViewModel(ICatalogoAdminService admin, ICatalogoService catalogo, ISesionActual sesion)
        : base(sesion)
    {
        _admin = admin;
        _catalogo = catalogo;
    }

    /// <summary>Avisa a la ventana principal para que muestre el nuevo nombre en la barra superior.</summary>
    public event Action<string>? NombreActualizado;

    [ObservableProperty] private string _nombreOrganizacion = string.Empty;
    [ObservableProperty] private string _correoRemitente = string.Empty;

    [RelayCommand]
    private async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            NombreOrganizacion = await _catalogo.ObtenerParametroAsync("NombreOrganizacion") ?? string.Empty;
            CorreoRemitente = await _catalogo.ObtenerParametroAsync("CorreoRemitente") ?? string.Empty;
        });
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            await _admin.GuardarParametroAsync("NombreOrganizacion", NombreOrganizacion, ActorId);
            await _admin.GuardarParametroAsync("CorreoRemitente", CorreoRemitente, ActorId);
            NombreActualizado?.Invoke(NombreOrganizacion.Trim());
        }, "Datos de la organización guardados.");
    }
}
