[Setup]
AppId=Veltrivon.Desktop
AppName=Veltrivon
AppVersion={#AppVersion}
DefaultDirName={localappdata}\Programs\Veltrivon
DefaultGroupName=Veltrivon
PrivilegesRequired=lowest
MinVersion=10.0.22000
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#AssetDir}
OutputBaseFilename=Veltrivon-Setup-win-x64
Compression=lzma2/fast
SolidCompression=yes
UninstallDisplayIcon={app}\Veltrivon.App.exe
CloseApplications=yes

[Files]
Source: "{#BundleDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Veltrivon"; Filename: "{app}\Veltrivon.App.exe"; WorkingDir: "{app}"
Name: "{group}\Developer Tool Installer"; Filename: "{app}\Tools\Veltrivon-Tools-win-x64.exe"; WorkingDir: "{app}\Tools"
