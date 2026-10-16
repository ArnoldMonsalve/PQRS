; ============================================================
;  Instalador de Sistema PQRS  (Inno Setup 6)
;  Compilar con installer\build-installer.ps1 (publica la app y luego ejecuta este script).
;  Más información: installer\README.md
; ============================================================

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define AppName    "Sistema PQRS"
#define AppExe     "PQRS.Desktop.exe"
#define PublishDir "publish"

[Setup]
; Identificador único de la aplicación: NO cambiarlo, así las actualizaciones reemplazan la versión instalada.
AppId={{B6D3F0A2-5C1E-4E7B-9A52-3F7D8C41E6A9}
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\PQRS
UsePreviousAppDir=yes
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=PQRS-Setup-{#AppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.14393
UninstallDisplayIcon={app}\{#AppExe}
CloseApplications=yes
RestartApplications=no
SetupLogging=yes

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "Crear un acceso directo en el escritorio"; GroupDescription: "Accesos directos:"

[Files]
; La app autocontenida (incluye .NET). appsettings.json NO se copia: lo genera el asistente con los datos de cada cliente.
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "appsettings.json,*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "..\database\*.sql"; DestDir: "{app}\database"; Flags: ignoreversion
Source: "..\documentacion.md"; DestDir: "{app}\docs"; Flags: ignoreversion

[Dirs]
; Carpeta de adjuntos por defecto, con permiso de escritura para todos los usuarios del equipo.
Name: "{commonappdata}\PQRS\Adjuntos"; Permissions: users-modify

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExe}"; Description: "Abrir {#AppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; El archivo de configuración contiene la clave de la base de datos: se elimina al desinstalar.
; Los datos (base de datos y archivos adjuntos) NO se tocan.
Type: files; Name: "{app}\appsettings.json"

[Code]
var
  PaginaBD: TInputQueryWizardPage;
  PaginaApp: TInputQueryWizardPage;

{ ---------- Utilidades ---------- }

function ConfigExiste: Boolean;
begin
  Result := FileExists(ExpandConstant('{app}\appsettings.json'));
end;

{ Cadena de conexión: si un valor trae ; = o comillas, se encierra entre comillas dobles. }
function ValorConexion(const Valor: String): String;
var
  R: String;
begin
  R := Valor;
  if (Pos(';', R) > 0) or (Pos('=', R) > 0) or (Pos('"', R) > 0) or (Pos('''', R) > 0) or (R <> Trim(R)) then
  begin
    StringChangeEx(R, '"', '""', True);
    R := '"' + R + '"';
  end;
  Result := R;
end;

{ Texto seguro para ir dentro de una cadena JSON. }
function EscaparJson(const Valor: String): String;
var
  R: String;
begin
  R := Valor;
  StringChangeEx(R, '\', '\\', True);
  StringChangeEx(R, '"', '\"', True);
  Result := R;
end;

{ Misma política que la aplicación: mínimo 8 caracteres, con letras y números. }
function ClaveValida(const Clave: String): Boolean;
var
  I: Integer;
  Ch: Char;
  HayLetra, HayDigito: Boolean;
begin
  HayLetra := False;
  HayDigito := False;
  for I := 1 to Length(Clave) do
  begin
    Ch := Clave[I];
    if (Ch >= '0') and (Ch <= '9') then
      HayDigito := True
    else if ((Ch >= 'A') and (Ch <= 'Z')) or ((Ch >= 'a') and (Ch <= 'z')) then
      HayLetra := True;
  end;
  Result := (Length(Clave) >= 8) and HayLetra and HayDigito;
end;

{ Usuario: de 3 a 50 caracteres entre letras, números, punto, guion y guion bajo. }
function UsuarioValido(const Usuario: String): Boolean;
var
  I: Integer;
  Ch: Char;
begin
  Result := (Length(Usuario) >= 3) and (Length(Usuario) <= 50);
  if not Result then Exit;
  for I := 1 to Length(Usuario) do
  begin
    Ch := Usuario[I];
    if not (((Ch >= '0') and (Ch <= '9')) or ((Ch >= 'A') and (Ch <= 'Z')) or
            ((Ch >= 'a') and (Ch <= 'z')) or (Ch = '.') or (Ch = '-') or (Ch = '_')) then
    begin
      Result := False;
      Exit;
    end;
  end;
end;

{ ---------- Asistente ---------- }

procedure InitializeWizard;
begin
  { Los parámetros /Servidor= /BaseDatos= /UsuarioBD= /ClaveBD= /RutaAdjuntos= /AdminUsuario= /AdminClave=
    permiten instalar sin intervención (por ejemplo con /VERYSILENT). }
  PaginaBD := CreateInputQueryPage(wpSelectTasks,
    'Base de datos',
    'Conexión a SQL Server',
    'Escriba los datos del servidor donde está la base de datos de PQRS. Si no los conoce, pídalos al administrador de sistemas.');
  PaginaBD.Add('Servidor de SQL Server (ej. SERVIDOR  o  SERVIDOR\INSTANCIA):', False);
  PaginaBD.Add('Nombre de la base de datos:', False);
  PaginaBD.Add('Usuario de SQL Server:', False);
  PaginaBD.Add('Contraseña de ese usuario:', True);
  PaginaBD.Values[0] := ExpandConstant('{param:Servidor|}');
  PaginaBD.Values[1] := ExpandConstant('{param:BaseDatos|PQRS}');
  PaginaBD.Values[2] := ExpandConstant('{param:UsuarioBD|}');
  PaginaBD.Values[3] := ExpandConstant('{param:ClaveBD|}');

  PaginaApp := CreateInputQueryPage(PaginaBD.ID,
    'Configuración inicial',
    'Archivos adjuntos y administrador',
    'El administrador inicial solo se crea si la base de datos todavía no tiene usuarios. Si varios equipos usarán el sistema, la carpeta de adjuntos debe ser una ruta de red compartida.');
  PaginaApp.Add('Carpeta de archivos adjuntos (vacío = C:\ProgramData\PQRS\Adjuntos):', False);
  PaginaApp.Add('Usuario del administrador inicial:', False);
  PaginaApp.Add('Contraseña del administrador (mín. 8 caracteres, con letras y números):', True);
  PaginaApp.Add('Confirme la contraseña:', True);
  PaginaApp.Values[0] := ExpandConstant('{param:RutaAdjuntos|}');
  PaginaApp.Values[1] := ExpandConstant('{param:AdminUsuario|admin}');
  PaginaApp.Values[2] := ExpandConstant('{param:AdminClave|}');
  PaginaApp.Values[3] := ExpandConstant('{param:AdminClave|}');
end;

{ En una actualización ya existe la configuración: no se vuelve a preguntar ni se sobrescribe. }
function ShouldSkipPage(PageID: Integer): Boolean;
begin
  Result := False;
  if (PageID = PaginaBD.ID) or (PageID = PaginaApp.ID) then
    Result := ConfigExiste;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;

  if CurPageID = PaginaBD.ID then
  begin
    if (Trim(PaginaBD.Values[0]) = '') or (Trim(PaginaBD.Values[1]) = '') or
       (Trim(PaginaBD.Values[2]) = '') or (PaginaBD.Values[3] = '') then
    begin
      MsgBox('Complete todos los datos de la conexión a la base de datos.', mbError, MB_OK);
      Result := False;
    end;
  end
  else if CurPageID = PaginaApp.ID then
  begin
    if not UsuarioValido(Trim(PaginaApp.Values[1])) then
    begin
      MsgBox('El usuario del administrador debe tener de 3 a 50 caracteres: letras, números, punto, guion o guion bajo, sin espacios.', mbError, MB_OK);
      Result := False;
    end
    else if not ClaveValida(PaginaApp.Values[2]) then
    begin
      MsgBox('La contraseña del administrador debe tener al menos 8 caracteres e incluir letras y números.', mbError, MB_OK);
      Result := False;
    end
    else if PaginaApp.Values[2] <> PaginaApp.Values[3] then
    begin
      MsgBox('Las contraseñas no coinciden.', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

{ Genera appsettings.json con los datos que escribió quien instala. }
procedure CurStepChanged(CurStep: TSetupStep);
var
  Conexion: String;
  Lineas: TArrayOfString;
  Ruta: String;
begin
  if (CurStep <> ssPostInstall) or ConfigExiste then Exit;

  if (Trim(PaginaBD.Values[0]) = '') or (Trim(PaginaBD.Values[2]) = '') or (PaginaBD.Values[3] = '') then
  begin
    Log('No se generó appsettings.json: faltan los datos de conexión (¿instalación silenciosa sin parámetros?).');
    if not WizardSilent then
      MsgBox('No se pudo crear el archivo de configuración. Copie appsettings.json a la carpeta de la aplicación y complete la conexión.', mbError, MB_OK);
    Exit;
  end;

  Conexion := 'Server=' + ValorConexion(Trim(PaginaBD.Values[0])) +
              ';Database=' + ValorConexion(Trim(PaginaBD.Values[1])) +
              ';User Id=' + ValorConexion(Trim(PaginaBD.Values[2])) +
              ';Password=' + ValorConexion(PaginaBD.Values[3]) +
              ';TrustServerCertificate=True;';

  SetArrayLength(Lineas, 13);
  Lineas[0]  := '{';
  Lineas[1]  := '  "ConnectionStrings": {';
  Lineas[2]  := '    "PqrsDb": "' + EscaparJson(Conexion) + '"';
  Lineas[3]  := '  },';
  Lineas[4]  := '  "Almacenamiento": {';
  Lineas[5]  := '    "RutaAdjuntos": "' + EscaparJson(Trim(PaginaApp.Values[0])) + '"';
  Lineas[6]  := '  },';
  Lineas[7]  := '  "Seed": {';
  Lineas[8]  := '    "AdminUsuario": "' + EscaparJson(Trim(PaginaApp.Values[1])) + '",';
  Lineas[9]  := '    "AdminClave": "' + EscaparJson(PaginaApp.Values[2]) + '",';
  Lineas[10] := '    "AdminCorreo": ""';
  Lineas[11] := '  }';
  Lineas[12] := '}';

  Ruta := ExpandConstant('{app}\appsettings.json');
  if not SaveStringsToUTF8File(Ruta, Lineas, False) then
    MsgBox('No se pudo escribir ' + Ruta + '. La aplicación no podrá conectarse hasta que se cree ese archivo.', mbError, MB_OK);
end;
