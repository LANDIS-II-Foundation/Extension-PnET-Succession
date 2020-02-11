using Landis.SpatialModeling;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET
{
    public static class AssignOutputFiles
    {
        // Maps the output files by pixel that are prescribed by the user with the term "PnETOutputSites", i.e. cohort and 
        // location specific output files. The  user can supply the location through the row and column number of the mao
        // location where she wants to see output, or though coordinates X and Y. If using coordinates, the user 
        // should supply the border X, Y coorninates of the map. 
        public struct ParameterNames
        {
            // Output location supplied by row, column: row
            public const string Row = "Row";

            // Output location supplied by row, column: column
            public const string Column = "Column";

            // Output location supplied by coordinate: Y
            public const string MapCoordinatesY = "MapCoordinatesY";

            // Output location supplied by coordinate: X
            public const string MapCoordinatesX = "MapCoordinatesX";

            // Output location supplied by border location: X
            public const string MapCoordinatesMaxX = "MapCoordinatesMaxX";

            // Output location supplied by border location: Y
            public const string MapCoordinatesMaxY = "MapCoordinatesMaxY";

            // This is used to get the parameter names that the user can supply; the model checks if the input parameters supplied by the user 
            // are known by the model. 
            public static List<string> AllNames
            {
                get
                {
                    return typeof(ParameterNames).GetFields().Select(x => x.Name).ToList();
                }
            }
        }
        
        public static Location GetLandisLocation(float MapCoordinatesX, float MapCoordinatesY, float MapCoordinatesMaxX, float MapCoordinatesMaxY)
        {
            int ColumnCount = PlugIn.ModelCore.Landscape.Columns;
            float relativecolumnposition = MapCoordinatesX / PlugIn.ModelCore.CellLength - MapCoordinatesMaxX / PlugIn.ModelCore.CellLength;
            int newColumn = (int)(ColumnCount + relativecolumnposition) + 1;

            int RowCount = PlugIn.ModelCore.Landscape.Rows;
            int newRow = (int)(System.Math.Abs((MapCoordinatesMaxY - MapCoordinatesY) / PlugIn.ModelCore.CellLength) + 1);

            return new Location(newRow, newColumn);

        }
        public static Location GetLandisLocation(double[] InputMapLocation, double[] MapTopLeftCorner)
        {
            // Map input location on location in LANDIS map
            double mapX = MapTopLeftCorner[0];
            double mapY = MapTopLeftCorner[1];
            double coordX = InputMapLocation[0];
            double coordY = InputMapLocation[1];
            float cellSize = PlugIn.ModelCore.CellLength;

            int newColumn = (int)(System.Math.Abs((mapX - coordX) / cellSize) + 1);
            int newRow = (int)(System.Math.Abs((mapY - coordY) / cellSize) + 1);

            return new Location(newRow, newColumn);
        }
        public static List<Location> GetLandisLocations(List<double[]> Locations, double[] MapTopLeftCorner)
        {
            List<Location> LandisLocations = new List<Location>();
            foreach (double[] L in Locations)
            {
                LandisLocations.Add(GetLandisLocation(L, MapTopLeftCorner));
            }
            return LandisLocations;
        }
        public static void MapCells(Dictionary<string, Parameter<string>> outputfiles, ref Dictionary<ActiveSite, string> OutputSiteNames)
        {
            foreach (KeyValuePair<string, Parameter<string>> site in outputfiles)
            {
                Location OutputLocation;

                // If the location parameters are supplied through MapCoordinatesX/MapCoordinatesY
                if (site.Value.ContainsKey(ParameterNames.MapCoordinatesX) && site.Value.ContainsKey(ParameterNames.MapCoordinatesY))
                {
                    // Get X location
                    float X = float.Parse(site.Value[ParameterNames.MapCoordinatesX]);

                    // Get Y location
                    float Y = float.Parse(site.Value[ParameterNames.MapCoordinatesY]);

                    // Get X border
                    float MaxX = float.Parse(site.Value[ParameterNames.MapCoordinatesMaxX]);

                    // Get Y border
                    float MaxY = float.Parse(site.Value[ParameterNames.MapCoordinatesMaxY]);

                    if (Y >= MaxY) throw new System.Exception("Cannot assign output location, Y coordinate " + Y + " should be larger than MaxY" + MaxY);
                    if (X >= MaxX) throw new System.Exception("Cannot assign output location, X coordinate " + X + " should be larger than MaxY" + MaxX);

                    OutputLocation = GetLandisLocation(X, Y, MaxX, MaxY);

                }
                else
                {
                    OutputLocation = new Location(int.Parse(site.Value[ParameterNames.Row]), int.Parse(site.Value[ParameterNames.Column]));
                }

                // Locate output sites by location
                List<Landis.SpatialModeling.ActiveSite> outputSites =
                    PlugIn.ModelCore.Landscape.Where(localSite => localSite.Location == OutputLocation).ToList();

                if (outputSites.Count() == 0)
                {
                    string msg = "Cannot determine location of " + site.Key;
                    foreach (KeyValuePair<string, string> v in site.Value)
                    {
                        msg += v.Key + " " + v.Value;
                    }
                    msg += " returned " + OutputLocation;

                    throw new System.Exception(msg);
                }
                else
                {
                    OutputSiteNames.Add(outputSites.First(), site.Key);
                }
                 
            }

        }

    }
}
