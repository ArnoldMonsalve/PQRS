using ClosedXML.Excel;
using PQRS.Application.Dtos;
using PQRS.Application.Interfaces;
using PQRS.Domain.Enums;

namespace PQRS.Infrastructure.Services;

public class ExportadorExcel : IExportadorExcel
{
    public void ExportarReporte(string ruta, DateTime? desde, DateTime? hasta,
        ResumenReporteDto resumen, IReadOnlyList<SolicitudResumenDto> listado)
    {
        using var libro = new XLWorkbook();
        EscribirResumen(libro.Worksheets.Add("Resumen"), desde, hasta, resumen);
        EscribirListado(libro.Worksheets.Add("Solicitudes"), listado);
        libro.SaveAs(ruta);
    }

    private static void EscribirResumen(IXLWorksheet hoja, DateTime? desde, DateTime? hasta, ResumenReporteDto r)
    {
        hoja.Cell(1, 1).Value = "Reporte de PQRS";
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 16;

        hoja.Cell(2, 1).Value = "Periodo";
        hoja.Cell(2, 2).Value = $"{desde?.ToString("dd/MM/yyyy") ?? "inicio"} al {hasta?.ToString("dd/MM/yyyy") ?? "hoy"}";
        hoja.Cell(3, 1).Value = "Generado";
        hoja.Cell(3, 2).Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        var fila = 5;
        void Indicador(string nombre, string valor)
        {
            hoja.Cell(fila, 1).Value = nombre;
            hoja.Cell(fila, 2).Value = valor;
            fila++;
        }

        Indicador("Total de solicitudes", r.Total.ToString());
        Indicador("Abiertas", r.Abiertas.ToString());
        Indicador("Vencidas (abiertas fuera de plazo)", r.Vencidas.ToString());
        Indicador("Respondidas o cerradas", r.Respondidas.ToString());
        Indicador("Anuladas", r.Anuladas.ToString());
        Indicador("Promedio de días hasta la respuesta", r.PromedioDiasRespuesta?.ToString("0.0") ?? "—");
        Indicador("Respondidas a tiempo", r.PorcentajeATiempo is { } p ? $"{p:0.0}%" : "—");

        fila++;
        fila = EscribirConteos(hoja, fila, "Por estado", r.PorEstado);
        fila = EscribirConteos(hoja, fila, "Por tipo", r.PorTipo);
        fila = EscribirConteos(hoja, fila, "Por categoría", r.PorCategoria);
        EscribirConteos(hoja, fila, "Por área", r.PorArea);

        hoja.Column(1).AdjustToContents();
        hoja.Column(2).AdjustToContents();
        hoja.Column(3).AdjustToContents();
    }

    private static int EscribirConteos(IXLWorksheet hoja, int fila, string titulo, IReadOnlyList<ConteoDto> conteos)
    {
        hoja.Cell(fila, 1).Value = titulo;
        hoja.Cell(fila, 2).Value = "Cantidad";
        hoja.Cell(fila, 3).Value = "%";
        hoja.Range(fila, 1, fila, 3).Style.Font.Bold = true;
        hoja.Range(fila, 1, fila, 3).Style.Fill.BackgroundColor = XLColor.FromHtml("#E8ECF2");
        fila++;

        foreach (var c in conteos)
        {
            hoja.Cell(fila, 1).Value = c.Etiqueta;
            hoja.Cell(fila, 2).Value = c.Cantidad;
            hoja.Cell(fila, 3).Value = c.Porcentaje / 100;
            hoja.Cell(fila, 3).Style.NumberFormat.Format = "0.0%";
            fila++;
        }
        return fila + 1;
    }

    private static void EscribirListado(IXLWorksheet hoja, IReadOnlyList<SolicitudResumenDto> listado)
    {
        string[] encabezados = ["Radicado", "Tipo", "Categoría", "Área", "Asunto", "Solicitante",
                                "Estado", "Prioridad", "Radicada", "Vence", "Vencida"];
        for (var i = 0; i < encabezados.Length; i++)
            hoja.Cell(1, i + 1).Value = encabezados[i];

        var cabecera = hoja.Range(1, 1, 1, encabezados.Length);
        cabecera.Style.Font.Bold = true;
        cabecera.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F3A5F");
        cabecera.Style.Font.FontColor = XLColor.White;

        for (var i = 0; i < listado.Count; i++)
        {
            var s = listado[i];
            var fila = i + 2;
            hoja.Cell(fila, 1).Value = s.Radicado;
            hoja.Cell(fila, 2).Value = s.Tipo;
            hoja.Cell(fila, 3).Value = s.Categoria;
            hoja.Cell(fila, 4).Value = s.Area ?? "Sin asignar";
            hoja.Cell(fila, 5).Value = s.Asunto;
            hoja.Cell(fila, 6).Value = s.Solicitante;
            hoja.Cell(fila, 7).Value = s.Estado.ATexto();
            hoja.Cell(fila, 8).Value = s.Prioridad.ToString();
            hoja.Cell(fila, 9).Value = s.FechaRadicacion;
            hoja.Cell(fila, 10).Value = s.FechaVencimiento;
            hoja.Cell(fila, 11).Value = s.Vencida ? "Sí" : "No";
        }

        hoja.Column(9).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
        hoja.Column(10).Style.DateFormat.Format = "dd/MM/yyyy";
        hoja.SheetView.FreezeRows(1);
        if (listado.Count > 0) hoja.Range(1, 1, listado.Count + 1, encabezados.Length).SetAutoFilter();

        hoja.Columns().AdjustToContents(1, Math.Min(listado.Count + 1, 200));
        hoja.Column(5).Width = Math.Min(hoja.Column(5).Width, 60);
    }
}
