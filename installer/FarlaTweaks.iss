#define AppName "Farla Tweaks"
#define AppVersion "0.2.0-alpha.1"
#define AppPublisher "Farlapata"
#define AppExeName "FarlaTweaks.exe"

[Setup]
AppId={{9EAF4A7C-4D60-4E4B-9A7A-0D0E9E1C8F21}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\Farla Tweaks
DefaultGroupName=Farla Tweaks
OutputDir=..\artifacts\installer
OutputBaseFilename=FarlaTweaks-{#AppVersion}-Setup
Compression=lzma2
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64
WizardStyle=modern
DisableProgramGroupPage=yes
UninstallDisplayIcon={app}\{#AppExeName}

[Files]
Source: "..\artifacts\FarlaTweaks\FarlaTweaks.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autodesktop}\Farla Tweaks"; Filename: "{app}\{#AppExeName}"
Name: "{group}\Farla Tweaks"; Filename: "{app}\{#AppExeName}"

[Run]
Filename: "{app}\{#AppExeName}"; Description: "Launch Farla Tweaks"; Flags: nowait postinstall skipifsilent
