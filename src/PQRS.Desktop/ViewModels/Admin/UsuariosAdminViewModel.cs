using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Enums;

namespace PQRS.Desktop.ViewModels.Admin;

public partial class UsuariosAdminViewModel : AdminViewModelBase
{
    private readonly IUsuarioAdminService _usuarios;
    private readonly ICatalogoService _catalogo;

    public UsuariosAdminViewModel(IUsuarioAdminService usuarios, ICatalogoService catalogo, ISesionActual sesion)
        : base(sesion)
    {
        _usuarios = usuarios;
        _catalogo = catalogo;
    }

    /// <summary>La vista lo escucha para vaciar el PasswordBox (su texto no se enlaza).</summary>
    public event Action? ClaveLimpiada;

    public IReadOnlyList<RolUsuario> Roles { get; } = Enum.GetValues<RolUsuario>();
    public ObservableCollection<CatalogoItemDto> Areas { get; } = new();
    public ObservableCollection<UsuarioAdminDto> Usuarios { get; } = new();

    [ObservableProperty] private UsuarioAdminDto? _seleccionado;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EsNuevo), nameof(AyudaClave))]
    private int _id;

    [ObservableProperty] private string _nombreCompleto = string.Empty;
    [ObservableProperty] private string _nombreUsuario = string.Empty;
    [ObservableProperty] private string _correo = string.Empty;
    [ObservableProperty] private RolUsuario _rol = RolUsuario.Radicador;
    [ObservableProperty] private CatalogoItemDto? _areaSeleccionada;
    [ObservableProperty] private bool _activo = true;
    [ObservableProperty] private string _clave = string.Empty;

    public bool EsNuevo => Id == 0;
    public string AyudaClave => EsNuevo
        ? "Obligatoria. Mínimo 8 caracteres, con letras y números."
        : "Déjela vacía para conservar la actual.";

    partial void OnSeleccionadoChanged(UsuarioAdminDto? value)
    {
        if (value is null) return;

        Id = value.Id;
        NombreCompleto = value.NombreCompleto;
        NombreUsuario = value.NombreUsuario;
        Correo = value.Correo;
        Rol = value.Rol;
        AreaSeleccionada = Areas.FirstOrDefault(a => a.Id == (value.AreaId ?? 0)) ?? Areas.FirstOrDefault();
        Activo = value.Activo;
        LimpiarClave();
    }

    [RelayCommand]
    private async Task CargarAsync()
    {
        await EjecutarAsync(async () =>
        {
            Areas.Clear();
            Areas.Add(new CatalogoItemDto(0, "Sin área"));
            foreach (var a in await _catalogo.ObtenerAreasAsync()) Areas.Add(a);

            await RecargarListaAsync(seleccionarId: null);
            Nuevo();
        });
    }

    [RelayCommand]
    private void Nuevo()
    {
        Seleccionado = null;
        Id = 0;
        NombreCompleto = string.Empty;
        NombreUsuario = string.Empty;
        Correo = string.Empty;
        Rol = RolUsuario.Radicador;
        AreaSeleccionada = Areas.FirstOrDefault();
        Activo = true;
        LimpiarClave();
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        await EjecutarAsync(async () =>
        {
            var areaId = AreaSeleccionada is { Id: > 0 } a ? a.Id : (int?)null;
            var id = await _usuarios.GuardarAsync(new GuardarUsuarioRequest(
                Id, NombreCompleto, NombreUsuario, Correo, Rol, areaId, Activo,
                string.IsNullOrEmpty(Clave) ? null : Clave), ActorId);

            await RecargarListaAsync(seleccionarId: id);
        }, "Usuario guardado.");
    }

    private async Task RecargarListaAsync(int? seleccionarId)
    {
        var lista = await _usuarios.ListarAsync(ActorId);
        Usuarios.Clear();
        foreach (var u in lista) Usuarios.Add(u);

        if (seleccionarId is { } id)
        {
            Seleccionado = null;   // fuerza a releer los datos guardados en el formulario
            Seleccionado = Usuarios.FirstOrDefault(u => u.Id == id);
        }
    }

    private void LimpiarClave()
    {
        Clave = string.Empty;
        ClaveLimpiada?.Invoke();
    }
}
