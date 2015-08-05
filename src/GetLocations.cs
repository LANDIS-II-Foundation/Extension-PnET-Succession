using Landis.SpatialModeling;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.BiomassPnET
{
    public static class GetLocations
    {


        /*
        InputParameters.HasSiteOutput = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
        if (ReadOptionalName(Names.PNEToutputsites) == true)
        {
            List<Location> LandisLocations = null;
            if (CurrentLine.Contains(Names.MapCoordinates))
            {
                StringReader currentLine = new StringReader(CurrentLine);

                List<string> MapCoordinates = new List<string>(new StringReader(CurrentLine).ReadLine().Trim().Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));

                //InputVar<string> MapCoordinates = new InputVar<string>(null);
                //ReadValue(MapCoordinates, currentLine);

                // Row, col of LANDIS MAP -> should be row, col of any input map, i.e. Initial Communities
                //InputVar<double> MaxCoordX = new InputVar<double>(null);
                //InputVar<double> MaxCoordY = new InputVar<double>(null);

                //ReadValue(MaxCoordX, currentLine);
                //ReadValue(MaxCoordY, currentLine);

                double[] MapTopRightCorner = new double[] { double.Parse(MapCoordinates[3]), double.Parse(MapCoordinates[4]) };//MapTopLeftCorner[0] =X, MapTopLeftCorner[1]=y

                GetNextLine();

                // OutputSites specifies X,Y coordinates
                List<double[]> OutputSites = GetInputLocations<double>(Names.PNEToutputSiteCoordinates);

                LandisLocations = GetLocations.GetLandisLocations(OutputSites, MapTopRightCorner);

                
            }
            else 
            {
                List<int[]> Locations = GetInputLocations<int>(Names.PNEToutputSiteLocation);

                LandisLocations = new List<Location>();

                foreach(int[] Location in Locations)
                {
                    // OutputSites specifies row, col
                    LandisLocations.Add(new Location(Location[0], Location[1]));
                }
                    
                    
            }
            parameters = SetOutputSites(LandisLocations,parameters);
        }
         */
         
        public static Location GetLandisLocation(float MapCoordinatesX,  float MapCoordinatesY, float MapCoordinatesMaxX, float MapCoordinatesMaxY)
        {
            float cellSize = PlugIn.ModelCore.CellLength;

            int ColumnCount = PlugIn.ModelCore.Landscape.Columns;
            float relativecolumnposition = MapCoordinatesX / cellSize - MapCoordinatesMaxX / cellSize;
            int newColumn = (int)(ColumnCount + relativecolumnposition) + 1;

            int RowCount = PlugIn.ModelCore.Landscape.Rows;
            int newRow = (int)(System.Math.Abs((MapCoordinatesMaxY - MapCoordinatesY) / cellSize) + 1);

            return new Location(newRow, newColumn);
        
        }
        public static Location GetLandisLocation(double[] InputMapLocation, double[] MapTopLeftCorner)
        {
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
                LandisLocations.Add(GetLocations.GetLandisLocation(L, MapTopLeftCorner));
            }
            return LandisLocations;
        }
    }
}
