using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;

namespace PQRS.Desktop.ViewModels.Admin;

public partial class AreasAdminViewModel : AdminViewModelBase
{
    private readonly ICatalogoAdminService _admin;

    public AreasAdminViewModel(ICatalogoAdminService admin, ISesionActual sesion) : base(sesion) => _admin = admin;

    public ObservableCollection<AreaAdminDto> Areas { get; } = new();

    [ObservableProperty] private AreaAdminDto? _seleccionada;
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _correoNotificacion = string.Empty;
    [ObservableProperty] private bool _activo = true;

    partial void OnSeleccionadaChanged(AreaAdminDto? value)
    {
        if (value is null) return;
        Id = value.Id;
        Nombre = value.Nombre;
        CorreoNotificacion = value.CorreoNotificacion ?? string.Empty;
        Activo = value.Activo;
    }

    [RelayCommand]
    private async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            await RecargarListaAsync(null);
            Nuevo();
        });
    }

    [RelayCommand]
    private void Nuevo()
    {
        Seleccionada = null;
        Id = 0;
        Nombre = string.Empty;
        CorreoNotificacion = string.Empty;
        Activo = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var id = await _admin.GuardarAreaAsync(new AreaAdminDto(Id, Nombre, CorreoNotificacion, Activo), ActorId);
            await RecargarListaAsync(id);
        }, "Área guardada.");
    }

    private async Task RecargarListaAsync(int? seleccionarId)
    {
        var lista = await _admin.ListarAreasAsync(ActorId);
        Areas.Clear();
        foreach (var a in lista) Areas.Add(a);

        if (seleccionarId is { } id)
        {
            Seleccionada = null;
            Seleccionada = Areas.FirstOrDefault(a => a.Id == id);
        }
    }
}
