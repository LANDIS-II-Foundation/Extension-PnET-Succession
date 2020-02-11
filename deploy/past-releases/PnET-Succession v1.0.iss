#include GetEnv("LANDIS_SDK") + '\packaging\initialize.iss'

#define ExtInfoFile "PnET-Succession.txt"

#include LandisSDK + '\packaging\read-ext-info.iss'
#include LandisSDK + '\packaging\Landis-vars.iss'

; The ExtensionName in the info file includes the major and minor version
; numbers (X.Y) to allow for multiple versions of the extension to be
; installed simultaneously.  That enables users to run tests comparing two
; versions (for example, was a bug fixed in the latest version?).
;
; The Setup directives expect the ExtensionName variable to NOT have any
; version numbers.  Therefore, we strip it out.
#define ExtensionName_X_Y ExtensionName
#define ExtensionName     Trim(StringChange(ExtensionName_X_Y, MajorMinor, ""))

[Setup]
#include LandisSDK + '\packaging\Setup-directives.iss'
;LicenseFile=..\Apache-License-2.0.txt

[Files]
#define ConfigOutDir '..\src\bin\Debug'

; The extension's assembly
Source: {#LandisExtDir}\Landis.Extension.Succession.BiomassPnET.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
Source: {#LandisExtDir}\Landis.Library.Biomass-v1.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
Source: {#LandisExtDir}\Landis.Library.BiomassCohorts.dll; DestDir: {app}\bin\extensions; Flags: replacesameversion
 

; The user guide
#define UserGuideSrc "LANDIS-II " + ExtensionName + " vX.Y User Guide.pdf"
#define UserGuide    StringChange(UserGuideSrc, "X.Y", MajorMinor)
Source: docs\{#UserGuideSrc}; DestDir: {app}\docs; DestName: {#UserGuide}

; Sample input files
Source: examples\biomass-Pnet-succession-example\*; DestDir: {app}\examples\{#ExtensionName}\{#MajorMinor}\biomass-Pnet-succession-example; Flags: recursesubdirs
Source: examples\OneCellSimulation\*; DestDir: {app}\examples\{#ExtensionName}\{#MajorMinor}\OneCellSimulation; Flags: recursesubdirs


; The extension's info file
#define ExtensionInfo  "PnET_Succession 1.0.txt"
Source: {#ExtInfoFile}; DestDir: {#LandisExtInfoDir}; DestName: {#ExtensionInfo}


[Run]
Filename: {#ExtAdminTool}; Parameters: "remove ""{#ExtensionName_X_Y}"" "; WorkingDir: {#LandisExtInfoDir}
Filename: {#ExtAdminTool}; Parameters: "add ""{#ExtensionInfo}"" "; WorkingDir: {#LandisExtInfoDir}

[UninstallRun]
Filename: {#ExtAdminTool}; Parameters: "remove ""{#ExtensionName_X_Y}"" "; WorkingDir: {#LandisExtInfoDir}

[Code]
#include LandisSDK + '\packaging\Pascal-code.iss'

//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  Result := True
end;
