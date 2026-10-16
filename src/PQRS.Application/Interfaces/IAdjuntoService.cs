using PQRS.Application.Dtos;

namespace PQRS.Application.Interfaces;

public interface IAdjuntoService
{
    /// <summary>Guarda el archivo en el almacenamiento y lo asocia a la solicitud. Deja constancia en el historial.</summary>
    Task<AdjuntoDto> AgregarAsync(int solicitudId, string nombreArchivo, Stream contenido, int usuarioId, CancellationToken ct = default);

    /// <summary>Copia el archivo a una carpeta temporal y devuelve su ruta, para abrirlo sin riesgo de alterar el original.</summary>
    Task<string> PrepararParaAbrirAsync(int adjuntoId, CancellationToken ct = default);

    /// <summary>Quita el adjunto de la solicitud (el archivo se conserva en disco como respaldo).</summary>
    Task EliminarAsync(int adjuntoId, int usuarioId, CancellationToken ct = default);
}
