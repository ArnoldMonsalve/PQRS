using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Desktop.Converters;
using PQRS.Domain.Enums;

namespace PQRS.Desktop.ViewModels;

public record OpcionEstado(string Texto, EstadoSolicitud? Valor);

public partial class BandejaSolicitudesViewModel : ObservableObject
{
    private readonly ISolicitudService _solicitudes;
    private readonly ICatalogoService _catalogo;

    public BandejaSolicitudesViewModel(ISolicitudService solicitudes, ICatalogoService catalogo)
    {
        _solicitudes = solicitudes;
        _catalogo = catalogo;

        Estados = [new OpcionEstado("Todos", null),
                   .. Enum.GetValues<EstadoSolicitud>().Select(e => new OpcionEstado(Textos.Estado(e), e))];
        EstadoFiltro = Estados[0];
    }

    /// <summary>Se dispara al abrir una solicitud (doble clic o botón); recibe el Id.</summary>
    public event Action<int>? SolicitudAbierta;

    public IReadOnlyList<OpcionEstado> Estados { get; }
    public ObservableCollection<CatalogoItemDto> Tipos { get; } = new();
    public ObservableCollection<SolicitudResumenDto> Solicitudes { get; } = new();

    [ObservableProperty] private string _texto = string.Empty;
    [ObservableProperty] private OpcionEstado? _estadoFiltro;
    [ObservableProperty] private CatalogoItemDto? _tipoFiltro;
    [ObservableProperty] private bool _soloVencidas;
    [ObservableProperty] private DateTime? _desde;
    [ObservableProperty] private DateTime? _hasta;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AbrirCommand))]
    private SolicitudResumenDto? _seleccionada;

    [ObservableProperty] private string _resumen = string.Empty;
    [ObservableProperty] private string _mensajeError = string.Empty;
    [ObservableProperty] private bool _estaOcupado;

    [RelayCommand]
    private async Task CargarAsync()
    {
        try
        {
            Tipos.Clear();
            Tipos.Add(new CatalogoItemDto(0, "Todos"));
            foreach (var t in await _catalogo.ObtenerTiposSolicitudAsync()) Tipos.Add(t);
            TipoFiltro = Tipos[0];
        }
        catch (Exception ex)
        {
            MensajeError = $"No se pudieron cargar los filtros: {ex.GetBaseException().Message}";
            return;
        }

        await BuscarAsync();
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        MensajeError = string.Empty;
        EstaOcupado = true;
        try
        {
            var filtro = new FiltroSolicitudes(
                Texto: Texto,
                Estado: EstadoFiltro?.Valor,
                TipoSolicitudId: TipoFiltro is { Id: > 0 } t ? t.Id : null,
                Desde: Desde,
                Hasta: Hasta,
                SoloVencidas: SoloVencidas ? true : null);

            var resultado = await _solicitudes.BuscarAsync(filtro);

            Solicitudes.Clear();
            foreach (var s in resultado) Solicitudes.Add(s);

            var vencidas = resultado.Count(s => s.Vencida);
            Resumen = resultado.Count switch
            {
                0 => "Sin resultados.",
                1 => "1 solicitud" + (vencidas > 0 ? $" ({vencidas} vencida)" : string.Empty),
                _ => $"{resultado.Count} solicitudes" + (vencidas > 0 ? $" ({vencidas} vencidas)" : string.Empty)
            };
        }
        catch (Exception ex)
        {
            MensajeError = $"No se pudo consultar: {ex.GetBaseException().Message}";
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    [RelayCommand]
    private async Task LimpiarFiltrosAsync()
    {
        Texto = string.Empty;
        EstadoFiltro = Estados[0];
        TipoFiltro = Tipos.FirstOrDefault();
        SoloVencidas = false;
        Desde = null;
        Hasta = null;
        await BuscarAsync();
    }

    [RelayCommand(CanExecute = nameof(HaySeleccion))]
    private void Abrir()
    {
        if (Seleccionada is not null) SolicitudAbierta?.Invoke(Seleccionada.Id);
    }

    private bool HaySeleccion() => Seleccionada is not null;
}
