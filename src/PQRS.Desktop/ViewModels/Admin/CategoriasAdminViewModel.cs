using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;

namespace PQRS.Desktop.ViewModels.Admin;

public partial class CategoriasAdminViewModel : AdminViewModelBase
{
    private readonly ICatalogoAdminService _admin;
    private readonly ICatalogoService _catalogo;

    public CategoriasAdminViewModel(ICatalogoAdminService admin, ICatalogoService catalogo, ISesionActual sesion)
        : base(sesion)
    {
        _admin = admin;
        _catalogo = catalogo;
    }

    public ObservableCollection<CategoriaAdminDto> Categorias { get; } = new();
    public ObservableCollection<CatalogoItemDto> Areas { get; } = new();

    [ObservableProperty] private CategoriaAdminDto? _seleccionada;
    [ObservableProperty] private int _id;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private string _descripcion = string.Empty;
    [ObservableProperty] private CatalogoItemDto? _areaSeleccionada;
    [ObservableProperty] private bool _activo = true;

    partial void OnSeleccionadaChanged(CategoriaAdminDto? value)
    {
        if (value is null) return;
        Id = value.Id;
        Nombre = value.Nombre;
        Descripcion = value.Descripcion ?? string.Empty;
        AreaSeleccionada = Areas.FirstOrDefault(a => a.Id == (value.AreaPredeterminadaId ?? 0)) ?? Areas.FirstOrDefault();
        Activo = value.Activo;
    }

    [RelayCommand]
    private async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            Areas.Clear();
            Areas.Add(new CatalogoItemDto(0, "Sin área predeterminada"));
            foreach (var a in await _catalogo.ObtenerAreasAsync()) Areas.Add(a);

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
        Descripcion = string.Empty;
        AreaSeleccionada = Areas.FirstOrDefault();
        Activo = true;
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var areaId = AreaSeleccionada is { Id: > 0 } a ? a.Id : (int?)null;
            var id = await _admin.GuardarCategoriaAsync(
                new CategoriaAdminDto(Id, Nombre, Descripcion, areaId, null, Activo), ActorId);
            await RecargarListaAsync(id);
        }, "Categoría guardada.");
    }

    private async Task RecargarListaAsync(int? seleccionarId)
    {
        var lista = await _admin.ListarCategoriasAsync(ActorId);
        Categorias.Clear();
        foreach (var c in lista) Categorias.Add(c);

        if (seleccionarId is { } id)
        {
            Seleccionada = null;
            Seleccionada = Categorias.FirstOrDefault(c => c.Id == id);
        }
    }
}
