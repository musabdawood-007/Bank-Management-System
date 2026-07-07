; ============================================================================
;  Bank Management System  --  Inno Setup 6 installer script
;
;  HOW TO USE
;  1. Publish the WPF app first:
;       dotnet publish BankManagementSystem.csproj ^
;           -c Release -r win-x64 --self-contained false -o publish
;
;  2. (Optional) Drop an icon file named app.ico next to this .iss.
;
;  3. Open this file in Inno Setup Studio 6 and press Ctrl+F9 (Compile).
;     The installer will be generated in .\InstallerOutput\
;
;  Tip: For a self-contained installer (no .NET runtime needed on the user
;       machine), change --self-contained false to true and remove the
;       .NET runtime check in the [Code] section below.
; ============================================================================

#define MyAppName          "Bank Management System"
#define MyAppShortName     "BankManagementSystem"
#define MyAppVersion       "1.0.0"
#define MyAppPublisher     "OOP_DB_Project_BMS"
#define MyAppExeName       "BankManagementSystem.exe"
#define MyAppURL           "https://github.com/your-username/OOP_DB_Project_BMS"

[Setup]
; NOTE: Generate your own GUID via Ctrl+G in Inno Setup. Keep the double {{.
AppId={{8F4C2A1B-3D5E-4F6A-9B7C-1E2F3A4B5C6D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppShortName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=.\InstallerOutput
OutputBaseFilename={#MyAppShortName}_Setup_v{#MyAppVersion}
; Optional: place an app.ico next to this .iss to brand the installer
SetupIconFile=.\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
MinVersion=10.0.18362

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; --- The published WPF app (from dotnet publish output) ---
Source: "publish\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion
; --- Bundle the SQL schema so users can recreate the database after install ---
Source: "SQL\*"; DestDir: "{app}\SQL"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Open Database Schema (SQL)"; Filename: "notepad.exe"; Parameters: "{app}\SQL\database_schema.sql"; IconFilename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Launch the app after install (only when not run silently)
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Clean up any local app data created at runtime
Type: filesandordirs; Name: "{localappdata}\{#MyAppShortName}"

; ---------------------------------------------------------------------------
;  Pre-install: detect .NET 8 Desktop Runtime, prompt to download if missing
; ---------------------------------------------------------------------------
[Code]

// Returns True if ANY version of the .NET Desktop Runtime 8.x is installed.
// Checked via the registry key written by the .NET installer.
function IsDotNet8DesktopRuntimeInstalled: Boolean;
begin
  Result :=
    RegKeyExists(HKLM, 'SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App')
    or RegKeyExists(HKLM, 'SOFTWARE\dotnet\Setup\InstalledVersions\x64\sharedfx\Microsoft.WindowsDesktop.App');
end;

function InitializeSetup: Boolean;
var
  ErrorCode: Integer;
begin
  Result := True;

  if not IsDotNet8DesktopRuntimeInstalled then
  begin
    if MsgBox(
        'Bank Management System requires the .NET 8 Desktop Runtime, which was not detected on this machine.' #13#10 #13#10
        'Would you like to download it now from Microsoft?',
        mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open',
        'https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime',
        '', '', SW_SHOW, ewNoWait, ErrorCode);
    end;

    Result := (MsgBox(
        'The installer will continue, but the application may fail to start until the .NET 8 Desktop Runtime is installed.' #13#10 #13#10
        'Continue with the installation anyway?',
        mbConfirmation, MB_YESNO) = IDYES);
  end;
end;

// Customize the "Ready to Install" memo so the user is reminded to set up the DB.
function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo,
                         MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo,
                         MemoTasksInfo: String): String;
begin
  Result := '';
  if MemoDirInfo   <> '' then Result := Result + MemoDirInfo   + NewLine + NewLine;
  if MemoGroupInfo <> '' then Result := Result + MemoGroupInfo + NewLine + NewLine;
  if MemoTasksInfo <> '' then Result := Result + MemoTasksInfo + NewLine + NewLine;

  Result := Result + 'Database setup (after install):' + NewLine;
  Result := Result + Space + '1. Open SSMS 22 and connect to your SQL Server.' + NewLine;
  Result := Result + Space + '2. Open and Execute:  ' + NewLine;
  Result := Result + Space + '   {app}\SQL\database_schema.sql' + NewLine;
  Result := Result + Space + '3. Launch {#MyAppExeName} and log in.' + NewLine;
end;

; ============================================================================
;  End of installer.iss
; ============================================================================
