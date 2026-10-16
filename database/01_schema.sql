/* ============================================================
   PQRS - Esquema de base de datos (SQL Server)
   ============================================================ */
IF DB_ID('PQRS') IS NULL CREATE DATABASE PQRS;
GO
USE PQRS;
GO

CREATE TABLE Area (
    Id                 INT IDENTITY PRIMARY KEY,
    Nombre             NVARCHAR(150) NOT NULL,
    CorreoNotificacion NVARCHAR(200) NULL,
    Activo             BIT NOT NULL DEFAULT 1,
    FechaCreacion      DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE TipoSolicitud (
    Id            INT IDENTITY PRIMARY KEY,
    Nombre        NVARCHAR(100) NOT NULL,
    Descripcion   NVARCHAR(300) NULL,
    DiasRespuesta INT NOT NULL DEFAULT 15,
    Prefijo       NVARCHAR(5) NOT NULL,
    Activo        BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Categoria (
    Id                   INT IDENTITY PRIMARY KEY,
    Nombre               NVARCHAR(150) NOT NULL,
    Descripcion          NVARCHAR(300) NULL,
    AreaPredeterminadaId INT NULL REFERENCES Area(Id),
    Activo               BIT NOT NULL DEFAULT 1,
    FechaCreacion        DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Usuario (
    Id             INT IDENTITY PRIMARY KEY,
    NombreCompleto NVARCHAR(200) NOT NULL,
    NombreUsuario  NVARCHAR(50)  NOT NULL UNIQUE,
    Correo         NVARCHAR(200) NOT NULL,
    PasswordHash   NVARCHAR(500) NOT NULL,
    Rol            INT NOT NULL,           -- 1 Admin, 2 Radicador, 3 Responsable, 4 Supervisor
    AreaId         INT NULL REFERENCES Area(Id),
    Activo         BIT NOT NULL DEFAULT 1,
    FechaCreacion  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Solicitante (
    Id              INT IDENTITY PRIMARY KEY,
    TipoDocumento   NVARCHAR(10)  NOT NULL DEFAULT 'CC',
    NumeroDocumento NVARCHAR(30)  NOT NULL,
    Nombres         NVARCHAR(100) NOT NULL,
    Apellidos       NVARCHAR(100) NOT NULL,
    Correo          NVARCHAR(200) NULL,
    Telefono        NVARCHAR(30)  NULL,
    Direccion       NVARCHAR(250) NULL,
    Activo          BIT NOT NULL DEFAULT 1,
    FechaCreacion   DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    CONSTRAINT UQ_Solicitante_Doc UNIQUE (TipoDocumento, NumeroDocumento)
);

CREATE TABLE Solicitud (
    Id               INT IDENTITY PRIMARY KEY,
    Radicado         NVARCHAR(30) NOT NULL UNIQUE,
    TipoSolicitudId  INT NOT NULL REFERENCES TipoSolicitud(Id),
    CategoriaId      INT NOT NULL REFERENCES Categoria(Id),
    AreaId           INT NULL REFERENCES Area(Id),
    SolicitanteId    INT NULL REFERENCES Solicitante(Id),   -- NULL = anónima
    ResponsableId    INT NULL REFERENCES Usuario(Id),
    Asunto           NVARCHAR(250) NOT NULL,
    Descripcion      NVARCHAR(MAX) NOT NULL,
    Canal            INT NOT NULL,
    Prioridad        INT NOT NULL DEFAULT 2,
    Estado           INT NOT NULL DEFAULT 1,
    FechaRadicacion  DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    FechaVencimiento DATETIME2 NOT NULL,
    FechaCierre      DATETIME2 NULL,
    Activo           BIT NOT NULL DEFAULT 1,
    FechaCreacion    DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);
CREATE INDEX IX_Solicitud_Estado  ON Solicitud(Estado, FechaVencimiento);
CREATE INDEX IX_Solicitud_Solicitante ON Solicitud(SolicitanteId);

CREATE TABLE Seguimiento (
    Id             INT IDENTITY PRIMARY KEY,
    SolicitudId    INT NOT NULL REFERENCES Solicitud(Id),
    UsuarioId      INT NULL REFERENCES Usuario(Id),
    EstadoAnterior INT NULL,
    EstadoNuevo    INT NOT NULL,
    Comentario     NVARCHAR(MAX) NOT NULL,
    EsInterno      BIT NOT NULL DEFAULT 0,
    Activo         BIT NOT NULL DEFAULT 1,
    FechaCreacion  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Respuesta (
    Id             INT IDENTITY PRIMARY KEY,
    SolicitudId    INT NOT NULL REFERENCES Solicitud(Id),
    UsuarioId      INT NOT NULL REFERENCES Usuario(Id),
    Contenido      NVARCHAR(MAX) NOT NULL,
    FechaRespuesta DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    Activo         BIT NOT NULL DEFAULT 1,
    FechaCreacion  DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Adjunto (
    Id            INT IDENTITY PRIMARY KEY,
    SolicitudId   INT NOT NULL REFERENCES Solicitud(Id),
    NombreArchivo NVARCHAR(260) NOT NULL,
    RutaArchivo   NVARCHAR(500) NOT NULL,
    TipoContenido NVARCHAR(100) NULL,
    TamanoBytes   BIGINT NOT NULL,
    Activo        BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE TABLE Parametro (
    Id            INT IDENTITY PRIMARY KEY,
    Clave         NVARCHAR(100) NOT NULL UNIQUE,
    Valor         NVARCHAR(500) NOT NULL,
    Descripcion   NVARCHAR(300) NULL,
    Activo        BIT NOT NULL DEFAULT 1,
    FechaCreacion DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

/* Datos iniciales genéricos */
INSERT INTO TipoSolicitud (Nombre, Descripcion, DiasRespuesta, Prefijo) VALUES
 ('Petición',   'Solicitud de información o servicio',          15, 'P'),
 ('Queja',      'Inconformidad con la atención de una persona', 15, 'Q'),
 ('Reclamo',    'Inconformidad con un producto o servicio',     15, 'R'),
 ('Sugerencia', 'Propuesta de mejora',                          30, 'S');

INSERT INTO Parametro (Clave, Valor, Descripcion) VALUES
 ('NombreOrganizacion', 'Mi Organización', 'Nombre que se muestra en la aplicación y reportes'),
 ('CorreoRemitente',    '',                'Correo desde el que se envían notificaciones');
GO
