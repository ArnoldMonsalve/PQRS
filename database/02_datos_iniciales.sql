/* ============================================================
   PQRS - Datos iniciales genéricos (editables según el ámbito)
   Guardar/ejecutar como UTF-8 para no dañar los acentos.
   ============================================================ */
USE PQRS;
GO

IF NOT EXISTS (SELECT 1 FROM Area)
BEGIN
    INSERT INTO Area (Nombre) VALUES
     (N'Atención al Usuario'),
     (N'Dirección General'),
     (N'Área Administrativa'),
     (N'Área Técnica');
END

IF NOT EXISTS (SELECT 1 FROM Categoria)
BEGIN
    INSERT INTO Categoria (Nombre, Descripcion, AreaPredeterminadaId) VALUES
     (N'Atención y servicio', N'Trato recibido por el personal',       (SELECT Id FROM Area WHERE Nombre = N'Atención al Usuario')),
     (N'Calidad del servicio', N'Fallas o demoras en el servicio',     (SELECT Id FROM Area WHERE Nombre = N'Área Técnica')),
     (N'Facturación y pagos',  N'Cobros, facturas y pagos',            (SELECT Id FROM Area WHERE Nombre = N'Área Administrativa')),
     (N'Información',          N'Solicitudes de información general', (SELECT Id FROM Area WHERE Nombre = N'Atención al Usuario')),
     (N'Otros',                N'Temas no clasificados',               (SELECT Id FROM Area WHERE Nombre = N'Atención al Usuario'));
END
GO
