namespace PQRS.Application.Dtos;

/// <summary>Porcentaje: participación sobre el total del grupo, de 0 a 100 (para dibujar barras).</summary>
public record ConteoDto(string Etiqueta, int Cantidad, double Porcentaje);

public record ResumenReporteDto(
    int Total,
    int Abiertas,
    int Vencidas,
    int Respondidas,
    int Anuladas,
    double? PromedioDiasRespuesta,
    double? PorcentajeATiempo,
    IReadOnlyList<ConteoDto> PorEstado,
    IReadOnlyList<ConteoDto> PorTipo,
    IReadOnlyList<ConteoDto> PorCategoria,
    IReadOnlyList<ConteoDto> PorArea);

public record AdjuntoDto(int Id, string NombreArchivo, long TamanoBytes, DateTime Fecha);
