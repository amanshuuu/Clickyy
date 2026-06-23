; Inno Setup Script for Clicky Windows
; Creates a professional installer that handles everything
;
; To compile: Open this file in Inno Setup Compiler → Build
; Download Inno Setup from: https://jrsoftware.org/isinfo.php

#define MyAppName "Clicky"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Clicky"
#define MyAppURL "https://github.com/clicky-windows"
#define MyAppExeName "Clicky.exe"

[Setup]
AppId={{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\Clicky
DefaultGroupName=Clicky
AllowNoIcons=yes
LicenseFile=
OutputDir=.
OutputBaseFilename=Clicky-Setup-1.0.0
SetupIconFile=..\Resources\clicky.ico
UninstallDisplayIcon={app}\Clicky.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
DisableProgramGroupPage=yes
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop shortcut"; GroupDescription: "Additional shortcuts:"; Flags: checkedonce
Name: "startup"; Description: "Launch Clicky &automatically when I log in"; GroupDescription: "Startup behavior:"; Flags: checkedonce

[Files]
Source: "Clicky.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Clicky"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Comment: "Clicky - AI Buddy"
Name: "{group}\Uninstall Clicky"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Clicky"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon; WorkingDir: "{app}"
Name: "{userstartup}\Clicky"; Filename: "{app}\{#MyAppExeName}"; Tasks: startup; WorkingDir: "{app}"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch Clicky now"; Flags: postinstall nowait skipifsilent

[UninstallRun]
Filename: "{app}\uninstall.ps1"; Flags: runhidden skipifdoesntexist

[Code]
function InitializeSetup: Boolean;
begin
  Result := True;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Create settings directory
    SaveStringToFile(ExpandConstant('{userappdata}\Clicky\settings.json'), '{}', False);
  end;
end;
