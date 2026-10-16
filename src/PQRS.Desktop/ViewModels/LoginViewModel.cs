using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Interfaces;

namespace PQRS.Desktop.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _auth;
    private readonly ISesionActual _sesion;

    public LoginViewModel(IAuthService auth, ISesionActual sesion)
    {
        _auth = auth;
        _sesion = sesion;
    }

    /// <summary>Lo escucha la ventana para cerrarse con resultado correcto.</summary>
    public event Action? LoginExitoso;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IngresarCommand))]
    private string _nombreUsuario = string.Empty;

    // La clave llega desde el PasswordBox (no se puede enlazar directamente por seguridad).
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IngresarCommand))]
    private string _clave = string.Empty;

    [ObservableProperty]
    private string _mensajeError = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(IngresarCommand))]
    private bool _estaOcupado;

    private bool PuedeIngresar() =>
        !EstaOcupado && !string.IsNullOrWhiteSpace(NombreUsuario) && !string.IsNullOrEmpty(Clave);

    [RelayCommand(CanExecute = nameof(PuedeIngresar))]
    private async Task IngresarAsync()
    {
        MensajeError = string.Empty;
        EstaOcupado = true;
        try
        {
            var usuario = await _auth.IniciarSesionAsync(NombreUsuario, Clave);
            if (usuario is null)
            {
                MensajeError = "Usuario o contraseña incorrectos.";
                return;
            }

            _sesion.Iniciar(usuario);
            Clave = string.Empty;
            LoginExitoso?.Invoke();
        }
        catch (Exception ex)
        {
            MensajeError = $"No se pudo conectar: {ex.GetBaseException().Message}";
        }
        finally
        {
            EstaOcupado = false;
        }
    }
}
