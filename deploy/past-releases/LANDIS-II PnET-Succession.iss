#include GetEnv("LANDIS_SDK") + '\packaging\initialize.iss'
;#include  "C:\Program Files\LANDIS-II\SDK\packaging\initialize.iss"
#define ExtInfoFile "PnET-Succession.txt"

#include LandisSDK + '\packaging\read-ext-info.iss'
#include LandisSDK + '\packaging\Landis-vars.iss'

[Setup]
#include LandisSDK + '\packaging\Setup-directives.iss'
LicenseFile={#LandisSDK}\licenses\LANDIS-II_Binary_license.rtf

[Files]
Source: {#LandisExtDir}\{#ExtensionAssembly}.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
Source: ..\..\Support-Library-Dlls\Landis.Library.AgeOnlyCohorts.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
Source: ..\..\Support-Library-Dlls\Landis.Library.Cohorts.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
Source: ..\..\Support-Library-Dlls\Landis.Library.Biomass-v1.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
Source: ..\..\Support-Library-Dlls\Landis.Library.BiomassCohorts-v2.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
Source: ..\..\Support-Library-Dlls\Landis.Library.Succession-v5.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
Source: ..\..\Support-Library-Dlls\Landis.Library.Parameters-v1.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion

Source: {#LandisExtDir}\Defaults\PnETGenericDefaultParameters.txt; DestDir: {app}\bin\extensions\Defaults; Flags: replacesameversion
Source: ..\..\Support-Library-Dlls\Defaults\VanGenuchtenParameters.txt; DestDir: {app}\bin\extensions\Defaults; Flags: replacesameversion
Source: ..\..\Support-Library-Dlls\Defaults\SaxtonAndRawlsParameters.txt; DestDir: {app}\bin\extensions\Defaults; Flags: replacesameversion

#define UserGuideSrc "LANDIS-II " + ExtensionName + " vX.Y User Guide.pdf"
#define UserGuide    StringChange(UserGuideSrc, "X.Y", MajorMinor)
Source: docs\{#UserGuide}; DestDir: {app}\docs; DestName: {#UserGuide}
#define Worksheet "PnET-Succession function worksheet.xlsx"
Source: docs\{#Worksheet}; DestDir:  {app}\docs; DestName: {#Worksheet}

Source: examples\*; DestDir: {app}\examples\{#ExtensionName}\{#MajorMinor}; Flags: recursesubdirs

#define ExtensionInfo  ExtensionName + " " + MajorMinor + ".txt"
Source: {#ExtInfoFile}; DestDir: {#LandisExtInfoDir}; DestName: {#ExtensionInfo}

[Run]
Filename: {#ExtAdminTool}; Parameters: "remove ""{#ExtensionName}"" "; WorkingDir: {#LandisExtInfoDir}
Filename: {#ExtAdminTool}; Parameters: "add ""{#ExtensionInfo}"" "; WorkingDir: {#LandisExtInfoDir}

[UninstallRun]
Filename: {#ExtAdminTool}; Parameters: "remove ""{#ExtensionName}"" "; WorkingDir: {#LandisExtInfoDir}

[Code]
#include LandisSDK + '\packaging\Pascal-code.iss'

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  Result := True
end;
