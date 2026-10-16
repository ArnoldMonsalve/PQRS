# Contexto del proyecto PQRS

Documento para dar contexto a una IA (o a una persona nueva) sobre cómo está construido el sistema. Última actualización: 2026-10-16.

## 1. Qué es

Aplicación de **escritorio para Windows** que gestiona **PQRS** (Peticiones, Quejas, Reclamos y Sugerencias). Está pensada como sistema **genérico**: sirve para cualquier organización, porque los tipos de solicitud, las categorías, las áreas y el nombre de la organización se configuran desde la propia aplicación, sin tocar código.

Funciones actuales:
- Login con usuarios y roles.
- Radicar solicitudes (con solicitante identificado o anónimo) y generar un número de radicado.
- Bandeja con filtros y semáforo de vencidas; detalle con historial completo.
- Asignar responsable, cambiar estado, responder, comentar (comentarios internos o visibles).
- Adjuntar archivos a una solicitud.
- Reportes con indicadores y exportación a Excel.
- Administración de usuarios, tipos, categorías, áreas y datos de la organización.

## 2. Tecnologías

| Tema | Elección |
|---|---|
| Lenguaje / plataforma | C# 14, **.NET 10** |
| Interfaz | **WPF** con patrón **MVVM** |
| MVVM | `CommunityToolkit.Mvvm` 8.x (`[ObservableProperty]`, `[RelayCommand]`) |
| Acceso a datos | **Entity Framework Core 10** (`Microsoft.EntityFrameworkCore.SqlServer`) |
| Base de datos | **SQL Server 2019** (instancia en el servidor `MILU`, base `PQRS`) |
| Inyección de dependencias | `Microsoft.Extensions.Hosting` (Generic Host) |
| Excel | `ClosedXML` |
| Hash de contraseñas | PBKDF2-SHA256 (clase estándar de .NET, sin paquetes) |
| IDE | Visual Studio 2026; la solución usa el formato nuevo `PQRS.slnx` |

Nota: `System.Security.Cryptography.Pkcs` está fijado como referencia directa en Infrastructure para evitar una versión transitiva con vulnerabilidades conocidas.

## 3. Estructura de la solución

```
C:\WORKSPACE\PQRS\
├── PQRS.slnx
├── context.md                  (este archivo)
├── installer\                  instalador (Inno Setup): PQRS.iss, build-installer.ps1, README.md
├── database\
│   ├── 01_schema.sql           esquema completo + tipos de solicitud y parámetros iniciales
│   └── 02_datos_iniciales.sql  áreas y categorías de ejemplo
└── src\
    ├── PQRS.Domain\            entidades, enums, reglas puras (sin dependencias)
    ├── PQRS.Application\       DTOs e interfaces de los servicios (sin EF)
    ├── PQRS.Infrastructure\    EF Core, implementaciones de servicios, seguridad, Excel
    └── PQRS.Desktop\           WPF: vistas, ViewModels, arranque (Composition Root)
```

### Dependencias entre proyectos
`Desktop → Application + Infrastructure`, `Infrastructure → Application + Domain`, `Application → Domain`. Domain no depende de nadie.

### Qué hay en cada capa

**PQRS.Domain**
- `Common/EntidadBase` — `Id`, `Activo`, `FechaCreacion` (todas las entidades la heredan).
- `Entities/` — `Solicitud`, `Solicitante`, `Seguimiento`, `Respuesta`, `Adjunto`, `TipoSolicitud`, `Categoria`, `Area`, `Usuario`, `Parametro`.
- `Enums/` — `EstadoSolicitud`, `CanalRecepcion`, `Prioridad`, `RolUsuario`; `EstadoSolicitudTexto.ATexto()` da el nombre con tildes ("En trámite").
- `Services/` — `TransicionesEstado` (flujo permitido entre estados), `CalculadoraVencimiento` (suma días hábiles lunes a viernes, **sin festivos todavía**), `PoliticaClaves` (mínimo 8 caracteres, letras y números).
- `Exceptions/ReglaNegocioException` — errores de negocio con mensaje apto para el usuario.

**PQRS.Application**
- `Interfaces/` — contratos: `ISolicitudService`, `ICatalogoService`, `IAuthService`, `ISesionActual`, `IPasswordHasher`, `IUsuarioAdminService`, `ICatalogoAdminService`, `IAdjuntoService`, `IReporteService`, `IExportadorExcel`.
- `Dtos/` — registros inmutables que viajan entre capas.
- `Services/SesionActual` — guarda el usuario con sesión abierta (singleton).

**PQRS.Infrastructure**
- `Persistence/PqrsDbContext` — mapea cada entidad a la tabla con el **nombre de la clase en singular** (`Solicitud`, no `Solicitudes`); borrado en cascada desactivado (`Restrict`).
- `Persistence/DbInitializer` — crea el administrador inicial si la tabla `Usuario` está vacía.
- `Services/` — implementaciones de los contratos anteriores. `Permisos` valida roles contra la base de datos.
- `Security/PasswordHasher` — formato guardado `v1.iteraciones.sal.hash`.
- `DependencyInjection.AddInfrastructure(...)` — registra todo.

**PQRS.Desktop**
- `App.xaml.cs` — construye el Host, registra ViewModels y ventanas, ejecuta `DbInitializer`, muestra el login y luego `MainWindow`.
- `ViewModels/` y `Views/` — un par por pantalla. `ViewModels/Admin` y `Views/Admin` para administración.
- `App.xaml` — contiene los `DataTemplate` que enlazan cada ViewModel con su vista y los estilos compartidos.
- `Services/DialogosArchivos` — abrir/guardar archivos y abrir documentos (aislado para que los ViewModels no dependan de WPF).
- `Converters/` — `EstadoTextoConverter`, `TamanoArchivoConverter`.

## 4. Modelo de datos

Tablas: `Area`, `TipoSolicitud`, `Categoria`, `Usuario`, `Solicitante`, `Solicitud`, `Seguimiento`, `Respuesta`, `Adjunto`, `Parametro`. Todas con `Id`, `Activo`, `FechaCreacion`.

- **Solicitud**: `Radicado` único (formato `{Prefijo}-{año}-{consecutivo de 6 dígitos}`, ej. `P-2026-000001`), tipo, categoría, área, solicitante (**null = anónima**), responsable, asunto, descripción, canal, prioridad, estado, `FechaRadicacion`, `FechaVencimiento`, `FechaCierre`.
- **Seguimiento**: historial de cada cambio o comentario (estado anterior/nuevo, usuario, texto, `EsInterno`). Es la trazabilidad de la solicitud.
- **Respuesta**: respuesta oficial entregada al solicitante.
- **Adjunto**: metadatos del archivo; el archivo vive en disco.
- **Parametro**: clave/valor. Claves editables: `NombreOrganizacion`, `CorreoRemitente`.
- Los enums se guardan como `INT`.

### Estados y flujo
`Radicada → EnTramite → (PendienteInformacion ↔ EnTramite) → Respondida → Cerrada`. Se puede `Anular` desde Radicada, EnTramite o PendienteInformacion. Desde Respondida se puede volver a EnTramite (reabrir). `Cerrada` y `Anulada` son finales y no admiten cambios. Se llega a `Respondida` **solo registrando una respuesta**, no desde "cambiar estado".

### Roles
| Rol | Puede |
|---|---|
| Administrador | Todo, incluida la administración y los reportes |
| Supervisor | Asignar, gestionar, ver reportes |
| Responsable | Cambiar estado, responder, comentar, eliminar adjuntos |
| Radicador | Radicar, ver, comentar y adjuntar |

Los permisos se validan **en los servicios consultando la base de datos**, no solo ocultando botones en la interfaz.

## 5. Reglas de negocio importantes

- **Radicado**: se genera en `SolicitudService.SiguienteRadicadoAsync` buscando el último con el mismo prefijo y año. Hay índice único; si dos usuarios chocan se reintenta hasta 3 veces.
- **Vencimiento**: `FechaRadicacion` + `DiasRespuesta` del tipo, en días hábiles (sin festivos).
- **Vencida** (`EstaVencida`, propiedad calculada, no se guarda): abierta y con fecha de vencimiento pasada.
- **Solicitante**: se identifica por tipo + número de documento; si ya existe se reutiliza y se actualizan sus datos de contacto.
- **Usuarios**: no se puede desactivar ni quitar el rol de administrador a uno mismo, ni dejar el sistema sin administrador activo. El usuario es único.
- **Catálogos**: no se borran, se **desactivan** (para conservar el historial). Nombres únicos; el prefijo de tipo es único (1–5 letras o números).
- **Adjuntos**: lista blanca de extensiones (pdf, doc/docx, xls/xlsx, ppt/pptx, txt, csv, jpg/jpeg, png, gif, zip), máximo 10 MB y 20 por solicitud. El nombre en disco lo genera el sistema (GUID) dentro de `{solicitudId}\`. Eliminar = desactivar (el archivo se conserva). Abrir = copiar a una carpeta temporal y abrir la copia.
- **Reportes**: solo Administrador y Supervisor. "A tiempo" = primera respuesta a más tardar el día de vencimiento.

## 6. Configuración (`src/PQRS.Desktop/appsettings.json`)

| Clave | Para qué |
|---|---|
| `ConnectionStrings:PqrsDb` | Cadena de conexión a SQL Server (usa un usuario de SQL; la clave está en este archivo, no se repite aquí) |
| `Almacenamiento:RutaAdjuntos` | Carpeta de adjuntos. Vacía = `C:\ProgramData\PQRS\Adjuntos`. **Con varios equipos debe ser una ruta de red compartida** |
| `Seed:AdminUsuario` / `AdminClave` / `AdminCorreo` | Datos del administrador inicial. Solo se usan cuando no existe ningún usuario; cambiarlos después no modifica al administrador existente |

El archivo se copia a la carpeta de salida (`CopyToOutputDirectory`). **Pendiente de seguridad**: no dejar claves reales en este archivo cuando se publique; mover a User Secrets o variables de entorno.

## 7. Cómo funciona el arranque

1. `App.OnStartup` arranca el Host y ejecuta `DbInitializer.EjecutarAsync()`.
2. Se abre `LoginWindow` como diálogo. Si no se inicia sesión, la app se cierra (`ShutdownMode = OnExplicitShutdown`, el cierre se controla a mano).
3. Con login correcto se muestra `MainWindow` con menú lateral; Reportes y Administración solo aparecen según el rol.
4. `MainViewModel` cambia `VistaActual`; los `DataTemplate` de `App.xaml` eligen la vista según el tipo de ViewModel.
5. Cerrar sesión limpia `SesionActual` y vuelve al login.

## 8. Convenciones del código

- **Español** en nombres de dominio, mensajes y comentarios.
- **MVVM con CommunityToolkit**: propiedades con `[ObservableProperty]`, comandos con `[RelayCommand]` (un método `GuardarAsync` genera `GuardarCommand`).
- **Servicios y DbContext en `Transient`**: cada ViewModel obtiene su propio `DbContext`, para no arrastrar datos viejos en una app de escritorio de larga duración.
- **Errores de negocio**: se lanzan como `ReglaNegocioException`; los ViewModels muestran su mensaje y para otras excepciones muestran un mensaje genérico.
- **Pantallas de administración**: patrón lista a la izquierda y formulario a la derecha; la lista se recarga tras guardar.
- Cada vista carga sus datos en el evento `Loaded` (si su lista está vacía).
- La contraseña de `PasswordBox` no se enlaza: se pasa al ViewModel desde el código de la vista.
- Los archivos nuevos van en la carpeta de su capa y respetan el sentido de las dependencias (Domain y Application no conocen EF ni WPF).

## 9. Cómo ejecutar y compilar

Base de datos (una sola vez), desde `database\`, con UTF-8 para no dañar los acentos:

```
sqlcmd -S MILU -U <usuario> -P <clave> -C -b -f 65001 -i database\01_schema.sql
sqlcmd -S MILU -U <usuario> -P <clave> -C -b -f 65001 -i database\02_datos_iniciales.sql
```

Aplicación: abrir `PQRS.slnx` en Visual Studio, proyecto de inicio `PQRS.Desktop`, **F5**.

Compilación por consola:

```
dotnet build PQRS.slnx
```

## 10. Cosas que conviene saber (trampas encontradas)

- Con la app abierta, el `dotnet build` normal falla por DLL bloqueadas por el proceso `PQRS.Desktop`. Cerrar la app, o compilar a otra carpeta: `dotnet build src\PQRS.Desktop -p:OutDir=<carpeta>\`.
- Los errores de **enlace de datos en XAML** (`Binding` con nombre mal escrito) no los detecta el compilador: aparecen solo al ejecutar, en la ventana *Salida* de Visual Studio.
- Dentro del espacio de nombres `PQRS.Desktop`, la palabra `Application` choca con el espacio de nombres `PQRS.Application`; por eso `App` hereda de `System.Windows.Application`.
- En proyectos WPF `System.IO` no entra en los *implicit usings*: hay que agregar `using System.IO;` donde se use `File` o `Path`.
- `SolicitudService.BuscarAsync` limita a 500 resultados (la bandeja); para exportar se usa `ReporteService.ObtenerListadoAsync`, sin tope.
- Los datos de ejemplo de `01_schema.sql` se rompieron una vez con los acentos por ejecutar sin `-f 65001`.

## 11. Pendiente / ideas

- **Paso 9**: notificaciones por correo (al solicitante y al responsable) y alertas de solicitudes por vencer. Ya existen `Area.CorreoNotificacion` y el parámetro `CorreoRemitente`.
- **Instalador (hecho, falta probarlo en un equipo limpio)**: carpeta `installer\` con `PQRS.iss` (Inno Setup 6), `build-installer.ps1` y `README.md`. Publica la app autocontenida (win-x64) y genera `installer\Output\PQRS-Setup-<versión>.exe`; el asistente pide la conexión a SQL Server, la carpeta de adjuntos y el administrador inicial, y genera `appsettings.json`. No crea la base de datos. Versión en `<Version>` del `.csproj`.
- Pendiente del paso 10: copias de seguridad y permisos más finos por pantalla.
- **Azure**: migrar la base a Azure SQL y, opcionalmente, publicar un portal o API para que los ciudadanos radiquen por internet. La arquitectura por capas está pensada para facilitarlo.
- Festivos en el cálculo de días hábiles.
- Adjuntar archivos desde la pantalla de radicación (hoy solo desde el detalle).
- Cambio de contraseña por el propio usuario (hoy solo lo hace un administrador editando al usuario).
- Bloqueo por intentos fallidos de login.
- Pruebas automatizadas (no existe proyecto de pruebas todavía).
- Exportar reportes a PDF.
