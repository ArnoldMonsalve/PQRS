using System.Collections.ObjectModel;
using System.Net.Mail;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Enums;
using PQRS.Domain.Exceptions;

namespace PQRS.Desktop.ViewModels;

public partial class RadicarSolicitudViewModel : ObservableObject
{
    private readonly ISolicitudService _solicitudes;
    private readonly ICatalogoService _catalogo;
    private readonly ISesionActual _sesion;

    public RadicarSolicitudViewModel(ISolicitudService solicitudes, ICatalogoService catalogo, ISesionActual sesion)
    {
        _solicitudes = solicitudes;
        _catalogo = catalogo;
        _sesion = sesion;
    }

    public ObservableCollection<CatalogoItemDto> Tipos { get; } = new();
    public ObservableCollection<CatalogoItemDto> Categorias { get; } = new();
    public IReadOnlyList<CanalRecepcion> Canales { get; } = Enum.GetValues<CanalRecepcion>();
    public IReadOnlyList<Prioridad> Prioridades { get; } = Enum.GetValues<Prioridad>();
    public IReadOnlyList<string> TiposDocumento { get; } = ["CC", "TI", "CE", "NIT", "PAS", "Otro"];

    // ----- Solicitud -----
    [ObservableProperty] private CatalogoItemDto? _tipoSeleccionado;
    [ObservableProperty] private CatalogoItemDto? _categoriaSeleccionada;
    [ObservableProperty] private CanalRecepcion _canal = CanalRecepcion.Presencial;
    [ObservableProperty] private Prioridad _prioridad = Prioridad.Media;
    [ObservableProperty] private string _asunto = string.Empty;
    [ObservableProperty] private string _descripcion = string.Empty;

    // ----- Solicitante -----
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarSolicitante))]
    private bool _esAnonima;

    [ObservableProperty] private string _tipoDocumento = "CC";
    [ObservableProperty] private string _numeroDocumento = string.Empty;
    [ObservableProperty] private string _nombres = string.Empty;
    [ObservableProperty] private string _apellidos = string.Empty;
    [ObservableProperty] private string _correo = string.Empty;
    [ObservableProperty] private string _telefono = string.Empty;
    [ObservableProperty] private string _direccion = string.Empty;

    // ----- Estado de la pantalla -----
    [ObservableProperty] [NotifyCanExecuteChangedFor(nameof(RadicarCommand))] private bool _estaOcupado;
    [ObservableProperty] private string _mensajeError = string.Empty;
    [ObservableProperty] private string _mensajeExito = string.Empty;

    public bool MostrarSolicitante => !EsAnonima;

    [RelayCommand]
    private async Task CargarAsync()
    {
        try
        {
            foreach (var t in await _catalogo.ObtenerTiposSolicitudAsync()) Tipos.Add(t);
            foreach (var c in await _catalogo.ObtenerCategoriasAsync()) Categorias.Add(c);
            TipoSeleccionado = Tipos.FirstOrDefault();
            CategoriaSeleccionada = Categorias.FirstOrDefault();
        }
        catch (Exception ex)
        {
            MensajeError = $"No se pudieron cargar los catálogos: {ex.GetBaseException().Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeRadicar))]
    private async Task RadicarAsync()
    {
        MensajeError = string.Empty;
        MensajeExito = string.Empty;

        var error = Validar();
        if (error is not null)
        {
            MensajeError = error;
            return;
        }

        EstaOcupado = true;
        try
        {
            SolicitanteDto? solicitante = EsAnonima
                ? null
                : new SolicitanteDto(TipoDocumento, NumeroDocumento, Nombres, Apellidos,
                    Vacio(Correo), Vacio(Telefono), Vacio(Direccion));

            var creada = await _solicitudes.RadicarAsync(new CrearSolicitudRequest(
                TipoSeleccionado!.Id, CategoriaSeleccionada!.Id, Asunto, Descripcion,
                Canal, Prioridad, solicitante, _sesion.Usuario?.Id));

            MensajeExito = $"Solicitud radicada con el número {creada.Resumen.Radicado}. " +
                           $"Vence el {creada.Resumen.FechaVencimiento:dd/MM/yyyy}.";
            Limpiar();
        }
        catch (ReglaNegocioException ex)
        {
            MensajeError = ex.Message;
        }
        catch (Exception ex)
        {
            MensajeError = $"No se pudo radicar la solicitud: {ex.GetBaseException().Message}";
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    private bool PuedeRadicar() => !EstaOcupado;

    [RelayCommand]
    private void Limpiar()
    {
        Asunto = string.Empty;
        Descripcion = string.Empty;
        TipoDocumento = "CC";
        NumeroDocumento = string.Empty;
        Nombres = string.Empty;
        Apellidos = string.Empty;
        Correo = string.Empty;
        Telefono = string.Empty;
        Direccion = string.Empty;
        EsAnonima = false;
        Canal = CanalRecepcion.Presencial;
        Prioridad = Prioridad.Media;
        TipoSeleccionado = Tipos.FirstOrDefault();
        CategoriaSeleccionada = Categorias.FirstOrDefault();
    }

    private string? Validar()
    {
        if (TipoSeleccionado is null) return "Seleccione el tipo de solicitud.";
        if (CategoriaSeleccionada is null) return "Seleccione la categoría.";
        if (string.IsNullOrWhiteSpace(Asunto)) return "El asunto es obligatorio.";
        if (string.IsNullOrWhiteSpace(Descripcion)) return "La descripción es obligatoria.";

        if (!EsAnonima)
        {
            if (string.IsNullOrWhiteSpace(NumeroDocumento)) return "El número de documento del solicitante es obligatorio.";
            if (string.IsNullOrWhiteSpace(Nombres)) return "Los nombres del solicitante son obligatorios.";
            if (!string.IsNullOrWhiteSpace(Correo) && !CorreoValido(Correo)) return "El correo electrónico no es válido.";
        }
        return null;
    }

    private static bool CorreoValido(string correo) => MailAddress.TryCreate(correo.Trim(), out _);
    private static string? Vacio(string valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
