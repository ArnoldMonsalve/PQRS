using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;

namespace PQRS.Desktop.ViewModels.Admin;

public partial class TiposAdminViewModel : AdminViewModelBase
{
    private readonly ICatalogoAdminService _admin;

    public TiposAdminViewModel(ICatalogoAdminService admin, ISesionActual sesion) : base(sesion) => _admin = admin;

    public ObservableCollection<TipoSolicitudAdminDto> Tipos { get; } = new();

    [ObservableProperty] private TipoSolicitudAdminDto? _seleccionado;
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _descripcion = string.Empty;
    [ObservableProperty] private int _diasRespuesta = 15;
    [ObservableProperty] private string _prefijo = string.Empty;
    [ObservableProperty] private bool _activo = true;

    partial void OnSeleccionadoChanged(TipoSolicitudAdminDto? value)
    {
        if (value is null) return;
        Id = value.Id;
        Nombre = value.Nombre;
        Descripcion = value.Descripcion ?? string.Empty;
        DiasRespuesta = value.DiasRespuesta;
        Prefijo = value.Prefijo;
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
        Seleccionado = null;
        Id = 0;
        Nombre = string.Empty;
        Descripcion = string.Empty;
        DiasRespuesta = 15;
        Prefijo = string.Empty;
        Activo = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var id = await _admin.GuardarTipoAsync(
                new TipoSolicitudAdminDto(Id, Nombre, Descripcion, DiasRespuesta, Prefijo, Activo), ActorId);
            await RecargarListaAsync(id);
        }, "Tipo de solicitud guardado.");
    }

    private async Task RecargarListaAsync(int? seleccionarId)
    {
        var lista = await _admin.ListarTiposAsync(ActorId);
        Tipos.Clear();
        foreach (var t in lista) Tipos.Add(t);

        if (seleccionarId is { } id)
        {
            Seleccionado = null;
            Seleccionado = Tipos.FirstOrDefault(t => t.Id == id);
        }
    }
}
