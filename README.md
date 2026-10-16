# Sistema PQRS

![.NET](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![WPF](https://img.shields.io/badge/UI-WPF%20(MVVM)-0078D4)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019%2B-CC2927?logo=microsoftsqlserver&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10-6DB33F)
![Plataforma](https://img.shields.io/badge/Windows-10%2F11-0078D4?logo=windows&logoColor=white)

Aplicación de escritorio para Windows que gestiona **Peticiones, Quejas, Reclamos y Sugerencias (PQRS)**. Es un sistema **genérico**: los tipos de solicitud, categorías, áreas y el nombre de la organización se configuran desde la propia aplicación, así que sirve para cualquier entidad (empresa, colegio, clínica, entidad pública, etc.) sin tocar código.

## Características

- **Radicación** de solicitudes con número único (`P-2026-000014`), con solicitante identificado o **anónimo**.
- **Bandeja** con filtros (texto, estado, tipo, fechas, solo vencidas) y semáforo de solicitudes vencidas.
- **Gestión completa**: asignar responsable, cambiar estado con un flujo validado, responder, comentar (notas internas o visibles) y adjuntar archivos.
- **Historial** de cada solicitud (trazabilidad: quién hizo qué y cuándo). Nada se borra.
- **Plazos automáticos**: fecha de vencimiento según los días hábiles definidos en cada tipo de solicitud.
- **Reportes** con indicadores (abiertas, vencidas, promedio de respuesta, % a tiempo), gráficos de barras y **exportación a Excel**.
- **Administración**: usuarios y roles, tipos de solicitud, categorías, áreas y datos de la organización.
- **Seguridad**: contraseñas cifradas (PBKDF2), permisos por rol validados en los servicios y adjuntos con lista blanca de extensiones.
- **Instalador** para Windows con asistente de configuración.

## Roles

| Rol | Qué puede hacer |
|---|---|
| **Administrador** | Todo, incluida la administración y los reportes |
| **Supervisor** | Asignar, gestionar solicitudes y ver reportes |
| **Responsable** | Cambiar estado, responder, comentar y adjuntar |
| **Radicador** | Radicar, ver, comentar y adjuntar |

## Tecnologías

- **C# / .NET 10**
- **WPF** con patrón **MVVM** ([CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet))
- **Entity Framework Core 10** sobre **SQL Server**
- **Microsoft.Extensions.Hosting** (inyección de dependencias y configuración)
- **ClosedXML** (exportación a Excel)
- **Inno Setup 6** (instalador)

## Arquitectura

Cuatro proyectos por capas; las dependencias solo van hacia abajo:

```
PQRS.Desktop         → pantallas WPF (Views + ViewModels)
   │
   ├─► PQRS.Infrastructure → EF Core, servicios, archivos, Excel
   │        │
   └────────┴─► PQRS.Application → interfaces y DTOs
                     │
                     └─► PQRS.Domain → entidades, enums y reglas del negocio
```

## Estructura del repositorio

```
PQRS/
├── src/
│   ├── PQRS.Domain/           Entidades, enums y reglas puras
│   ├── PQRS.Application/      Contratos (interfaces) y DTOs
│   ├── PQRS.Infrastructure/   Acceso a datos y servicios
│   └── PQRS.Desktop/          Aplicación WPF
├── database/                  Scripts SQL (esquema y datos iniciales)
├── installer/                 Instalador (Inno Setup)
├── documentacion.md           Manual de uso
├── documentacionDEVS.md       Guía para desarrolladores
├── context.md                 Resumen técnico del proyecto
└── PQRS.slnx
```

## Inicio rápido

### Requisitos

- Windows 10 u 11
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server 2019 o superior (con autenticación de SQL Server habilitada)
- Visual Studio 2026 (o la CLI de .NET)

### 1. Clonar el repositorio

```bash
git clone https://github.com/ArnoldMonsalve/PQRS.git
cd PQRS
```

### 2. Crear la base de datos

Ejecute los scripts de la carpeta `database` en este orden, como **UTF-8** (para no dañar los acentos):

```bash
sqlcmd -S SERVIDOR -U USUARIO -P CLAVE -C -b -f 65001 -i database\01_schema.sql
sqlcmd -S SERVIDOR -U USUARIO -P CLAVE -C -b -f 65001 -i database\02_datos_iniciales.sql
```

- `01_schema.sql`: crea la base `PQRS`, las tablas, los tipos de solicitud y los parámetros iniciales.
- `02_datos_iniciales.sql`: áreas y categorías de ejemplo (opcional).

### 3. Configurar la aplicación

Edite `src/PQRS.Desktop/appsettings.json`:

| Clave | Descripción |
|---|---|
| `ConnectionStrings:PqrsDb` | Cadena de conexión a SQL Server |
| `Almacenamiento:RutaAdjuntos` | Carpeta de archivos adjuntos. Vacía = `C:\ProgramData\PQRS\Adjuntos`. Con varios equipos, use una carpeta de red compartida |
| `Seed:AdminUsuario` / `Seed:AdminClave` | Usuario y contraseña del administrador inicial. Solo se usan la primera vez, cuando la base no tiene usuarios (la contraseña debe tener mínimo 8 caracteres, con letras y números) |

> **Importante:** no suba contraseñas reales al repositorio. Use valores de ejemplo en `appsettings.json` y no incluya ese archivo en los commits con sus claves.

### 4. Ejecutar

Abra `PQRS.slnx` en Visual Studio, deje `PQRS.Desktop` como proyecto de inicio y presione **F5**. También puede usar:

```bash
dotnet build PQRS.slnx
dotnet run --project src/PQRS.Desktop
```

Inicie sesión con el administrador definido en `Seed`. Después de entrar, cambie su contraseña desde *Administración → Usuarios*.

## Instalador

La carpeta [`installer/`](installer) contiene un instalador para Windows (Inno Setup). Genera una aplicación autocontenida (no requiere instalar .NET en los equipos de los usuarios) y un asistente que pide la conexión a SQL Server, la carpeta de adjuntos y el administrador inicial.

```bash
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
```

El instalador queda en `installer\Output\`. Más detalles en [`installer/README.md`](installer/README.md). El instalador **no crea la base de datos**: eso se hace con los scripts del paso 2.

## Documentación

| Documento | Para quién |
|---|---|
| [`documentacion.md`](documentacion.md) | Usuarios y administradores: cómo se usa cada pantalla, roles, plazos de vencimiento y solución de problemas |
| [`documentacionDEVS.md`](documentacionDEVS.md) | Desarrolladores: arquitectura, flujo del código, recetas para agregar funciones e instalador |
| [`context.md`](context.md) | Resumen técnico (útil también para dar contexto a una IA) |

## Estado del proyecto

**Versión 1.0.0**

- [x] Radicación, bandeja y detalle de solicitudes
- [x] Flujo de estados, respuestas, comentarios e historial
- [x] Adjuntos
- [x] Reportes y exportación a Excel
- [x] Administración de usuarios y catálogos
- [x] Instalador para Windows
- [ ] Notificaciones por correo y alertas de vencimiento
- [ ] Festivos en el cálculo de plazos
- [ ] Copias de seguridad automáticas
- [ ] Cambio de contraseña por el propio usuario
- [ ] Bloqueo por intentos fallidos de inicio de sesión
- [ ] Pruebas automatizadas
- [ ] Despliegue en Azure

## Cómo contribuir

1. Cree una rama a partir de `main` (`git checkout -b mi-cambio`).
2. Haga sus cambios siguiendo las convenciones de [`documentacionDEVS.md`](documentacionDEVS.md) (capas, MVVM, nombres en español).
3. Si cambia una entidad, agregue el script SQL correspondiente en `database/` (el proyecto no usa migraciones de EF).
4. Compile (`dotnet build PQRS.slnx`) y pruebe antes de enviar.
5. Abra un *Pull Request* explicando qué cambió y por qué.

## Licencia

Aún no se ha definido una licencia para este proyecto.
