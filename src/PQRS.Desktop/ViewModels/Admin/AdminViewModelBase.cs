using CommunityToolkit.Mvvm.ComponentModel;
using PQRS.Application.Interfaces;
using PQRS.Domain.Exceptions;

namespace PQRS.Desktop.ViewModels.Admin;

/// <summary>Estado y manejo de errores común a las pantallas de administración.</summary>
public abstract partial class AdminViewModelBase : ObservableObject
{
    private readonly ISesionActual _sesion;

    protected AdminViewModelBase(ISesionActual sesion) => _sesion = sesion;

    [ObservableProperty] private string _mensajeError = string.Empty;
    [ObservableProperty] private string _mensajeExito = string.Empty;
    [ObservableProperty] private bool _estaOcupado;

    protected int ActorId => _sesion.Usuario?.Id ?? throw new ReglaNegocioException("No hay una sesión activa.");

    protected async Task EjecutarAsync(Func<Task> accion, string? mensajeExito = null)
    {
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
        EstaOcupado = true;
        try
        {
            await accion();
            if (mensajeExito is not null) MensajeExito = mensajeExito;
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
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
