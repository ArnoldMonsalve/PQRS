using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Desktop.Services;
using PQRS.Domain.Enums;
using PQRS.Domain.Exceptions;
using PQRS.Domain.Services;

namespace PQRS.Desktop.ViewModels;

public partial class DetalleSolicitudViewModel : ObservableObject
{
    private readonly ISolicitudService _solicitudes;
    private readonly ICatalogoService _catalogo;
    private readonly ISesionActual _sesion;
    private readonly IAdjuntoService _adjuntos;
    private readonly IDialogosArchivos _dialogos;

    public DetalleSolicitudViewModel(ISolicitudService solicitudes, ICatalogoService catalogo, ISesionActual sesion,
        IAdjuntoService adjuntos, IDialogosArchivos dialogos)
    {
        _solicitudes = solicitudes;
        _catalogo = catalogo;
        _sesion = sesion;
        _adjuntos = adjuntos;
        _dialogos = dialogos;
    }

    public event Action? Volver;

    public ObservableCollection<CatalogoItemDto> Responsables { get; } = new();
    public ObservableCollection<EstadoSolicitud> EstadosSiguientes { get; } = new();
    public ObservableCollection<AdjuntoDto> Adjuntos { get; } = new();

    [ObservableProperty] private AdjuntoDto? _adjuntoSeleccionado;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeAsignar), nameof(PuedeGestionar), nameof(PuedeResponder),
        nameof(PuedeComentar), nameof(PuedeCambiarEstado), nameof(EsAnonima), nameof(SinRespuestas))]
    private SolicitudDetalleDto? _detalle;

    [ObservableProperty] private CatalogoItemDto? _responsableSeleccionado;
    [ObservableProperty] private EstadoSolicitud? _estadoSeleccionado;
    [ObservableProperty] private string _comentarioEstado = string.Empty;
    [ObservableProperty] private string _textoRespuesta = string.Empty;
    [ObservableProperty] private string _textoComentario = string.Empty;
    [ObservableProperty] private bool _comentarioInterno;

    [ObservableProperty] private string _mensajeError = string.Empty;
    [ObservableProperty] private string _mensajeExito = string.Empty;
    [ObservableProperty] private bool _estaOcupado;

    // ----- Qué puede hacer el usuario según su rol y el estado de la solicitud -----
    private bool EsFinal => Detalle is not null && TransicionesEstado.EsEstadoFinal(Detalle.Resumen.Estado);

    public bool PuedeAsignar => Detalle is not null && !EsFinal
        && _sesion.TieneRol(RolUsuario.Administrador, RolUsuario.Supervisor);

    public bool PuedeGestionar => Detalle is not null && !EsFinal
        && _sesion.TieneRol(RolUsuario.Administrador, RolUsuario.Supervisor, RolUsuario.Responsable);

    public bool PuedeResponder => PuedeGestionar
        && TransicionesEstado.EsPermitida(Detalle!.Resumen.Estado, EstadoSolicitud.Respondida);

    public bool PuedeCambiarEstado => PuedeGestionar && EstadosSiguientes.Count > 0;
    public bool PuedeComentar => Detalle is not null && !EsFinal;
    public bool EsAnonima => Detalle is not null && Detalle.Solicitante is null;
    public bool SinRespuestas => Detalle is { Respuestas.Count: 0 };

    public async Task CargarAsync(int solicitudId)
    {
        try
        {
            if (Responsables.Count == 0)
                foreach (var r in await _catalogo.ObtenerResponsablesAsync()) Responsables.Add(r);

            await RecargarAsync(solicitudId);
        }
        catch (Exception ex)
        {
            MensajeError = $"No se pudo cargar la solicitud: {ex.GetBaseException().Message}";
        }
    }

    private async Task RecargarAsync(int solicitudId)
    {
        Detalle = await _solicitudes.ObtenerPorIdAsync(solicitudId)
            ?? throw new ReglaNegocioException("La solicitud ya no existe.");

        EstadosSiguientes.Clear();
        foreach (var e in TransicionesEstado.Siguientes(Detalle.Resumen.Estado)
                     .Where(e => e != EstadoSolicitud.Respondida))   // Respondida solo se alcanza al responder
            EstadosSiguientes.Add(e);

        EstadoSeleccionado = EstadosSiguientes.Cast<EstadoSolicitud?>().FirstOrDefault();
        ResponsableSeleccionado = null;

        Adjuntos.Clear();
        foreach (var a in Detalle.Adjuntos) Adjuntos.Add(a);
        AdjuntoSeleccionado = null;

        OnPropertyChanged(nameof(PuedeCambiarEstado));
        OnPropertyChanged(nameof(PuedeResponder));
    }

    [RelayCommand]
    private void IrAtras() => Volver?.Invoke();

    [RelayCommand]
    private Task AsignarAsync() => EjecutarAsync(
        () =>
        {
            if (ResponsableSeleccionado is null) throw new ReglaNegocioException("Seleccione un responsable.");
            return _solicitudes.AsignarAsync(Detalle!.Resumen.Id, ResponsableSeleccionado.Id, UsuarioId);
        },
        "Responsable asignado.");

    [RelayCommand]
    private Task CambiarEstadoAsync() => EjecutarAsync(
        async () =>
        {
            if (EstadoSeleccionado is null) throw new ReglaNegocioException("Seleccione el nuevo estado.");
            await _solicitudes.CambiarEstadoAsync(Detalle!.Resumen.Id, EstadoSeleccionado.Value, ComentarioEstado, UsuarioId);
            ComentarioEstado = string.Empty;
        },
        "Estado actualizado.");

    [RelayCommand]
    private Task ResponderAsync() => EjecutarAsync(
        async () =>
        {
            await _solicitudes.ResponderAsync(Detalle!.Resumen.Id, TextoRespuesta, UsuarioId);
            TextoRespuesta = string.Empty;
        },
        "Respuesta registrada.");

    [RelayCommand]
    private Task ComentarAsync() => EjecutarAsync(
        async () =>
        {
            await _solicitudes.ComentarAsync(Detalle!.Resumen.Id, TextoComentario, UsuarioId, ComentarioInterno);
            TextoComentario = string.Empty;
        },
        "Comentario agregado.");

    [RelayCommand]
    private async Task AgregarAdjuntoAsync()
    {
        var ruta = _dialogos.ElegirArchivo("Seleccione el archivo a adjuntar");
        if (ruta is null) return;

        await EjecutarAsync(async () =>
        {
            await using var archivo = File.OpenRead(ruta);
            await _adjuntos.AgregarAsync(Detalle!.Resumen.Id, Path.GetFileName(ruta), archivo, UsuarioId);
        }, "Archivo adjuntado.");
    }

    [RelayCommand]
    private async Task AbrirAdjuntoAsync()
    {
        if (AdjuntoSeleccionado is null)
        {
            MensajeError = "Seleccione un adjunto de la lista.";
            return;
        }

        var id = AdjuntoSeleccionado.Id;
        await EjecutarAsync(async () => _dialogos.Abrir(await _adjuntos.PrepararParaAbrirAsync(id)), recargar: false);
    }

    [RelayCommand]
    private async Task EliminarAdjuntoAsync()
    {
        if (AdjuntoSeleccionado is null)
        {
            MensajeError = "Seleccione un adjunto de la lista.";
            return;
        }

        var id = AdjuntoSeleccionado.Id;
        await EjecutarAsync(() => _adjuntos.EliminarAsync(id, UsuarioId), "Adjunto eliminado.");
    }

    private int UsuarioId => _sesion.Usuario?.Id ?? throw new ReglaNegocioException("No hay una sesión activa.");

    private async Task EjecutarAsync(Func<Task> accion, string? mensajeExito = null, bool recargar = true)
    {
        MensajeError = string.Empty;
        MensajeExito = string.Empty;
        EstaOcupado = true;
        try
        {
            await accion();
            if (recargar) await RecargarAsync(Detalle!.Resumen.Id);
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
