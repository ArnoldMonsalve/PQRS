using PQRS.Application.Dtos;

namespace PQRS.Application.Interfaces;

public interface IExportadorExcel
{
    /// <summary>Genera un libro .xlsx con una hoja de resumen y otra con el listado de solicitudes.</summary>
    void ExportarReporte(string ruta, DateTime? desde, DateTime? hasta,
        ResumenReporteDto resumen, IReadOnlyList<SolicitudResumenDto> listado);
}
