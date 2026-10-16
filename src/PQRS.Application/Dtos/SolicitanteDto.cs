namespace PQRS.Application.Dtos;

public record SolicitanteDto(
    string TipoDocumento,
    string NumeroDocumento,
    string Nombres,
    string Apellidos,
    string? Correo,
    string? Telefono,
    string? Direccion);
