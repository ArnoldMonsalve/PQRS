# Sistema PQRS — Guía para desarrolladores

Esta guía explica **cómo está hecho el código**: qué hace cada parte, cómo se conectan entre sí y cómo seguir el recorrido de una acción desde que el usuario hace clic hasta que se guarda en la base de datos. Está escrita para que un desarrollador junior pueda entender el proyecto sin que alguien se lo explique.

> Otros documentos del proyecto:
> - [`documentacion.md`](documentacion.md) — manual de **uso** para usuarios y administradores (qué hace cada pantalla, roles, plazos).
> - [`context.md`](context.md) — resumen técnico corto, pensado para dar contexto a una IA.
>
> Si no conoce el negocio (qué es una PQRS, estados, roles), lea primero las secciones 1 a 4 de `documentacion.md`.

---

## Contenido

1. [Cómo leer esta guía](#1-cómo-leer-esta-guía)
2. [Conceptos previos](#2-conceptos-previos)
3. [Mapa del proyecto](#3-mapa-del-proyecto)
4. [Arquitectura por capas](#4-arquitectura-por-capas)
5. [Cómo arranca la aplicación](#5-cómo-arranca-la-aplicación)
6. [Inyección de dependencias](#6-inyección-de-dependencias)
7. [Cómo funciona una pantalla (MVVM)](#7-cómo-funciona-una-pantalla-mvvm)
8. [Recorrido completo: radicar una solicitud](#8-recorrido-completo-radicar-una-solicitud)
9. [Recorrido: iniciar sesión](#9-recorrido-iniciar-sesión)
10. [Navegación: bandeja → detalle](#10-navegación-bandeja--detalle)
11. [Atender una solicitud (asignar, estado, responder)](#11-atender-una-solicitud-asignar-estado-responder)
12. [Adjuntos](#12-adjuntos)
13. [Reportes y Excel](#13-reportes-y-excel)
14. [Administración (usuarios y catálogos)](#14-administración-usuarios-y-catálogos)
15. [Base de datos y Entity Framework](#15-base-de-datos-y-entity-framework)
16. [Seguridad](#16-seguridad)
17. [Recetas: cómo agregar cosas](#17-recetas-cómo-agregar-cosas)
18. [Errores comunes y cómo depurarlos](#18-errores-comunes-y-cómo-depurarlos)
19. [Catálogo de archivos](#19-catálogo-de-archivos)
20. [Mejoras pendientes y dónde engancharlas](#20-mejoras-pendientes-y-dónde-engancharlas)
21. [El instalador: cómo se hizo y cómo mantenerlo](#21-el-instalador-cómo-se-hizo-y-cómo-mantenerlo)

---

## 1. Cómo leer esta guía

- Si es su **primer día**: lea las secciones 2, 3 y 4, y luego la 8 (el recorrido completo de radicar). Con eso entiende el 80 % del proyecto, porque todas las demás pantallas siguen el mismo patrón.
- Si va a **hacer un cambio**: busque la receta en la sección 17.
- Si algo **falla**: sección 18.
- Los nombres de dominio (Solicitud, Radicado, Seguimiento…) están en **español**, igual que los mensajes y comentarios. Los términos técnicos (Service, ViewModel, DbContext) se dejan en inglés.

---

## 2. Conceptos previos

Si ya los conoce, salte a la sección 3.

### Capas
Separar el código en "pisos" con responsabilidades distintas. Un piso solo puede usar los de abajo, nunca los de arriba. Así, por ejemplo, la lógica del negocio no sabe nada de ventanas ni de SQL, y se puede cambiar una sin romper la otra.

### MVVM (Model–View–ViewModel)
Patrón para pantallas WPF:
- **View** (`.xaml`): solo el dibujo (botones, cajas de texto). Casi sin código.
- **ViewModel** (`.cs`): los datos y las acciones de la pantalla (propiedades y comandos). **No conoce** los controles.
- **Model**: los datos del negocio (en este proyecto: entidades y DTOs).

La View y el ViewModel se conectan por **data binding**: en XAML se escribe `Text="{Binding Asunto}"` y WPF mantiene sincronizado el texto de la caja con la propiedad `Asunto` del ViewModel, en ambos sentidos.

### Comando (`ICommand`)
Una acción del ViewModel que un botón puede ejecutar: `Command="{Binding RadicarCommand}"`. Reemplaza al evento `Click`. Un comando puede **habilitarse o deshabilitarse** según una condición.

### Inyección de dependencias (DI)
En lugar de que una clase cree lo que necesita (`new SolicitudService()`), lo **pide en su constructor** y alguien externo (el "contenedor") se lo entrega. Ventajas: se puede cambiar la implementación sin tocar la clase y es más fácil probar el código.

```csharp
public RadicarSolicitudViewModel(ISolicitudService solicitudes, ...)   // pide una interfaz
```

### Interfaz vs implementación
`ISolicitudService` (interfaz) dice **qué** se puede hacer; `SolicitudService` (clase) dice **cómo**. Quien usa el servicio solo conoce la interfaz.

### DTO (Data Transfer Object)
Un objeto simple (aquí, `record`) que **solo transporta datos** entre capas, por ejemplo `SolicitudResumenDto`. Sirve para no exponer las entidades de la base de datos a la pantalla.

### `record`
Tipo de C# para datos inmutables: `public record AreaAdminDto(int Id, string Nombre, ...)`. Se crea con `new AreaAdminDto(1, "Técnica", ...)` y sus valores no cambian después.

### Entity Framework Core (EF Core)
Librería que traduce entre **objetos C#** (entidades) y **tablas SQL**. Se escribe `db.Solicitudes.Where(...)` en C# y EF genera el SQL.

### `async` / `await`
Permite esperar a la base de datos **sin congelar la ventana**. Todo método que consulta datos es `async Task` y se llama con `await`. Si olvida el `await`, el código sigue sin esperar el resultado.

---

## 3. Mapa del proyecto

```
C:\WORKSPACE\PQRS\
├── PQRS.slnx                      Solución de Visual Studio
├── documentacion.md               Manual de uso
├── documentacionDEVS.md           Esta guía
├── context.md                     Resumen técnico (para IA)
├── installer\                     Instalador (Inno Setup): script, compilador y README (sección 21)
├── database\
│   ├── 01_schema.sql              Crea base, tablas, tipos y parámetros iniciales
│   └── 02_datos_iniciales.sql     Áreas y categorías de ejemplo
└── src\
    ├── PQRS.Domain\               El negocio puro
    ├── PQRS.Application\          Contratos (interfaces) y DTOs
    ├── PQRS.Infrastructure\       Implementación: base de datos, archivos, Excel
    └── PQRS.Desktop\              La aplicación WPF (pantallas)
```

| Proyecto | Responde a la pregunta | Depende de |
|---|---|---|
| **Domain** | ¿Qué es una solicitud y qué reglas tiene? | Nada |
| **Application** | ¿Qué operaciones ofrece el sistema? | Domain |
| **Infrastructure** | ¿Cómo se hacen esas operaciones realmente? | Application, Domain |
| **Desktop** | ¿Cómo se ven y se usan? | Application, Infrastructure |

---

## 4. Arquitectura por capas

```
        ┌─────────────────────────────────────────────┐
        │                PQRS.Desktop                 │  ← Lo que ve el usuario
        │        Views (XAML) + ViewModels            │
        └───────────────┬───────────────┬─────────────┘
                        │ usa           │ registra (DI)
                        ▼               ▼
        ┌──────────────────────┐   ┌─────────────────────────────┐
        │    PQRS.Application  │◄──│      PQRS.Infrastructure    │  ← Hace el trabajo
        │ Interfaces + DTOs    │   │ EF Core, servicios, Excel…  │
        └──────────┬───────────┘   └──────────────┬──────────────┘
                   │                              │
                   └───────────┬──────────────────┘
                               ▼
                    ┌─────────────────────┐
                    │     PQRS.Domain     │  ← Entidades, enums, reglas
                    └─────────────────────┘
```

### Regla de oro
**Las flechas solo apuntan hacia abajo.** Por eso:
- **Domain** no conoce EF, ni WPF, ni nada: solo C# puro. Es la parte más fácil de probar.
- **Application** no conoce EF ni WPF: solo define *qué* se puede hacer.
- **Infrastructure** conoce EF y archivos: aquí vive todo lo "sucio".
- **Desktop** nunca habla con la base de datos directamente: siempre pasa por una interfaz de Application.

Si se encuentra escribiendo `using Microsoft.EntityFrameworkCore;` dentro de Desktop, Application o Domain, algo se está haciendo en el lugar equivocado.

### ¿Qué hay dentro de cada capa?

**Domain**
- `Entities/` — las "cosas" del negocio: `Solicitud`, `Solicitante`, `Seguimiento`, `Respuesta`, `Adjunto`, `TipoSolicitud`, `Categoria`, `Area`, `Usuario`, `Parametro`. Todas heredan de `EntidadBase` (`Id`, `Activo`, `FechaCreacion`).
- `Enums/` — listas cerradas de valores: `EstadoSolicitud`, `CanalRecepcion`, `Prioridad`, `RolUsuario`.
- `Services/` — reglas puras sin dependencias:
  - `TransicionesEstado`: qué estados pueden seguir a cuáles.
  - `CalculadoraVencimiento`: suma días hábiles (lunes a viernes).
  - `PoliticaClaves`: reglas de contraseña (8+ caracteres, letras y números).
- `Exceptions/ReglaNegocioException` — el error que significa "esto no se puede hacer, y este es el motivo para mostrar al usuario".

**Application**
- `Interfaces/` — los contratos (`ISolicitudService`, `IAuthService`, `IReporteService`…).
- `Dtos/` — los datos que entran y salen de esos contratos.
- `Services/SesionActual` — guarda quién tiene la sesión abierta.

**Infrastructure**
- `Persistence/PqrsDbContext` — la puerta de entrada a la base de datos.
- `Persistence/DbInitializer` — crea el primer administrador.
- `Services/` — implementan las interfaces de Application.
- `Security/PasswordHasher` — cifra y verifica contraseñas.
- `DependencyInjection.cs` — registra todo en el contenedor.

**Desktop**
- `App.xaml(.cs)` — arranque y configuración global.
- `ViewModels/` y `Views/` — una pareja por pantalla.
- `Services/DialogosArchivos` — abrir/guardar archivos (aislado de los ViewModels).
- `Converters/` — convierten un dato para mostrarlo (ej. enum → texto con tildes).

---

## 5. Cómo arranca la aplicación

Todo comienza en `src/PQRS.Desktop/App.xaml.cs`. Siga este orden:

```
1. new App()                    → construye el "Host" y registra los servicios (DI)
2. OnStartup()
   2.1 ShutdownMode = OnExplicitShutdown   → la app no se cierra sola al cerrar el login
   2.2 _host.StartAsync()
   2.3 DbInitializer.EjecutarAsync()       → si no hay usuarios, crea el administrador
   2.4 MostrarLogin()
3. LoginWindow.ShowDialog()     → ventana modal; el código espera aquí
   ├─ si el usuario cancela → Shutdown() y termina
   └─ si inicia sesión bien → continúa
4. MainWindow.Show()            → ventana principal
5. Al cerrar MainWindow:
   ├─ por "Cerrar sesión" → vuelve a MostrarLogin()
   └─ por la X de la ventana → Shutdown()
```

Puntos a notar:
- **`App` hereda de `System.Windows.Application`** (con el nombre completo) porque dentro del namespace `PQRS.Desktop`, la palabra `Application` chocaría con el namespace `PQRS.Application`.
- Si algo falla al iniciar (por ejemplo, no hay conexión a la base), se muestra un `MessageBox` con el error y la aplicación se cierra.
- `DbInitializer` lee `Seed:AdminUsuario` y `Seed:AdminClave` de `appsettings.json`. **Solo actúa si la tabla `Usuario` está vacía**; después ya no hace nada.
- La configuración (`appsettings.json`) se lee gracias a `Host.CreateDefaultBuilder()`, que carga ese archivo automáticamente. Se copia a la carpeta de salida por una línea en `PQRS.Desktop.csproj`.
- El Host se crea con `.UseContentRoot(AppContext.BaseDirectory)`, para que `appsettings.json` se busque **junto al `.exe`** y no en la "carpeta de trabajo" desde donde se lance el programa (un acceso directo, una terminal, etc.). Sin esa línea, la app instalada podría no encontrar su configuración. Se agregó al hacer el instalador (sección 21).

---

## 6. Inyección de dependencias

El contenedor se configura en dos sitios:

### `PQRS.Infrastructure/DependencyInjection.cs` (método `AddInfrastructure`)
Registra todo lo de datos y servicios:

| Qué | Ciclo de vida | Por qué |
|---|---|---|
| `PqrsDbContext` | **Transient** | Ver explicación abajo |
| `ISolicitudService`, `IAuthService`, `ICatalogoService`, `IReporteService`, `IAdjuntoService`, `IUsuarioAdminService`, `ICatalogoAdminService`, `DbInitializer` | **Transient** | Cada uno recibe su propio `DbContext` |
| `IPasswordHasher`, `IExportadorExcel`, `AlmacenamientoOptions` | **Singleton** | No guardan estado: sirven una sola instancia para todos |

También lee `Almacenamiento:RutaAdjuntos` (si está vacía usa `C:\ProgramData\PQRS\Adjuntos`).

### `PQRS.Desktop/App.xaml.cs` (constructor de `App`)
Registra lo propio de la interfaz:
- `ISesionActual → SesionActual` (**Singleton**: debe ser la misma en toda la aplicación, porque recuerda quién inició sesión).
- `IDialogosArchivos → DialogosArchivos` (Singleton).
- Todas las ventanas y ViewModels (**Transient**).

### ¿Por qué `DbContext` es Transient?
En una aplicación web, cada petición HTTP tiene su "scope" y se crea un `DbContext` por petición. En escritorio **no existe esa petición**: la aplicación vive horas. Si hubiera un solo `DbContext` compartido, **guardaría en memoria datos viejos** y mostraría información desactualizada. Con Transient, **cada ViewModel recibe su propio contexto**, y se crea un ViewModel nuevo cada vez que se abre una pantalla, así los datos siempre se leen frescos.

> **Consecuencia práctica:** no guarde entidades en variables de larga vida; use DTOs y vuelva a consultar.

### ¿Cómo se pide un servicio?
Siempre por constructor:

```csharp
public class BandejaSolicitudesViewModel : ObservableObject
{
    private readonly ISolicitudService _solicitudes;
    public BandejaSolicitudesViewModel(ISolicitudService solicitudes) => _solicitudes = solicitudes;
}
```

Para crear una pantalla desde otra (navegación), `MainViewModel` usa `IServiceProvider.GetRequiredService<T>()`.

---

## 7. Cómo funciona una pantalla (MVVM)

Cada pantalla son **tres piezas** que se conectan. Tomemos "Bandeja":

| Pieza | Archivo | Qué contiene |
|---|---|---|
| ViewModel | `ViewModels/BandejaSolicitudesViewModel.cs` | Filtros, lista, comandos |
| Vista | `Views/BandejaSolicitudesView.xaml` | El diseño (DataGrid, filtros) |
| Enlace | `App.xaml` | Un `DataTemplate` que dice "cuando el contenido sea un `BandejaSolicitudesViewModel`, dibújalo con `BandejaSolicitudesView`" |

### El enlace ViewModel → Vista (DataTemplates)
En `App.xaml`:

```xml
<DataTemplate DataType="{x:Type vm:BandejaSolicitudesViewModel}">
    <views:BandejaSolicitudesView/>
</DataTemplate>
```

`MainWindow` tiene un `ContentControl` enlazado a `VistaActual`. Cuando `MainViewModel` hace `VistaActual = bandeja;` (un ViewModel), WPF busca el `DataTemplate` para ese tipo y **dibuja la vista correspondiente**, ya con el ViewModel como `DataContext`. Por eso las vistas no crean sus ViewModels: reciben el que les toca.

### CommunityToolkit.Mvvm: menos código repetido
El proyecto usa la librería `CommunityToolkit.Mvvm`, que **genera código automáticamente**. Hay que entender sus convenciones de nombres:

**Propiedades observables**

```csharp
[ObservableProperty] private string _asunto = string.Empty;
```
Genera una propiedad pública **`Asunto`** (el campo `_asunto` pasa a `Asunto` con mayúscula, sin guion bajo). Al cambiar, avisa a la interfaz. **Desde el código use siempre `Asunto`, no `_asunto`**, porque si se cambia el campo no se avisa a la pantalla.

**Reaccionar a un cambio** (método parcial generado):
```csharp
partial void OnSeleccionadoChanged(UsuarioAdminDto? value) { ... }
```
Se ejecuta cada vez que `Seleccionado` cambia. Las pantallas de administración lo usan para llenar el formulario al elegir una fila.

**Avisar que otras propiedades cambiaron:**
```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(EsNuevo))]   // si Id cambia, también se recalcula EsNuevo
private int _id;
```

**Comandos**
```csharp
[RelayCommand] private async Task RadicarAsync() { ... }
```
Genera **`RadicarCommand`** (se quita el sufijo `Async` y se agrega `Command`). Es lo que se enlaza en XAML: `Command="{Binding RadicarCommand}"`.

**Habilitar/deshabilitar un comando:**
```csharp
[RelayCommand(CanExecute = nameof(PuedeRadicar))]
private async Task RadicarAsync() { ... }
private bool PuedeRadicar() => !EstaOcupado;
```
Con `[NotifyCanExecuteChangedFor(nameof(RadicarCommand))]` sobre `EstaOcupado`, el botón se deshabilita solo mientras hay una operación en curso (evita doble clic).

> Las clases con estos atributos deben ser **`partial`**; por eso verá `public partial class ...ViewModel`.

### Patrón de estado repetido en los ViewModels
Casi todos tienen estas tres propiedades, que la vista muestra:

| Propiedad | Uso |
|---|---|
| `MensajeError` | Texto rojo bajo el formulario |
| `MensajeExito` | Texto verde |
| `EstaOcupado` | Verdadero mientras se espera a la base de datos |

Y un método `EjecutarAsync(...)` que envuelve cada acción con el mismo manejo de errores:

```csharp
try { await accion(); }
catch (ReglaNegocioException ex) { MensajeError = ex.Message; }          // error esperado: se muestra tal cual
catch (Exception ex) { MensajeError = $"No se pudo completar: {ex.GetBaseException().Message}"; }  // error inesperado
finally { EstaOcupado = false; }
```

---

## 8. Recorrido completo: radicar una solicitud

Este es el flujo más importante. **Si lo entiende, entiende el proyecto**: las demás pantallas hacen lo mismo con otros datos.

### Vista panorámica

```
Usuario presiona [Radicar]
        │
        ▼
 RadicarSolicitudView.xaml          Command="{Binding RadicarCommand}"
        │
        ▼
 RadicarSolicitudViewModel          RadicarAsync(): valida, arma el DTO, llama al servicio
        │   _solicitudes.RadicarAsync(new CrearSolicitudRequest(...))
        ▼
 ISolicitudService  (interfaz, en Application)
        │
        ▼
 SolicitudService   (implementación, en Infrastructure)
        │   reglas, radicado, vencimiento
        ▼
 PqrsDbContext (EF Core)  ──►  SQL Server (tablas Solicitud, Solicitante, Seguimiento)
        │
        ▼
 devuelve SolicitudDetalleDto  ──►  el ViewModel muestra "Radicada con el número P-2026-000014"
```

### Paso 1 — La vista
`Views/RadicarSolicitudView.xaml` tiene cajas de texto enlazadas, por ejemplo:

```xml
<TextBox Text="{Binding Asunto, UpdateSourceTrigger=PropertyChanged}"/>
...
<Button Content="Radicar" Command="{Binding RadicarCommand}"/>
```

Al cargarse (`Loaded`), el código de la vista pide al ViewModel que cargue los catálogos (tipos y categorías para los desplegables).

### Paso 2 — El ViewModel
`ViewModels/RadicarSolicitudViewModel.cs`, método `RadicarAsync`:

1. Limpia mensajes anteriores.
2. Llama a `Validar()`: revisa lo mínimo en la pantalla (tipo, categoría, asunto, descripción; si no es anónima: documento y nombres; correo válido). Si algo falta, muestra el mensaje y **no continúa**. Esta validación es solo para **dar respuesta rápida al usuario**; la validación "de verdad" se repite en el servicio.
3. Marca `EstaOcupado = true`.
4. Arma un `SolicitanteDto` (o `null` si es anónima) y un `CrearSolicitudRequest`, que incluye el Id del usuario actual (`_sesion.Usuario?.Id`).
5. Llama `await _solicitudes.RadicarAsync(...)`.
6. Si funciona: muestra el radicado y la fecha de vencimiento, y limpia el formulario.
7. Si lanza `ReglaNegocioException`: muestra su mensaje en rojo.

### Paso 3 — El servicio (el cerebro)
`Infrastructure/Services/SolicitudService.cs`, método `RadicarAsync`. En orden:

1. **Valida** asunto y descripción (de nuevo: nunca se confía solo en la pantalla).
2. **Busca el tipo y la categoría** en la base y verifica que existan y estén activos.
3. **Solicitante:** si viene uno, llama a `ObtenerOCrearSolicitanteAsync`: busca por (tipo + número de documento); si existe, lo reutiliza y actualiza sus datos; si no, lo crea. Guarda y devuelve su Id. (Si es anónima, se salta esto y `SolicitanteId` queda `null`.)
4. **Calcula el vencimiento:**
   ```csharp
   var vencimiento = CalculadoraVencimiento.SumarDiasHabiles(ahora, tipo.DiasRespuesta);
   ```
   (La prioridad **no** interviene; solo los días del tipo.)
5. **Genera el radicado** con `SiguienteRadicadoAsync`: busca el último radicado con el mismo prefijo y año (`P-2026-`), le suma 1 y lo formatea a 6 dígitos (`P-2026-000014`).
6. Crea la entidad `Solicitud` con estado `Radicada`, y le agrega un `Seguimiento` inicial ("Solicitud radicada.") para el historial.
7. **Guarda** con `SaveChangesAsync()`.
8. **Reintento por choque de radicados:** si dos usuarios radican al mismo tiempo y ambos calculan el mismo número, el índice único de la base rechaza el segundo (`DbUpdateException`). El código captura esto, limpia el `ChangeTracker` y reintenta (hasta 3 veces) con el siguiente número.
9. Vuelve a leer la solicitud completa con `ObtenerPorIdAsync` y la devuelve como `SolicitudDetalleDto`.

### Paso 4 — La base de datos
EF Core genera los `INSERT` en `Solicitud` y `Seguimiento` (y `Solicitante` si era nuevo) y los envía a SQL Server.

### Paso 5 — La respuesta vuelve
El `SolicitudDetalleDto` viaja de regreso hasta el ViewModel, que arma el mensaje verde. La vista se actualiza sola gracias al data binding.

### Por qué está dividido así
| Si quisiera… | Cambia… |
|---|---|
| Otra pantalla distinta que también radique | Solo la vista y el ViewModel; el servicio se reutiliza |
| Cambiar cómo se numera el radicado | Solo `SolicitudService.SiguienteRadicadoAsync` |
| Radicar desde una API web | Reutilizar `ISolicitudService` tal cual |
| Cambiar de SQL Server a otra base | Solo Infrastructure |

---

## 9. Recorrido: iniciar sesión

```
LoginWindow ──► LoginViewModel.IngresarAsync()
                     │  _auth.IniciarSesionAsync(usuario, clave)
                     ▼
              AuthService
                     │ 1. busca Usuario por NombreUsuario y Activo
                     │ 2. IPasswordHasher.Verificar(clave, hash guardado)
                     ▼
        UsuarioSesion (o null)
                     │
   ┌─────────────────┴─────────────────┐
   │ null: "Usuario o contraseña        │ éxito: _sesion.Iniciar(usuario)
   │ incorrectos" (no dice cuál falló)  │ evento LoginExitoso → la ventana se cierra con DialogResult = true
   └────────────────────────────────────┘
```

Detalles útiles:
- **La contraseña del `PasswordBox` no se enlaza** por binding (WPF lo impide por seguridad). La ventana (`LoginWindow.xaml.cs`) escucha `PasswordChanged` y copia el texto al ViewModel (`_viewModel.Clave = TxtClave.Password`).
- El ViewModel avisa a la ventana que cierre mediante un **evento** (`LoginExitoso`), así no necesita conocer la ventana.
- Tras iniciar sesión, `SesionActual.Usuario` contiene un `UsuarioSesion` (Id, nombre, rol…). El resto de la app lo consulta con `ISesionActual` (por ejemplo, `TieneRol(...)` para mostrar u ocultar botones).
- Contraseñas: se guardan como `v1.iteraciones.sal.hash` (PBKDF2-SHA256, 210 000 iteraciones, sal aleatoria). Se comparan en tiempo constante. Nunca se guarda la clave en texto.

---

## 10. Navegación: bandeja → detalle

No hay una librería de navegación. Todo se resuelve con **una propiedad y eventos**.

### `MainViewModel` es el "director"
- Tiene la propiedad `VistaActual` (tipo `object`).
- Comandos `NavegarBandeja`, `NavegarRadicar`, `NavegarReportes`, `NavegarAdministracion`: cada uno crea el ViewModel de esa pantalla con `GetRequiredService<T>()` y lo asigna a `VistaActual`.
- Los botones del menú lateral (`MainWindow.xaml`) están enlazados a esos comandos.

### ¿Cómo se abre el detalle desde la bandeja?
La bandeja **no conoce** al detalle ni a `MainViewModel`. Solo lanza un evento:

```csharp
// BandejaSolicitudesViewModel
public event Action<int>? SolicitudAbierta;
private void Abrir() { if (Seleccionada is not null) SolicitudAbierta?.Invoke(Seleccionada.Id); }
```

`MainViewModel` se suscribe cuando crea la bandeja:

```csharp
var bandeja = _services.GetRequiredService<BandejaSolicitudesViewModel>();
bandeja.SolicitudAbierta += async id => await AbrirDetalleAsync(id);
VistaActual = bandeja;
```

Y `AbrirDetalleAsync` crea `DetalleSolicitudViewModel`, le pide `CargarAsync(id)` y lo muestra. Para volver, el detalle lanza su evento `Volver`, que `MainViewModel` conecta a `NavegarBandeja`.

**Patrón:** *los ViewModels hijos avisan con eventos; el padre decide a dónde navegar.* Evita que las pantallas se conozcan entre sí.

### Doble clic en la lista
El `DataGrid` tiene un manejador `MouseDoubleClick` en el código de la vista (`BandejaSolicitudesView.xaml.cs`) que ejecuta `AbrirCommand`, **solo si el clic fue sobre una fila** (así no se activa al hacer doble clic en el encabezado).

### Visibilidad por rol
`MainViewModel` expone `EsAdministrador` y `PuedeVerReportes`, calculados con `_sesion.TieneRol(...)`. En `MainWindow.xaml` los botones usan `Visibility="{Binding EsAdministrador, Converter={StaticResource BoolToVis}}"`. **Ocultar un botón no es seguridad**; la seguridad real está en los servicios (sección 16).

---

## 11. Atender una solicitud (asignar, estado, responder)

Pantalla: `DetalleSolicitudView` + `DetalleSolicitudViewModel`. Servicio: `SolicitudService`.

### Qué muestra y qué permite según el rol y el estado
El ViewModel calcula propiedades booleanas que la vista usa para mostrar/ocultar secciones:

| Propiedad | Condición |
|---|---|
| `PuedeAsignar` | No final + rol Administrador o Supervisor |
| `PuedeGestionar` | No final + rol Administrador, Supervisor o Responsable |
| `PuedeResponder` | `PuedeGestionar` + el estado actual permite pasar a `Respondida` |
| `PuedeCambiarEstado` | `PuedeGestionar` + hay estados siguientes disponibles |
| `PuedeComentar` | No final |

("Final" = Cerrada o Anulada.)

### Las reglas viven en el Dominio
`Domain/Services/TransicionesEstado.cs` es un diccionario con los cambios permitidos:

```
Radicada             → EnTramite, Anulada
EnTramite            → PendienteInformacion, Respondida, Anulada
PendienteInformacion → EnTramite, Anulada
Respondida           → Cerrada, EnTramite
Cerrada              → (nada)
Anulada              → (nada)
```

- La **vista** usa `Siguientes(estado)` para llenar el desplegable "Cambiar estado" (sin `Respondida`, porque a ese estado solo se llega respondiendo).
- El **servicio** usa `EsPermitida(actual, nuevo)` para **rechazar** cambios inválidos aunque alguien los intente por otra vía.

Tener la regla en un solo lugar evita que la pantalla y el servicio se contradigan.

### Cada acción, en el servicio

| Método | Qué hace |
|---|---|
| `AsignarAsync` | Exige rol Admin/Supervisor. Asigna responsable; copia su área a la solicitud si tiene; si estaba `Radicada` pasa a `EnTramite`. Agrega un `Seguimiento` interno. |
| `CambiarEstadoAsync` | Rechaza `Respondida` (debe usar Responder). Exige rol Admin/Supervisor/Responsable. Verifica la transición. Pide comentario si es `Anulada` o `PendienteInformacion`. Si el nuevo estado es final, guarda `FechaCierre`. Agrega `Seguimiento`. |
| `ResponderAsync` | Exige rol. Verifica que se pueda pasar a `Respondida` (o sea, que esté `EnTramite`). Crea la `Respuesta`, cambia el estado y agrega `Seguimiento`. |
| `ComentarAsync` | Agrega un `Seguimiento` (con `EsInterno` si se marcó). No cambia el estado. |

Todos pasan por `CargarParaEdicionAsync`, que **rechaza cualquier cambio sobre solicitudes en estado final** ("Cerrada y no admite cambios").

### El Seguimiento = la auditoría
Cada acción agrega un registro `Seguimiento` (usuario, estado anterior, estado nuevo, comentario, fecha). Es lo que se ve en el "Historial" del detalle. **Nunca se edita ni se borra.** Al agregar una acción nueva, **agregue también su Seguimiento** para no perder trazabilidad.

### Recargar después de cada acción
El ViewModel envuelve cada acción en `EjecutarAsync(accion, mensajeExito, recargar: true)`: ejecuta, vuelve a pedir la solicitud (`RecargarAsync`) y así la pantalla muestra el estado, el historial y los botones actualizados.

---

## 12. Adjuntos

Servicio: `Infrastructure/Services/AdjuntoService.cs`. Interfaz: `IAdjuntoService`.

### Guardar un archivo (`AgregarAsync`)
```
1. Limpia el nombre (Path.GetFileName) y valida longitud
2. Valida la extensión contra una LISTA BLANCA (pdf, docx, xlsx, jpg, zip…)
3. Valida que no esté vacío y que pese ≤ 10 MB
4. Verifica que la solicitud exista y no esté cerrada/anulada
5. Verifica máximo 20 adjuntos por solicitud
6. Guarda el archivo en disco con un nombre generado (GUID) en  {RutaAdjuntos}\{solicitudId}\{guid}.ext
7. Guarda en BD el registro Adjunto (nombre ORIGINAL + ruta relativa) y un Seguimiento
8. Si falla el guardado en BD, BORRA el archivo del disco (no deja basura)
```

### Decisiones de seguridad (importantes)
- **Lista blanca, no lista negra:** se aceptan solo extensiones conocidas; todo lo demás se rechaza (así un `.exe` o `.bat` no entra).
- **El nombre en disco lo genera el sistema (GUID).** Nunca se usa el nombre que mandó el usuario para guardar, evitando colisiones y ataques de ruta (`..\..\algo`).
- **`ResolverRuta`** combina la carpeta base con la ruta relativa y **verifica que el resultado siga dentro de la carpeta de adjuntos**; si no, lanza error.
- **Abrir** (`PrepararParaAbrirAsync`): copia el archivo a una carpeta temporal y devuelve la copia, así lo que se abre y se edita **no es el original**.
- **Eliminar** solo marca `Activo = false` (el archivo se conserva como respaldo) y deja un `Seguimiento` interno.

### Del lado de la pantalla
- `DetalleSolicitudViewModel.AgregarAdjuntoAsync` pide la ruta con `IDialogosArchivos.ElegirArchivo(...)`, abre el archivo con `File.OpenRead` y se lo pasa al servicio como `Stream`.
- `IDialogosArchivos` (`Desktop/Services/DialogosArchivos.cs`) existe para que el ViewModel **no use directamente** `OpenFileDialog` ni `Process.Start`: así el ViewModel no depende de WPF y se podría probar.
- Si el sistema se usa en **varios equipos**, `Almacenamiento:RutaAdjuntos` debe apuntar a una carpeta compartida de red; si no, cada equipo guardaría los archivos en su propio disco.

---

## 13. Reportes y Excel

Servicios: `ReporteService` (cálculos) y `ExportadorExcel` (archivo). Pantalla: `ReportesView` / `ReportesViewModel`.

### Cálculo (`ReporteService.ObtenerResumenAsync`)
1. Verifica que el usuario sea Administrador o Supervisor.
2. Filtra las solicitudes por rango de fechas de radicación.
3. Trae a memoria **solo las columnas necesarias** (estado, fechas, nombres de tipo/categoría/área y la fecha de la primera respuesta) con un `Select` que arma un tipo anónimo. Es más liviano que traer entidades completas.
4. Calcula en memoria: totales, abiertas, vencidas, promedio de días hasta la primera respuesta, % respondidas a tiempo.
5. Arma las distribuciones por estado, tipo, categoría y área con el método `Contar(...)`, que devuelve `ConteoDto(Etiqueta, Cantidad, Porcentaje)`.

El `Porcentaje` está pensado para **dibujar la barra**: el `ProgressBar` de la vista tiene `Maximum=100` y `Value` enlazado al porcentaje.

### Listado completo
`ObtenerListadoAsync` devuelve todas las solicitudes del rango **sin el tope de 500** que sí tiene la bandeja. Se usa para exportar.

### Exportar (`ExportadorExcel`)
- Usa la librería **ClosedXML** para crear un libro con dos hojas: "Resumen" y "Solicitudes".
- La interfaz `IExportadorExcel` está en Application; la implementación en Infrastructure. Así, si algún día se cambia ClosedXML por otra librería, solo se toca ese archivo.
- En el ViewModel, la escritura va dentro de `Task.Run(...)` para **no congelar la ventana**.
- Un texto que empiece con `=` (por ejemplo un asunto "=SUMA(1,1)") se guarda como **texto**, no como fórmula, porque ClosedXML lo asigna como cadena. Esto fue probado.

---

## 14. Administración (usuarios y catálogos)

Pantallas dentro de `Views/Admin` y `ViewModels/Admin`. Contenedor: `AdministracionView` con un `TabControl` de 5 pestañas, cada una con su ViewModel (`AdministracionViewModel` los agrupa).

### Patrón maestro–detalle (igual en las 5 pestañas)
```
┌─────────────── lista (DataGrid) ───────────────┬──── formulario ────┐
│ filas del catálogo                              │ campos + Guardar   │
│ al seleccionar una → llena el formulario        │ botón Nuevo        │
└─────────────────────────────────────────────────┴────────────────────┘
```

**Cómo funciona en código** (ejemplo `TiposAdminViewModel`):
- `Seleccionado` (la fila elegida). Su método generado `OnSeleccionadoChanged` copia los valores a los campos del formulario.
- `Id`: si es **0**, "Guardar" **crea**; si es distinto de 0, **actualiza**. Es la convención de todo el módulo.
- `Nuevo`: pone `Seleccionado = null` y limpia los campos (Id = 0).
- `GuardarAsync`: llama al servicio con un DTO, y al terminar **recarga la lista** y vuelve a seleccionar el registro guardado.

### Truco de la reselección
Tras guardar, para que el formulario muestre los datos recién guardados, el código hace:
```csharp
Seleccionado = null;                                  // fuerza un cambio
Seleccionado = Lista.FirstOrDefault(x => x.Id == id);  // dispara OnSeleccionadoChanged otra vez
```
Sin el `null` intermedio no hay cambio de valor y no se dispara el evento.

### `AdminViewModelBase`
Clase base común que ofrece `MensajeError`, `MensajeExito`, `EstaOcupado`, `ActorId` (el Id del usuario actual) y `EjecutarAsync`. Evita repetir el manejo de errores en cada pestaña.

### Reglas en los servicios (`UsuarioAdminService`, `CatalogoAdminService`)
- Todo método comienza con `Permisos.ExigirAdministradorAsync(...)`, que **consulta la base de datos** para confirmar que quien llama es un administrador activo.
- **Nada se borra:** los registros se desactivan con `Activo = false` (para no romper el historial de solicitudes que los usan).
- Validaciones: nombres únicos, formato de usuario (`^[A-Za-z0-9._-]{3,50}$`), correo válido, prefijo (1–5 letras/números, único), días entre 1 y 365, contraseña según `PoliticaClaves`.
- **Protecciones de administradores** (`ProtegerAdministradoresAsync`): nadie puede desactivarse a sí mismo ni quitarse el rol de administrador, y debe quedar siempre al menos un administrador activo.
- **Contraseña al editar un usuario:** si el campo viene vacío no se cambia; si trae texto, se valida y se vuelve a cifrar.

### Particularidad del PasswordBox en usuarios
`UsuariosAdminView.xaml.cs` copia el texto del `PasswordBox` al ViewModel y escucha un evento (`ClaveLimpiada`) que el ViewModel lanza para **vaciar el cuadro** al cambiar de usuario o guardar (porque el texto de un `PasswordBox` no se puede enlazar y no se limpia solo).

### Parámetros
`Parametro` guarda pares clave/valor. Solo se pueden editar las claves de una lista permitida (`NombreOrganizacion`, `CorreoRemitente`). Al guardar el nombre, `OrganizacionAdminViewModel` lanza el evento `NombreActualizado` y `MainViewModel` actualiza la barra superior al instante.

---

## 15. Base de datos y Entity Framework

### El esquema se maneja con scripts SQL, **no con migraciones de EF**
Esto es clave: EF Core **no crea ni modifica** las tablas. La estructura se define en `database/01_schema.sql` y los datos iniciales en `02_datos_iniciales.sql`.

> **Si cambia una entidad en C# (por ejemplo, agrega una propiedad), debe agregar la columna correspondiente en la base de datos con un script SQL.** Si no, la aplicación fallará con "Invalid column name". Cree scripts nuevos numerados (`03_...sql`) en lugar de editar los anteriores, y actualice `01_schema.sql` para instalaciones nuevas.

### Tablas
`Area`, `TipoSolicitud`, `Categoria`, `Usuario`, `Solicitante`, `Solicitud`, `Seguimiento`, `Respuesta`, `Adjunto`, `Parametro`. Todas con `Id` (identidad), `Activo` y `FechaCreacion`.

### Cómo se mapea (en `PqrsDbContext.OnModelCreating`)
```csharp
// 1. Las tablas se llaman como la clase, en singular (Solicitud, no Solicitudes)
modelBuilder.Entity(entityType.ClrType).ToTable(entityType.ClrType.Name);

// 2. Propiedades calculadas: no existen como columna
modelBuilder.Entity<Solicitud>().Ignore(s => s.EstaVencida);
modelBuilder.Entity<Solicitante>().Ignore(s => s.NombreCompleto);

// 3. Ninguna llave foránea borra en cascada
fk.DeleteBehavior = DeleteBehavior.Restrict;

// 4. Índices únicos
Solicitud.Radicado, Usuario.NombreUsuario, Parametro.Clave
```

- Los **enums** se guardan como `INT` (ej. `Estado`: 1 Radicada, 2 EnTramite…). **No cambie el orden ni los números de los enums existentes**: dañaría los datos guardados. Agregue valores nuevos al final.
- `Restrict` significa que **no se puede borrar** un registro referenciado por otro (por ejemplo, un área que tiene solicitudes). Es otra razón por la que se usa `Activo` en lugar de borrar.
- Las propiedades calculadas (`EstaVencida`, `NombreCompleto`) **no pueden usarse dentro de consultas LINQ a la base**, porque EF no sabe traducirlas a SQL. Por eso `BuscarAsync` repite la condición de "vencida" explícitamente en el `Where`.

### Relaciones principales
```
TipoSolicitud ─┐
Categoria ─────┼─►  Solicitud ──► Seguimiento (historial, muchos)
Area ──────────┤        │   ├──► Respuesta    (muchas)
Solicitante ───┤        │   └──► Adjunto      (muchos)
Usuario (resp.)┘        │
                        └ SolicitanteId = NULL  →  solicitud anónima
```

### Consultas: buenas prácticas usadas
- `AsNoTracking()` en lecturas (más rápido; EF no vigila cambios que no se van a guardar).
- `Include(...)` para traer relaciones y evitar consultas extra; `AsSplitQuery()` en el detalle (varias colecciones) para no multiplicar filas.
- `Select` a DTOs o tipos anónimos cuando solo se necesitan algunas columnas.
- Para **editar**, se carga la entidad **con** tracking, se modifican sus propiedades y se llama `SaveChangesAsync()`.

### Fechas
Se usa `DateTime.Now` (hora local del equipo) y columnas `DATETIME2`. Si algún día el sistema se usa en zonas horarias distintas, habría que pasar a UTC.

---

## 16. Seguridad

### Los permisos se validan en **dos** lugares
| Lugar | Para qué | ¿Es seguridad real? |
|---|---|---|
| **ViewModels / vistas** (`PuedeAsignar`, visibilidad del menú) | Mostrar solo lo que corresponde al rol | **No**: es comodidad de interfaz |
| **Servicios** (`ExigirRolAsync`, `Permisos.ExigirAdministradorAsync`) | Rechazar la acción si el rol no corresponde, **consultando la base de datos** | **Sí** |

**Si cambia un permiso, cámbielo en ambos sitios**, o la pantalla ofrecerá algo que el servicio rechaza (o al revés).

El rol se vuelve a leer de la base en cada operación sensible; no se confía en el rol guardado en memoria al iniciar sesión (si un administrador cambia el rol o desactiva a alguien, aplica de inmediato).

### Resumen de medidas
- **Contraseñas:** PBKDF2-SHA256 + sal aleatoria + 210 000 iteraciones; comparación en tiempo constante; política mínima de 8 caracteres con letras y números.
- **Login:** mensaje genérico ("usuario o contraseña incorrectos") para no revelar qué usuarios existen.
- **Adjuntos:** lista blanca de extensiones, límite de tamaño, nombre en disco generado, verificación de ruta, apertura sobre copia temporal.
- **SQL:** todo pasa por EF Core con parámetros; **no se arma SQL concatenando texto**, lo que evita inyección SQL.
- **Excel:** el texto del usuario se guarda como texto (no como fórmula).
- **Sin borrado:** solicitudes, historial, usuarios y catálogos no se eliminan.

### Pendientes de seguridad
- La cadena de conexión y la clave del administrador inicial están en `appsettings.json`: pasar a *User Secrets* / variables de entorno antes de publicar.
- No hay bloqueo por intentos fallidos de login.
- El usuario de SQL de la aplicación debería tener solo los permisos necesarios (lectura/escritura en la base `PQRS`).

---

## 17. Recetas: cómo agregar cosas

### Receta A — Agregar un campo nuevo a la solicitud (ej. "Número de contrato")
1. **Base de datos:** nuevo script `database/03_...sql` con `ALTER TABLE Solicitud ADD NumeroContrato NVARCHAR(50) NULL;`. Actualice también `01_schema.sql`.
2. **Domain:** agregue `public string? NumeroContrato { get; set; }` en `Entities/Solicitud.cs`.
3. **Application:** agregue el campo a `CrearSolicitudRequest` y, si se debe mostrar, a `SolicitudDetalleDto` (`Dtos/`).
4. **Infrastructure:** en `SolicitudService.RadicarAsync` asigne el valor a la entidad; en `ObtenerDetalleAsync` inclúyalo en el DTO.
5. **Desktop:** en `RadicarSolicitudViewModel` agregue `[ObservableProperty] private string _numeroContrato` y pásela al request; en `RadicarSolicitudView.xaml` agregue el `TextBox`; en `DetalleSolicitudView.xaml` muéstrelo.
6. Compile y pruebe.

### Receta B — Agregar una pantalla nueva (ej. "Mis solicitudes")
1. **ViewModel:** cree `ViewModels/MisSolicitudesViewModel.cs` (`partial`, hereda de `ObservableObject`) con sus datos y comandos. Pida los servicios en el constructor.
2. **Vista:** cree `Views/MisSolicitudesView.xaml` (un `UserControl`) con sus bindings. Si necesita cargar datos al abrirse, use el evento `Loaded` en el `.xaml.cs` como hacen las demás.
3. **Enlace:** en `App.xaml` agregue el `DataTemplate` que une `MisSolicitudesViewModel` con `MisSolicitudesView`.
4. **Registro DI:** en `App.xaml.cs` agregue `services.AddTransient<MisSolicitudesViewModel>();`.
5. **Navegación:** en `MainViewModel` agregue un `[RelayCommand] private void NavegarMisSolicitudes() => VistaActual = _services.GetRequiredService<MisSolicitudesViewModel>();` y en `MainWindow.xaml` un botón enlazado a `NavegarMisSolicitudesCommand` (con `Visibility` si depende del rol).

### Receta C — Agregar un servicio / operación nueva
1. **Application:** declare el método en una interfaz existente o cree una nueva (`Interfaces/`), y los DTOs necesarios (`Dtos/`).
2. **Infrastructure:** implemente la clase en `Services/`, reciba `PqrsDbContext` por constructor, **valide el rol** si la acción es sensible y lance `ReglaNegocioException` para errores de negocio.
3. **Registro:** en `DependencyInjection.cs`: `services.AddTransient<IMiServicio, MiServicio>();`.
4. **Desktop:** pida `IMiServicio` en el constructor del ViewModel y llámelo dentro de `EjecutarAsync`.

### Receta D — Agregar un estado nuevo
1. Agregue el valor **al final** de `Enums/EstadoSolicitud.cs`.
2. Agréguelo a `TransicionesEstado.Permitidas` (a qué estados puede ir y desde cuáles se llega).
3. Agregue su texto en `Enums/EstadoSolicitudTexto.cs`.
4. Decida si es **abierto** o **final**: revise `EsEstadoFinal`, la propiedad `EstaVencida` de `Solicitud`, y `ReporteService` (`EstadosAbiertos` y los conteos).
5. Revise `SolicitudService` por si requiere regla especial (como `Respondida`, que solo se alcanza respondiendo).

### Receta E — Agregar un rol nuevo
1. Valor **al final** de `Enums/RolUsuario.cs`.
2. Actualice las listas de roles permitidos en los servicios (`ExigirRolAsync` en `SolicitudService`, `Permisos`, `AdjuntoService.EliminarAsync`, `ReporteService`) y en los ViewModels que muestran u ocultan acciones (`DetalleSolicitudViewModel`, `MainViewModel`).
3. Actualice `documentacion.md` (tabla de roles).

### Receta F — Cambiar el cálculo del vencimiento
- **Festivos:** modifique `Domain/Services/CalculadoraVencimiento.cs` para recibir una lista de festivos y omitirlos al contar.
- **Que dependa de la prioridad:** en `SolicitudService.RadicarAsync`, donde dice `SumarDiasHabiles(ahora, tipo.DiasRespuesta)`, ajuste los días según `request.Prioridad`.
- Actualice la sección 9 de `documentacion.md`.

### Receta G — Cambiar extensiones o tamaño de adjuntos
`AdjuntoService.cs`: constante `TamanoMaximoBytes`, `MaximoPorSolicitud` y el `HashSet` `ExtensionesPermitidas`. Actualice también el filtro del diálogo en `DialogosArchivos.ElegirArchivo` y el texto de ayuda en `DetalleSolicitudView.xaml`.

---

## 18. Errores comunes y cómo depurarlos

| Síntoma | Causa probable | Solución |
|---|---|---|
| `dotnet build` falla con "archivo bloqueado por PQRS.Desktop" | La aplicación está abierta | Ciérrela, o compile a otra carpeta: `dotnet build src\PQRS.Desktop -p:OutDir=<carpeta>\` |
| "Invalid column name 'X'" o "Invalid object name" | La entidad cambió pero la tabla no | Ejecute el script SQL que agrega la columna/tabla (sección 15) |
| Una pantalla sale vacía o un texto no aparece, **sin error de compilación** | Un `Binding` mal escrito (nombre de propiedad con typo) | WPF no avisa en compilación. Mire la ventana **Salida** de Visual Studio: aparece `BindingExpression path error` |
| Un botón nunca se habilita | La condición del comando (`CanExecute`) no se re-evalúa | Agregue `[NotifyCanExecuteChangedFor(nameof(XCommand))]` a la propiedad de la que depende |
| Cambié una propiedad pero la pantalla no se actualiza | Se asignó el campo (`_nombre`) en vez de la propiedad (`Nombre`) | Use siempre la propiedad generada |
| `CS0118: 'Application' es espacio de nombres pero se usa como tipo` | Choca `Application` con el namespace `PQRS.Application` | Use `System.Windows.Application` |
| `'File' / 'Path' / 'IOException' no existe` | En WPF `System.IO` no está en los *implicit usings* | Agregue `using System.IO;` |
| Los acentos salen como `Ã³` en la base | El script se ejecutó sin UTF-8 | Use `sqlcmd ... -f 65001` o guarde el script como UTF-8 |
| "No se pudo conectar" al iniciar | Cadena de conexión errónea, servicio de SQL detenido o usuario sin permisos | Revise `appsettings.json`, el servicio de SQL Server y los permisos del login |
| Mensaje "No tiene permisos para realizar esta acción" | El servicio rechazó el rol | Es esperado si el rol no corresponde; revise la tabla de roles |
| La bandeja "pierde" solicitudes antiguas | La bandeja muestra máximo 500 | Use filtros; para todo, exporte desde Reportes |
| Los adjuntos no aparecen en otro computador | La ruta de adjuntos es local a cada equipo | Configure `Almacenamiento:RutaAdjuntos` con una carpeta de red compartida |
| `build-installer.ps1` dice "No se encontró Inno Setup 6" | Instálelo con `winget install JRSoftware.InnoSetup` y cierre/abra la terminal (sección 21) |
| El instalador no compila: `Unknown type '...'` en la sección `[Code]` | El nombre de un tipo de Inno está mal escrito (ej. el correcto es `TInputQueryWizardPage`). Revise la ayuda de Pascal Script de Inno |
| En el instalador los acentos se ven mal (`Ã³`) | Los archivos `.iss` y `.ps1` deben guardarse como **UTF-8 con BOM** (sección 21.9) |
| La app instalada dice "Falta la cadena de conexión" | No existe `appsettings.json` junto al `.exe` (instalación silenciosa sin parámetros, o archivo borrado) |

### Consejos de depuración
- **Entienda de dónde viene el error:** si el mensaje es rojo y claro, es una `ReglaNegocioException` (regla del negocio). Si empieza con "No se pudo completar la acción", es una excepción inesperada: el texto que sigue es el error original (`GetBaseException().Message`).
- Ponga un *breakpoint* en el método del servicio (por ejemplo `RadicarAsync`) y siga el flujo con **F10/F11**.
- Para ver el SQL que genera EF: en `DependencyInjection.cs`, al `UseSqlServer(...)` puede encadenar temporalmente `.LogTo(Console.WriteLine)` o `.LogTo(s => System.Diagnostics.Debug.WriteLine(s))` (quítelo después).
- **No hay proyecto de pruebas automáticas todavía.** El mejor candidato para las primeras pruebas es `Domain` (`CalculadoraVencimiento`, `TransicionesEstado`, `PoliticaClaves`): son funciones puras, sin base de datos ni ventanas.

---

## 19. Catálogo de archivos

### Domain
| Archivo | Para qué sirve |
|---|---|
| `Common/EntidadBase.cs` | Base de todas las entidades: `Id`, `Activo`, `FechaCreacion` |
| `Entities/Solicitud.cs` | La PQRS. Incluye `EstaVencida` (calculada) y colecciones de seguimientos, respuestas y adjuntos |
| `Entities/Solicitante.cs` | Quien presenta la solicitud |
| `Entities/Seguimiento.cs` | Historial/auditoría de cada solicitud |
| `Entities/Respuesta.cs` | Respuesta oficial |
| `Entities/Adjunto.cs` | Metadatos de un archivo adjunto |
| `Entities/TipoSolicitud.cs` | Catálogo de tipos (nombre, prefijo, días de respuesta) |
| `Entities/Categoria.cs` | Catálogo de temas (con área predeterminada) |
| `Entities/Area.cs` | Dependencias que atienden |
| `Entities/Usuario.cs` | Cuentas del sistema |
| `Entities/Parametro.cs` | Configuración clave/valor |
| `Enums/*.cs` | Estados, canales, prioridades, roles (y `EstadoSolicitudTexto` para mostrar tildes) |
| `Services/TransicionesEstado.cs` | Flujo permitido entre estados |
| `Services/CalculadoraVencimiento.cs` | Suma de días hábiles |
| `Services/PoliticaClaves.cs` | Reglas de contraseña |
| `Exceptions/ReglaNegocioException.cs` | Error de negocio con mensaje para el usuario |

### Application
| Archivo | Para qué sirve |
|---|---|
| `Interfaces/ISolicitudService.cs` | Radicar, buscar, asignar, cambiar estado, responder, comentar |
| `Interfaces/ICatalogoService.cs` | Listas para desplegables y lectura de parámetros |
| `Interfaces/IAuthService.cs` | Iniciar sesión |
| `Interfaces/ISesionActual.cs` | Quién está conectado y qué rol tiene |
| `Interfaces/IPasswordHasher.cs` | Cifrar y verificar contraseñas |
| `Interfaces/IUsuarioAdminService.cs` | Administrar usuarios |
| `Interfaces/ICatalogoAdminService.cs` | Administrar tipos, categorías, áreas y parámetros |
| `Interfaces/IAdjuntoService.cs` | Adjuntos |
| `Interfaces/IReporteService.cs` | Indicadores y listado para reportes |
| `Interfaces/IExportadorExcel.cs` | Generar el archivo de Excel |
| `Dtos/*.cs` | Datos que viajan entre capas |
| `Services/SesionActual.cs` | Guarda el `UsuarioSesion` actual |

### Infrastructure
| Archivo | Para qué sirve |
|---|---|
| `DependencyInjection.cs` | Registra DbContext y servicios |
| `Persistence/PqrsDbContext.cs` | Acceso a la base de datos y mapeo |
| `Persistence/DbInitializer.cs` | Crea el primer administrador |
| `Security/PasswordHasher.cs` | PBKDF2 |
| `Services/SolicitudService.cs` | Núcleo del negocio de solicitudes |
| `Services/CatalogoService.cs` | Listas y parámetros (lectura) |
| `Services/AuthService.cs` | Verifica credenciales |
| `Services/UsuarioAdminService.cs` | CRUD de usuarios con protecciones |
| `Services/CatalogoAdminService.cs` | CRUD de catálogos y parámetros |
| `Services/AdjuntoService.cs` | Almacenamiento de archivos |
| `Services/AlmacenamientoOptions.cs` | Ruta base de adjuntos |
| `Services/ReporteService.cs` | Cálculos de reportes |
| `Services/ExportadorExcel.cs` | Genera el .xlsx con ClosedXML |
| `Services/Permisos.cs` | Verifica rol de administrador en la base |

### Desktop
| Archivo | Para qué sirve |
|---|---|
| `App.xaml` | Estilos compartidos, converters y `DataTemplate` ViewModel→Vista |
| `App.xaml.cs` | Arranque, DI, ciclo login → ventana principal |
| `appsettings.json` | Configuración (conexión, adjuntos, administrador inicial) |
| `MainWindow.xaml(.cs)` + `ViewModels/MainViewModel.cs` | Marco principal: barra, menú, navegación |
| `ViewModels/LoginViewModel.cs` + `Views/LoginWindow.xaml(.cs)` | Inicio de sesión |
| `ViewModels/RadicarSolicitudViewModel.cs` + `Views/RadicarSolicitudView.xaml` | Nueva solicitud |
| `ViewModels/BandejaSolicitudesViewModel.cs` + `Views/BandejaSolicitudesView.xaml(.cs)` | Lista y filtros |
| `ViewModels/DetalleSolicitudViewModel.cs` + `Views/DetalleSolicitudView.xaml(.cs)` | Detalle y acciones |
| `ViewModels/ReportesViewModel.cs` + `Views/ReportesView.xaml(.cs)` | Reportes |
| `ViewModels/Admin/*` + `Views/Admin/*` | Administración (5 pestañas) |
| `Services/DialogosArchivos.cs` | Diálogos de archivos y abrir documentos |
| `Converters/EstadoTextoConverter.cs` | Enum → texto, bytes → "KB/MB" |

### Instalador (`installer/`)
| Archivo | Para qué sirve |
|---|---|
| `PQRS.iss` | Script de Inno Setup: qué se instala, accesos directos y asistente de configuración |
| `build-installer.ps1` | Publica la app y compila el instalador en un paso |
| `README.md` | Guía corta del instalador (generar, instalar, actualizar, problemas) |
| `.gitignore` | Excluye las carpetas generadas `publish/` y `Output/` |

---

## 20. Mejoras pendientes y dónde engancharlas

| Mejora | Dónde se engancha |
|---|---|
| **Notificaciones por correo** (al radicar, asignar, responder) | Cree `INotificador` en `Application/Interfaces` y su implementación (SMTP) en `Infrastructure/Services`. Llámelo desde los métodos de `SolicitudService` tras guardar. Datos ya disponibles: `Area.CorreoNotificacion`, parámetro `CorreoRemitente`, correo del `Solicitante` |
| **Alertas de vencimiento** | Un servicio en segundo plano (`BackgroundService`) o una revisión al iniciar la app que consulte solicitudes abiertas próximas a vencer |
| **Festivos en los plazos** | `CalculadoraVencimiento` (parámetro con lista de festivos, idealmente una tabla `Festivo` en la base) |
| **Bloqueo por intentos fallidos** | `AuthService` (contador y fecha de bloqueo en `Usuario`) |
| **Cambio de contraseña por el propio usuario** | Nuevo método en `IAuthService` y una pantalla o diálogo |
| **Adjuntar desde "Nueva solicitud"** | `RadicarSolicitudViewModel`: lista de archivos pendientes y, tras radicar, llamar a `IAdjuntoService.AgregarAsync` por cada uno |
| **Exportar a PDF** | Nueva implementación junto a `IExportadorExcel` |
| **Pruebas automáticas** | Nuevo proyecto xUnit; empezar por `Domain` |
| **Portal web ciudadano / Azure** | Reutilizar `ISolicitudService` desde una API; migrar la base a Azure SQL (cambiar la cadena de conexión) |
| **Instalador** | **Ya existe** (carpeta `installer/`, sección 21). Pendiente: firmarlo digitalmente, cifrar la clave de `appsettings.json`, y que aplique solo los scripts de base de datos nuevos |
| **Copias de seguridad** | Fuera del código: tareas programadas de respaldo de SQL Server y de la carpeta de adjuntos |

---

## 21. El instalador: cómo se hizo y cómo mantenerlo

Esta sección documenta **todo lo que se hizo** para construir el instalador, con los comandos exactos, para que cualquier desarrollador pueda reproducirlo, mantenerlo o modificarlo. Hay también una guía corta en [`installer/README.md`](installer/README.md).

### 21.1 Qué es y qué entrega

El instalador es un único archivo, **`installer/Output/PQRS-Setup-<versión>.exe`** (unos 50 MB), que se le entrega a quien vaya a usar el sistema. Al ejecutarlo:

1. Instala la aplicación en `C:\Program Files\PQRS` (pide permisos de administrador).
2. Muestra un **asistente** que pregunta los datos de conexión a SQL Server, la carpeta de adjuntos y el administrador inicial.
3. Genera el archivo `appsettings.json` con esas respuestas.
4. Crea los accesos directos (menú Inicio y, opcionalmente, escritorio).
5. Copia los scripts SQL (`database`) y el manual de uso (`documentacion.md`) para consulta.

**No instala ni crea la base de datos.** Eso lo hace una persona una sola vez en el servidor, con los scripts `01_schema.sql` y `02_datos_iniciales.sql`.

### 21.2 Decisiones de diseño (y por qué)

| Decisión | Por qué |
|---|---|
| **Inno Setup 6** como herramienta | Gratuita, muy usada, genera un `.exe` simple, permite escribir un asistente propio y tiene traducción oficial al español. Alternativas descartadas: **MSIX** (exige certificado de firma de pago), **WiX/MSI** (curva de aprendizaje alta para este caso) |
| **Aplicación autocontenida** (`--self-contained`) | El instalador incluye el *runtime* de .NET 10. Los equipos de los usuarios **no necesitan instalar .NET**. Cuesta tamaño (168 MB publicado, 50 MB comprimido) pero evita el error más común en equipos de usuarios |
| **No usar `PublishSingleFile` ni `PublishTrimmed`** | WPF no es compatible con el recorte (*trimming*), y el archivo único obliga a extraer librerías nativas en cada arranque. Se publica en carpeta normal |
| **`appsettings.json` no viaja dentro del instalador** | Cada cliente tiene su propio servidor, base y claves. El de desarrollo tiene claves nuestras y **no debe llegar a un cliente**. Lo genera el asistente |
| **La base de datos queda fuera del instalador** | Crear la base exige permisos de administrador del servidor SQL, y repetir el script en una actualización rompería (las tablas ya existen). Es más seguro y claro que sea un paso manual y controlado |
| **Actualizaciones que no tocan la configuración** | Si ya existe `appsettings.json`, no se vuelve a preguntar ni se sobrescribe |
| **Sin firma digital (por ahora)** | Requiere comprar un certificado. Mientras tanto Windows SmartScreen muestra "editor desconocido" |

### 21.3 Cómo se construyó, paso a paso

Estos son los pasos reales, en orden, con sus comandos.

**Paso 1 — Instalar Inno Setup en el equipo de desarrollo** (una sola vez):

```
winget install --id JRSoftware.InnoSetup -e --accept-package-agreements --accept-source-agreements
```
Se instaló la versión 6.7.3 en `%LOCALAPPDATA%\Programs\Inno Setup 6\` (instalación por usuario). El compilador de línea de comandos es `ISCC.exe` dentro de esa carpeta.

**Paso 2 — Preparar el proyecto** (dos cambios pequeños en el código):

- En `src/PQRS.Desktop/PQRS.Desktop.csproj` se agregaron la versión y el nombre del producto:
  ```xml
  <Version>1.0.0</Version>
  <Product>Sistema PQRS</Product>
  ```
  Esa `<Version>` es la **única fuente de la versión**: el script de compilación la lee y la usa en el nombre del instalador.
- En `src/PQRS.Desktop/App.xaml.cs` se agregó `.UseContentRoot(AppContext.BaseDirectory)` al crear el Host (explicado en la sección 5). Sin esto, la app instalada puede no encontrar `appsettings.json` según desde dónde se abra.

**Paso 3 — Publicar la aplicación autocontenida:**

```
dotnet publish src\PQRS.Desktop\PQRS.Desktop.csproj -c Release -r win-x64 --self-contained true -o installer\publish
```

| Parte del comando | Qué hace |
|---|---|
| `dotnet publish` | Compila y deja todo lo necesario para ejecutar en una carpeta |
| `-c Release` | Compilación optimizada (no la de depuración) |
| `-r win-x64` | Apunta a Windows de 64 bits |
| `--self-contained true` | Incluye el runtime de .NET dentro de la carpeta |
| `-o installer\publish` | Carpeta de salida (se genera y está en `.gitignore`) |

Resultado: unos 481 archivos y ~168 MB, con `PQRS.Desktop.exe` incluido.

**Paso 4 — Probar que lo publicado arranca** (antes de empaquetar). Se ejecutó el `.exe` publicado desde **otra carpeta** (`C:\Windows`), para comprobar que `appsettings.json` se encuentra junto al ejecutable: apareció la ventana "PQRS - Iniciar sesión", lo que confirma que la configuración se leyó bien.

**Paso 5 — Escribir el script del instalador** `installer/PQRS.iss` (explicado en 21.5 y 21.6).

**Paso 6 — Escribir `installer/build-installer.ps1`**, que automatiza los pasos 3 y 7: lee la versión del `.csproj`, publica, localiza `ISCC.exe` y compila el instalador.

**Paso 7 — Compilar el instalador:**

```
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
```
Internamente ejecuta `ISCC.exe /DAppVersion=1.0.0 installer\PQRS.iss`. El resultado queda en `installer\Output\PQRS-Setup-1.0.0.exe`.

### 21.4 Cómo generar una versión nueva

1. Cierre la aplicación si la tiene abierta.
2. Suba `<Version>` en `PQRS.Desktop.csproj` (por ejemplo `1.0.1`).
3. Ejecute:
   ```
   powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
   ```
4. Entregue el archivo de `installer\Output\`.
5. Si la versión cambió algo de la base de datos, entregue también el **script SQL** correspondiente (el instalador no toca la base).

El `AppId` del script **nunca debe cambiar**: es lo que le permite a Windows reconocer que una versión nueva es una actualización de la misma aplicación y no una instalación aparte.

### 21.5 El script `PQRS.iss` explicado por secciones

| Sección | Qué hace |
|---|---|
| Líneas `#define` / `#ifndef` | Constantes: nombre, ejecutable, carpeta publicada y versión (`AppVersion`, que llega desde el script con `/DAppVersion=`) |
| `[Setup]` | Datos generales: `AppId` (identificador único), carpeta de instalación (`{autopf}\PQRS`), compresión, permisos de administrador (`PrivilegesRequired=admin`), solo Windows 64 bits (`ArchitecturesAllowed=x64compatible`), Windows 10 o superior (`MinVersion`), nombre del instalador generado (`OutputBaseFilename`) |
| `CloseApplications=yes` | Si la app está abierta durante una actualización, el instalador la cierra para poder reemplazar archivos |
| `UsePreviousAppDir=yes` | En una actualización reutiliza la carpeta elegida antes |
| `SetupLogging=yes` | Deja un registro de la instalación en `%TEMP%` (`Setup Log AAAA-MM-DD #NNN.txt`), muy útil para diagnosticar fallos |
| `[Languages]` | Español (`compiler:Languages\Spanish.isl`, viene con Inno) |
| `[Tasks]` | La casilla opcional "Crear un acceso directo en el escritorio" |
| `[Files]` | Qué se copia: toda la carpeta `publish` (**excluyendo** `appsettings.json` y `*.pdb`), los scripts SQL a `{app}\database` y el manual a `{app}\docs` |
| `[Dirs]` | Crea `C:\ProgramData\PQRS\Adjuntos` con permiso de escritura para todos los usuarios del equipo (carpeta de adjuntos por defecto) |
| `[Icons]` | Accesos directos del menú Inicio y del escritorio (con `WorkingDir` = carpeta de la app) |
| `[Run]` | Casilla final "Abrir Sistema PQRS" (se omite en instalaciones silenciosas) |
| `[UninstallDelete]` | Al desinstalar borra `appsettings.json` (tiene la clave de la base). **No** borra datos ni adjuntos |
| `[Code]` | Código en Pascal Script: el asistente de configuración y la generación de `appsettings.json` (21.6) |

Las constantes entre llaves (`{app}`, `{autopf}`, `{commonappdata}`…) las resuelve Inno al instalar: `{app}` es la carpeta de instalación, `{autopf}` es `Program Files` (o `Program Files (x86)` según el caso), `{commonappdata}` es `C:\ProgramData`.

### 21.6 El asistente y cómo se genera `appsettings.json`

Todo está en la sección `[Code]` del `.iss`.

**Pantallas que se agregan** (en `InitializeWizard`, después de la pantalla de tareas):

| Pantalla | Campos |
|---|---|
| **Base de datos** | Servidor, nombre de la base (por defecto `PQRS`), usuario y contraseña de SQL Server |
| **Configuración inicial** | Carpeta de adjuntos (vacía = `C:\ProgramData\PQRS\Adjuntos`), usuario y contraseña del administrador inicial (con confirmación) |

**Validaciones** (`NextButtonClick`): todos los datos de conexión son obligatorios; el usuario administrador debe cumplir el mismo formato que exige la app (3 a 50 caracteres entre letras, números, `.`, `-`, `_`); la contraseña debe tener mínimo 8 caracteres con letras y números; y la confirmación debe coincidir.

> ⚠️ Estas reglas están **duplicadas** del código de la aplicación (`PoliticaClaves.cs` y la expresión regular de `UsuarioAdminService.cs`). Si cambia la política de contraseñas o de usuarios en C#, **actualice también el `.iss`** (funciones `ClaveValida` y `UsuarioValido`).

**Cuándo se saltan las pantallas** (`ShouldSkipPage`): si ya existe `{app}\appsettings.json`, se considera una **actualización** y no se pregunta nada. Consecuencia práctica: para reconfigurar una instalación existente hay que editar ese archivo a mano.

**Cómo se genera el archivo** (`CurStepChanged`, paso `ssPostInstall`, es decir, justo después de copiar los archivos):

1. Si el archivo ya existe, no hace nada (no sobrescribe).
2. Arma la cadena de conexión: `Server=...;Database=...;User Id=...;Password=...;TrustServerCertificate=True;`.
3. Escribe el JSON completo con `SaveStringsToUTF8File`.

Dos funciones auxiliares evitan que un dato "rompa" el archivo:

| Función | Problema que evita |
|---|---|
| `ValorConexion` | Si un valor (por ejemplo una contraseña) contiene `;`, `=` o comillas, rompería la cadena de conexión. Se encierra entre comillas dobles (y las comillas internas se duplican), que es la forma correcta en ADO.NET |
| `EscaparJson` | Las barras invertidas (`\`) y comillas dentro del texto deben escaparse en JSON. Importante porque un servidor como `SERVIDOR\INSTANCIA` o una ruta `\\SERVIDOR\carpeta` llevan `\` |

**Si faltan los datos de conexión** (por ejemplo, instalación silenciosa sin parámetros) el instalador **no** crea el archivo, lo anota en su registro y, si no es silenciosa, muestra un aviso.

### 21.7 Instalar, actualizar, desinstalar y modo silencioso

| Acción | Qué ocurre |
|---|---|
| **Instalación nueva** | Copia todo, muestra el asistente y genera `appsettings.json` |
| **Actualización** (ejecutar un instalador más nuevo) | Cierra la app, reemplaza los archivos, **no pregunta** nada y **no toca** `appsettings.json`, la base ni los adjuntos |
| **Desinstalación** | Quita la app, accesos directos y `appsettings.json`. Deja la base de datos y los adjuntos |
| **Instalación silenciosa** | `PQRS-Setup-1.0.0.exe /VERYSILENT /Servidor="..." /BaseDatos="PQRS" /UsuarioBD="..." /ClaveBD="..." /AdminUsuario="admin" /AdminClave="..." /RutaAdjuntos="..."` |

Los parámetros `/Servidor=`, `/UsuarioBD=`, etc. se leen con la constante de Inno `{param:Nombre|valorPorDefecto}`, y sirven de valor inicial de los campos del asistente. **Ojo:** al pasar claves por la línea de comandos, estas pueden quedar visibles en el historial o en los procesos; úselo solo en entornos controlados.

### 21.8 Cómo modificar el instalador

**Agregar una opción de configuración nueva** (por ejemplo, un parámetro más en `appsettings.json`):
1. Agregue el campo en la pantalla correspondiente en `InitializeWizard` (`PaginaApp.Add(...)`), y su `{param:...}` por si se usa en modo silencioso.
2. Valídelo en `NextButtonClick` si hace falta.
3. Escríbalo en el JSON en `CurStepChanged` (recuerde ajustar el tamaño de `SetArrayLength` y los números de línea, y las comas del JSON).
4. Asegúrese de que la aplicación lea ese parámetro (`configuration["..."]`).

**Incluir un archivo extra en la instalación:** agregue una línea en `[Files]`, por ejemplo `Source: "..\mi-archivo.pdf"; DestDir: "{app}\docs"; Flags: ignoreversion`. Las rutas del `Source` son relativas a la carpeta `installer/`.

**Cambiar el nombre, la carpeta o el idioma:** `AppName`, `DefaultDirName` y `[Languages]` en el `.iss`.

**Firmar el instalador** (cuando se tenga certificado): Inno admite `SignTool` en `[Setup]` (`SignTool=...` y `SignedUninstaller=yes`). Con eso desaparece la advertencia de SmartScreen.

**Cambiar qué se publica** (por ejemplo, agregar `-p:PublishReadyToRun=true` para arranque más rápido): se hace en `build-installer.ps1`, en el comando `dotnet publish`.

### 21.9 Reglas al editar estos archivos (¡importante!)

- **Guarde `PQRS.iss`, `build-installer.ps1` y `README.md` como "UTF-8 con BOM".** El compilador de Inno y Windows PowerShell 5.1 interpretan los archivos sin BOM con la página de códigos local, y los acentos y la `ñ` se dañan (aparecen como `Ã³`). En Visual Studio Code: botón de codificación de la barra inferior → *Save with Encoding* → *UTF-8 with BOM*.
- **No suba** `installer/publish/` ni `installer/Output/` al repositorio (ya están en `installer/.gitignore`). Los `.exe` generados se entregan por otro medio.
- **No cambie el `AppId`** del `.iss`.
- Después de editar el `.iss`, **recompile** con `build-installer.ps1` para detectar errores de sintaxis del Pascal Script.

### 21.10 Qué se probó y qué no

| Verificado | No verificado todavía |
|---|---|
| El proyecto publica sin errores (481 archivos, ~168 MB) | El flujo completo del asistente en un equipo limpio |
| El ejecutable publicado arranca desde otra carpeta y muestra el login (la configuración se lee bien) | La generación real de `appsettings.json` con las respuestas del asistente |
| El script compila sin errores y genera `PQRS-Setup-1.0.0.exe` (≈ 50 MB) | Actualización sobre una instalación existente |
| | Desinstalación |
| | Instalación silenciosa con parámetros |

**Lista de comprobación para probarlo** en un equipo limpio (o máquina virtual):
- [ ] El asistente muestra las dos pantallas de configuración y rechaza datos incompletos, contraseñas cortas o que no coinciden.
- [ ] Existe `C:\Program Files\PQRS\appsettings.json` con los datos escritos.
- [ ] La aplicación abre y el login funciona (con la base ya creada en el servidor).
- [ ] Se puede radicar una solicitud y adjuntar un archivo.
- [ ] Al ejecutar de nuevo el mismo instalador, **no** vuelve a preguntar la configuración y `appsettings.json` queda intacto.
- [ ] Al desinstalar, se eliminan la app y `appsettings.json`, pero no la base ni los adjuntos.
- [ ] Probar con una contraseña de SQL que contenga `;` o `"` y con un servidor del tipo `SERVIDOR\INSTANCIA`, para validar el escapado.

### 21.11 Problemas que aparecieron al construirlo (y cómo se resolvieron)

| Problema | Causa | Solución |
|---|---|---|
| El compilador de Inno falló con `Unknown type 'TInputQueryPage'` | El tipo se llama **`TInputQueryWizardPage`** | Corregir el nombre en la sección `[Code]` |
| Riesgo de acentos dañados en `.iss` y `.ps1` | Los archivos UTF-8 **sin BOM** se leen como ANSI | Se guardaron **con BOM** (21.9) |
| Con la app abierta falla la copia de archivos al compilar el proyecto | El proceso `PQRS.Desktop` bloquea los `.dll` | Cerrar la aplicación antes de compilar |
| `appsettings.json` podría no encontrarse al abrir desde un acceso directo | WPF/Host buscan el archivo en la *carpeta de trabajo*, no junto al `.exe` | `UseContentRoot(AppContext.BaseDirectory)` (sección 5) |
| El script de compilación no encontraba `ISCC.exe` | Inno por `winget` se instala **por usuario** en `%LOCALAPPDATA%\Programs\Inno Setup 6\` | `build-installer.ps1` busca en las tres rutas habituales y en el `PATH` |

### 21.12 Mejoras pendientes del instalador

| Mejora | Detalle |
|---|---|
| **Firma digital** | Elimina la advertencia de SmartScreen. Requiere certificado de firma de código |
| **Cifrar la clave de la base** | Hoy `appsettings.json` guarda la contraseña en texto plano y es legible por los usuarios del equipo. Opciones: cifrarla (DPAPI), usar variables de entorno o autenticación integrada de Windows |
| **Quitar `Seed:AdminClave` tras el primer arranque** | La clave del administrador inicial solo sirve una vez, pero queda en el archivo |
| **Probar la conexión desde el asistente** | Un botón que valide servidor, usuario y clave antes de continuar |
| **Aplicar scripts de base de datos nuevos** | Hoy es manual. Se puede automatizar con una tabla de versiones de esquema y scripts numerados |
| **Actualización automática** | Que la app detecte una versión nueva del instalador |
| **Copias de seguridad** | Fuera del instalador: respaldo programado de la base y de la carpeta de adjuntos |

---

## Resumen de una página (para repasar)

1. **Cuatro proyectos**: Domain (negocio puro) → Application (contratos y DTOs) → Infrastructure (EF Core, archivos, Excel) → Desktop (WPF). Las dependencias solo van hacia abajo.
2. **Cada pantalla** = View (XAML) + ViewModel (datos y comandos) + un `DataTemplate` en `App.xaml` que los une.
3. **El ViewModel nunca toca la base**: llama a una interfaz (`I…Service`) de Application, cuya implementación en Infrastructure usa EF Core.
4. **Los errores de negocio** se lanzan como `ReglaNegocioException` y el ViewModel muestra su mensaje en rojo.
5. **Los permisos** se muestran en la interfaz y se **validan de verdad en los servicios** consultando la base.
6. **Nada se borra**: se desactiva (`Activo = false`) y todo cambio importante queda en `Seguimiento`.
7. **La base se cambia con scripts SQL** (no hay migraciones de EF): si cambia una entidad, cambie la tabla.
8. **El vencimiento** = fecha de radicación + días hábiles (lunes a viernes, sin festivos) del **tipo** de solicitud; la prioridad no influye.
9. **DbContext y servicios son Transient**: cada ViewModel recibe el suyo, para no mostrar datos viejos.
10. Para cualquier cambio, empiece por la receta correspondiente de la sección 17.
11. **El instalador** (`installer/`) se genera con `installer\build-installer.ps1` (publica la app autocontenida y compila con Inno Setup). Genera `appsettings.json` desde un asistente, no toca la base de datos ni los adjuntos, y sus archivos se guardan como UTF-8 con BOM (sección 21).
