; -------------------------------------------------------------------
; LANDIS-II PnET-Succession Dynamic Installer Template
; 6.1.2 will be replaced by GitHub Actions at build time.
; -------------------------------------------------------------------

#define CoreRelease "LANDIS-II-V8"
#define ExtensionName "PnET-Succession"
#define AppVersion "6.1.2"
#define AppPublisher "LANDIS-II Foundation"
#define AppURL "http://www.landis-ii.org/"

#define BuildDir "..\..\src\bin\Release"

#define ExtDir "C:\Program Files\LANDIS-II-v8\extensions"
#define AppDir "C:\Program Files\LANDIS-II-v8"
#define LandisPlugInDir "C:\Program Files\LANDIS-II-v8\plug-ins-installer-files"
#define ExtensionsCmd AppDir + "\commands\landis-ii-extensions.cmd"

[Setup]
AppId={{00061EE9-D461-4B71-A2AC-8E5D15F28D6A}
AppName={#CoreRelease} {#ExtensionName}
AppVersion={#AppVersion}
AppVerName={#CoreRelease} {#ExtensionName} v{#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={pf}\{#ExtensionName}
DisableDirPage=yes
DefaultGroupName={#ExtensionName}
DisableProgramGroupPage=yes
LicenseFile=LANDIS-II_Binary_license.rtf
OutputDir={#SourcePath}
OutputBaseFilename={#CoreRelease}_{#ExtensionName}_{#AppVersion}_setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: {#BuildDir}\Landis.Extension.Succession.BiomassPnET-v6.dll; DestDir: {#ExtDir}; Flags: ignoreversion
Source: {#BuildDir}\Landis.Library.UniversalCohorts-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.Climate-v6.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.InitialCommunity.Universal-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.Metadata-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.Parameters-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.PnETCohorts-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.Succession-v10.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion

Source: ..\Defaults\PnETGenericDefaultParameters.txt; DestDir: {#ExtDir}\Defaults; Flags: ignoreversion
Source: ..\Defaults\VanGenuchtenParameters.txt; DestDir: {#ExtDir}\Defaults; Flags: ignoreversion
Source: ..\Defaults\SaxtonAndRawlsParameters.txt; DestDir: {#ExtDir}\Defaults; Flags: ignoreversion

#define InfoTxt "PnET-Succession.txt"
Source: {#InfoTxt}; DestDir: {#LandisPlugInDir}

[Run]
Filename: {#ExtensionsCmd}; Parameters: "remove PnET-Succession"; WorkingDir: {#LandisPlugInDir}
Filename: {#ExtensionsCmd}; Parameters: "add {#InfoTxt}"; WorkingDir: {#LandisPlugInDir}

[UninstallRun]
Filename: {#ExtensionsCmd}; Parameters: "remove PnET-Succession"; WorkingDir: {#LandisPlugInDir}
