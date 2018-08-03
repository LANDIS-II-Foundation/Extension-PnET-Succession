# Support-Library-Dlls
This repository is specifically designated for providing a stable URL for building extensions.  This is, in effect, **LandisGet**.  The repository is designed to work with the [**Download Tool**](https://github.com/LANDIS-II-Foundation/Tool-Download-Current-Dlls) and with the project file (.csproj) for each library or extension.  To activiate **LandisGet**:  
a) make sure the WinPkgTools directory  is in the src directory [**Example Here**](https://github.com/LANDIS-II-Foundation/Tool-Download-Current-Dlls).  
b) In your project file, add the PreBuildEvent "call $(SolutionDir)\install-libs.cmd" In the .csproj xml, this is listed as: 
```
<PropertyGroup>
    <PreBuildEvent>call $(SolutionDir)\install-libs.cmd</PreBuildEvent>
</PropertyGroup>
```

c) To initially load or reload the dlls, perform a 'Rebuild' after opening the project file.

Changes to this extension are governed by the [**Repository Rules**](https://sites.google.com/site/landismodel/developers) from the Technical Advisory Committee.



