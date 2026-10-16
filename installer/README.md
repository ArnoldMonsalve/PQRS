# Instalador de Sistema PQRS

Esta carpeta contiene todo lo necesario para generar el instalador (`PQRS-Setup-<versión>.exe`) y entregarlo a quienes van a usar el sistema.

| Archivo | Para qué sirve |
|---|---|
| `PQRS.iss` | Script de **Inno Setup**: define qué se instala, los accesos directos y el asistente que pide la conexión a la base de datos |
| `build-installer.ps1` | Publica la aplicación y compila el instalador en un solo paso |
| `publish\` | *(se genera)* La aplicación publicada. No se sube al repositorio |
| `Output\` | *(se genera)* Aquí queda el instalador `.exe`. No se sube al repositorio |

---

## 1. Cómo generar el instalador (para el desarrollador)

### Requisitos (una sola vez, en el equipo de desarrollo)
- **.NET 10 SDK** (ya lo tiene si compila el proyecto).
- **Inno Setup 6**: `winget install JRSoftware.InnoSetup` (o descargarlo de jrsoftware.org).

### Pasos
1. Suba el número de versión en `src/PQRS.Desktop/PQRS.Desktop.csproj` (`<Version>1.0.0</Version>`).
2. Ejecute desde la raíz del proyecto:

```
powershell -ExecutionPolicy Bypass -File installer\build-installer.ps1
```

3. El instalador queda en `installer\Output\PQRS-Setup-<versión>.exe` (pesa unos 60–80 MB).

El script hace dos cosas: `dotnet publish` de la aplicación **autocontenida** (incluye el runtime de .NET, así que **los equipos de los usuarios no necesitan instalar nada más**) y luego compila `PQRS.iss`.

### Antes de entregar una versión
- [ ] Subió la versión en el `.csproj`.
- [ ] Compiló y probó la aplicación.
- [ ] Generó el instalador y lo probó en un **equipo limpio** (o máquina virtual) sin Visual Studio ni .NET.
- [ ] Probó también una **actualización** sobre una instalación anterior.
- [ ] Actualizó `documentacion.md` si cambió algo visible para el usuario.

---

## 2. Qué hace el instalador

1. Pide permisos de administrador (instala en `C:\Program Files\PQRS`).
2. Copia la aplicación, los scripts SQL (`{app}\database`) y el manual de uso (`{app}\docs`).
3. **Asistente de configuración** (solo la primera vez):
   - **Conexión a SQL Server:** servidor, base de datos, usuario y contraseña.
   - **Carpeta de adjuntos:** vacía = `C:\ProgramData\PQRS\Adjuntos`. Con varios equipos, debe ser una ruta de red compartida (ej. `\\SERVIDOR\PQRS\Adjuntos`).
   - **Administrador inicial:** usuario y contraseña (mínimo 8 caracteres, con letras y números). Solo se crea si la base aún no tiene usuarios.
4. Genera `appsettings.json` con esos datos.
5. Crea el acceso directo en el menú Inicio (y en el escritorio, si se marca).

### Actualizaciones
Al ejecutar un instalador de una versión nueva sobre una instalación existente:
- Reemplaza los archivos de la aplicación (cierra la aplicación si está abierta).
- **No vuelve a preguntar** la configuración ni **sobrescribe** `appsettings.json`.
- No toca la base de datos ni los archivos adjuntos.

> Si una versión nueva necesita cambios en la base de datos, hay que **entregar el script SQL** correspondiente y ejecutarlo en el servidor. El instalador no modifica la base de datos.

### Desinstalación
Panel de control → *Aplicaciones* → Sistema PQRS. Elimina la aplicación y su `appsettings.json` (que contiene la clave de la base). **No elimina** la base de datos ni los archivos adjuntos.

### Instalación silenciosa (opcional)
Para instalar desde un script, sin ventanas:

```
PQRS-Setup-1.0.0.exe /VERYSILENT /Servidor="SERVIDOR" /BaseDatos="PQRS" /UsuarioBD="usuario" /ClaveBD="clave" /AdminUsuario="admin" /AdminClave="Clave1234" /RutaAdjuntos="\\SERVIDOR\PQRS\Adjuntos"
```
Si faltan los datos de conexión, el instalador **no** crea `appsettings.json` y lo deja anotado en su registro.

---

## 3. La base de datos NO la crea el instalador

La base de datos se prepara **una sola vez, en el servidor**, antes de usar la aplicación. Los scripts quedan en `{app}\database` (y en la carpeta `database` del proyecto):

1. `01_schema.sql` — crea la base `PQRS`, las tablas, los tipos de solicitud y los parámetros iniciales.
2. `02_datos_iniciales.sql` — áreas y categorías de ejemplo (opcional, se pueden ajustar luego desde Administración).

Ejecútelos con SQL Server Management Studio, o con `sqlcmd` indicando UTF-8 para no dañar los acentos:

```
sqlcmd -S SERVIDOR -U USUARIO -P CLAVE -C -b -f 65001 -i 01_schema.sql
sqlcmd -S SERVIDOR -U USUARIO -P CLAVE -C -b -f 65001 -i 02_datos_iniciales.sql
```

Debe existir un **usuario de SQL Server** con permisos de lectura y escritura sobre la base `PQRS` (es el que se escribe en el asistente). El servidor debe permitir autenticación de SQL Server y ser alcanzable por red desde los equipos de los usuarios (puerto 1433 por defecto).

La primera vez que alguien abre la aplicación con la base vacía de usuarios, se crea el administrador con los datos escritos en el asistente.

---

## 4. Cambiar la configuración después de instalar

Edite `C:\Program Files\PQRS\appsettings.json` (requiere permisos de administrador) y reinicie la aplicación:

| Clave | Para qué |
|---|---|
| `ConnectionStrings:PqrsDb` | Conexión a la base de datos |
| `Almacenamiento:RutaAdjuntos` | Carpeta de adjuntos |
| `Seed:*` | Solo se usan cuando la base no tiene usuarios |

---

## 5. Notas de seguridad

- `appsettings.json` guarda la **contraseña de SQL Server en texto plano** y, en la carpeta de Program Files, **puede ser leído por los usuarios del equipo**. Es aceptable en una red interna controlada, pero mejora pendiente: cifrar el archivo o usar autenticación integrada de Windows.
- `Seed:AdminClave` solo sirve en el primer arranque; después de crear el administrador puede borrarse del archivo.
- La cadena de conexión usa `TrustServerCertificate=True` (no valida el certificado del servidor). Si el servidor tiene un certificado válido, quitar esa opción es más seguro.
- El instalador **no está firmado digitalmente**: Windows SmartScreen puede mostrar una advertencia de "editor desconocido". Se soluciona con un certificado de firma de código.
- Los instaladores `.exe` generados **no deben subirse al repositorio** (`Output\` está en `.gitignore`).

---

## 6. Problemas frecuentes

| Problema | Solución |
|---|---|
| `build-installer.ps1` dice "No se encontró Inno Setup 6" | Instálelo: `winget install JRSoftware.InnoSetup` |
| Al abrir la app dice "No se pudo conectar" | Revise `appsettings.json`: servidor, usuario, clave, y que el servidor sea alcanzable por red |
| La app abre pero "Falta la cadena de conexión" | No existe `appsettings.json` junto al `.exe`: se instaló en modo silencioso sin parámetros. Créelo o reinstale |
| SmartScreen bloquea el instalador | Es porque no está firmado: "Más información" → "Ejecutar de todas formas", o firme el instalador |
| Los adjuntos no se ven desde otro equipo | La ruta de adjuntos es local a cada equipo; configure una carpeta de red compartida en `Almacenamiento:RutaAdjuntos` |
