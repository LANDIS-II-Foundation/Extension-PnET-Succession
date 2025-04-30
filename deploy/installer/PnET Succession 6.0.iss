; LANDIS-II Extension infomation
#define CoreRelease "LANDIS-II-V8"
#define ExtensionName "PnET-Succession"
#define AppVersion "6.0"
#define AppPublisher "LANDIS-II Foundation"
#define AppURL "http://www.landis-ii.org/"

; Build directory
#define BuildDir "..\..\src\bin\Release"
;define BuildDir "..\..\src\bin\Debug"
;#define BuildDir ".."

; LANDIS-II installation directories
#define ExtDir "C:\Program Files\LANDIS-II-v8\extensions"
#define AppDir "C:\Program Files\LANDIS-II-v8"
#define LandisPlugInDir "C:\Program Files\LANDIS-II-v8\plug-ins-installer-files"
#define ExtensionsCmd AppDir + "\commands\landis-ii-extensions.cmd"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{C7B48755-64C6-42F9-8544-802FA84D1959}
AppName={#CoreRelease} {#ExtensionName}
AppVersion={#AppVersion}
; Name in "Programs and Features"
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
OutputBaseFilename={#CoreRelease} {#ExtensionName} {#AppVersion}-setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"


[Files]
; This .dll IS the extension (ie, the extension's assembly)
; NB: Do not put an additional version number in the file name of this .dll
; (The name of this .dll is defined in the extension's \src\*.csproj file)
Source: {#BuildDir}\Landis.Extension.Succession.BiomassPnET-v6.dll; DestDir: {#ExtDir}; Flags: ignoreversion

; Requisite auxiliary libraries
; NB. These libraries are used by other extensions and thus are never uninstalled.
Source: {#BuildDir}\Landis.Library.UniversalCohorts-v1.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion  
Source: {#BuildDir}\Landis.Library.Climate-v5.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.InitialCommunity.Universal.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.Metadata-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion
Source: {#BuildDir}\Landis.Library.Parameters-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion   
Source: {#BuildDir}\Landis.Library.PnETCohorts-v2.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion 
Source: {#BuildDir}\Landis.Library.Succession-v9.dll; DestDir: {#ExtDir}; Flags: uninsneveruninstall ignoreversion

; Complete example for testing the extension
Source: ..\examples\biomass-Pnet-succession-example-v8\*.txt; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: ignoreversion
Source: ..\examples\biomass-Pnet-succession-example-v8\*.gis; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: ignoreversion skipifsourcedoesntexist
Source: ..\examples\biomass-Pnet-succession-example-v8\*.img; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: ignoreversion skipifsourcedoesntexist
Source: ..\examples\biomass-Pnet-succession-example-v8\*.bat; DestDir: {#AppDir}\examples\{#ExtensionName}; Flags: ignoreversion skipifsourcedoesntexist

Source: ..\Defaults\PnETGenericDefaultParameters.txt; DestDir: {#ExtDir}\Defaults; Flags: ignoreversion
Source: ..\Defaults\VanGenuchtenParameters.txt; DestDir: {#ExtDir}\Defaults; Flags: ignoreversion
Source: ..\Defaults\SaxtonAndRawlsParameters.txt; DestDir: {#ExtDir}\Defaults; Flags: ignoreversion

; LANDIS-II identifies the extension with the info in this .txt file
; NB. New releases must modify the name of this file and the info in it
#define InfoTxt "PnET-Succession.txt"
Source: {#InfoTxt}; DestDir: {#LandisPlugInDir}
; NOTE: Don't use "Flags: ignoreversion" on any shared system files


[Run]
Filename: {#ExtensionsCmd}; Parameters: "remove ""PnET-Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#ExtensionsCmd}; Parameters: "add ""{#InfoTxt}"" "; WorkingDir: {#LandisPlugInDir} 


[UninstallRun]
; Remove "Age-Only Succession" from "extensions.xml" file.
Filename: {#ExtensionsCmd}; Parameters: "remove ""PnET-Succession"" "; WorkingDir: {#LandisPlugInDir}


