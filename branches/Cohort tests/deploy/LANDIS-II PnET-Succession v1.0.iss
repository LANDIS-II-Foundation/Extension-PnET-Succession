#define DeployFolder AddBackslash("C:\Users\adebruij\Desktop\PnET_Succession\InstallerFolder\PnET_Succession\deploy")


#define PackageName      "PnET_Succession"
#define PackageNameLong  "PnET_Succession Extension"
#define Version          "1.0"
#define ReleaseType      "alpha"
#define ReleaseNumber    "1"

#define CoreVersion      "6.0"
#define CoreReleaseAbbr  ""


#include DeployFolder + "package (Setup section) v6.0.iss"

;#include AddBackslash(GetEnv("LANDIS_DEPLOY")) + "package (Setup section) v6.0.iss"

#if ReleaseType != "official"
  #define Configuration  "debug"
#else
  #define Configuration  "release"
#endif


[Files]
 
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Extension.Succession.BiomassPnET.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Extension.Succession.Biomass.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Library.BiomassCohorts.dll; DestDir: {app}\bin; Flags: replacesameversion
Source: C:\Program Files\LANDIS-II\v6\bin\extensions\Landis.Library.BiomassCohortsPnET.dll; DestDir: {app}\bin; Flags: replacesameversion

Source: examples\SIMULATION_c1c\*; DestDir: C:\Program Files\LANDIS-II\v6\examples\PnET_Succession\SIMULATION_c1c
Source: examples\SIMULATION_sm\*; DestDir: C:\Program Files\LANDIS-II\v6\examples\PnET_Succession\SIMULATION_sm

#define PnET_Succession "PnET_Succession 1.0.txt"
Source: {#PnET_Succession}; DestDir: {#LandisPlugInDir}

[Run]
;; Run plug-in admin tool to add the entry for the plug-in
#define PlugInAdminTool  PluginAdminDir + "\Landis.PlugIns.Admin.exe"

Filename: {#PlugInAdminTool}; Parameters: "remove ""PnET_Succession"" "; WorkingDir: {#LandisPlugInDir}
Filename: {#PlugInAdminTool}; Parameters: "add ""{#PnET_Succession}"" "; WorkingDir: {#LandisPlugInDir}


[Code]
{ Check for other prerequisites during the setup initialization }

#include DeployFolder + "package (Code section) v3.iss"

//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------

function CurrentVersion_PostUninstall(currentVersion: TInstalledVersion): Integer;
begin
end;


//-----------------------------------------------------------------------------

function InitializeSetup_FirstPhase(): Boolean;
begin
  CurrVers_PostUninstall := @CurrentVersion_PostUninstall
  Result := True
end;
