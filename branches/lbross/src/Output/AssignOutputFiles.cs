using Landis.Core;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET
{
    public static class AssignOutputFiles
    {
        public struct ParameterNames
        {
            
            public const string Row = "Row";
            public const string Column = "Column";
            public const string MapCoordinatesY = "MapCoordinatesY";
            public const string MapCoordinatesX = "MapCoordinatesX";
            public const string MapCoordinatesMaxX = "MapCoordinatesMaxX";
            public const string MapCoordinatesMaxY = "MapCoordinatesMaxY";

            public static List<string> AllNames
            {
                get
                {
                    return typeof(ParameterNames).GetFields().Select(x => x.Name).ToList();
                }
            }
        }
        static void DisplayInLog(Landis.SpatialModeling.ActiveSite site)
        {
            PlugIn.ModelCore.UI.WriteLine("OutputSite\t" + site.Location + " ecoregion " + PlugIn.ModelCore.Ecoregion[site]);

        
        }
        public static void MapCells(Dictionary<string, Parameter<string>> outputfiles, ref Dictionary<ActiveSite, string> OutputSiteNames)
        {
            foreach (KeyValuePair<string, Parameter<string>> site in outputfiles)
            {
                Location OutputLocation;
                Parameter<string> p = site.Value;
                if (p.ContainsKey(ParameterNames.MapCoordinatesX) && p.ContainsKey(ParameterNames.MapCoordinatesY))
                {
                    float X = float.Parse(site.Value[ParameterNames.MapCoordinatesX]);
                    float Y = float.Parse(site.Value[ParameterNames.MapCoordinatesY]);
                    float MaxX = float.Parse(site.Value[ParameterNames.MapCoordinatesMaxX]);
                    float MaxY = float.Parse(site.Value[ParameterNames.MapCoordinatesMaxY]);

                    if (Y >= MaxY) throw new System.Exception("Cannot assign output location, Y coordinate " + Y + " should be larger than MaxY" + MaxY);
                    if (X >= MaxX) throw new System.Exception("Cannot assign output location, X coordinate " + X + " should be larger than MaxY" + MaxX);

                    OutputLocation = GetLocations.GetLandisLocation(X, Y, MaxX, MaxY);

                }
                else
                {
                    OutputLocation = new Location(int.Parse(p[ParameterNames.Row]), int.Parse(p[ParameterNames.Column]));
                }

                bool foundsite = false;
                foreach (Landis.SpatialModeling.ActiveSite lsite in PlugIn.ModelCore.Landscape)
                {
                    if (lsite.Location == OutputLocation)
                    {
                        OutputSiteNames.Add(lsite, site.Key);

                        DisplayInLog(lsite);

                        foundsite = true;
                    }
                }
                if (foundsite == false)
                {
                    string msg = "Cannot determine location of " + site.Key;
                    foreach (KeyValuePair<string, string> v in p)
                    {
                        msg += v.Key + " " + v.Value;
                    }
                    msg += " returned " + OutputLocation;

                    throw new System.Exception(msg);
                }
            }

        }

    }
}
