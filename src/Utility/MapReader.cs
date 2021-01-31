//  Copyright 2007-2016 Portland State University
//  Author: Austen Ruzicka (and Robert Scheller)

using Landis.Core;
using Landis.Library.PnETCohorts;
using Landis.SpatialModeling;
using Landis.Utilities;
using System.IO;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Methods to read maps in lieu of spin-up
    /// </summary>
    public static class MapReader
    {
        static private double maxLitter = 5176.124;
        static private double maxWoodyDebris = 95007.64;
        static private double minLitter = 0;
        static private double minWoodyDebris = 0;

        public static void ReadWoodyDebrisFromMap(string path)
        {
            IInputRaster<DoublePixel> map = MakeDoubleMap(path);

            using (map)
            {
                DoublePixel pixel = map.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    int mapValue = (int)pixel.MapCode.Value;
                    if (site.IsActive)
                    {
                        if (mapValue < minWoodyDebris || mapValue > maxWoodyDebris)
                            throw new InputValueException(mapValue.ToString(),
                                                          "Down dead value {0} is not between {1:0.0} and {2:0.0}. Site_Row={3:0}, Site_Column={4:0}",
                                                          mapValue, minWoodyDebris, maxWoodyDebris, site.Location.Row, site.Location.Column);
                        SiteVars.WoodyDebris[site].InitialMass = mapValue;
                        SiteVars.WoodyDebris[site].Mass = mapValue;
                    }
                }
            }
        }

        public static void ReadLitterFromMap(string path)
        {
            IInputRaster<DoublePixel> map = MakeDoubleMap(path);

            using (map)
            {
                DoublePixel pixel = map.BufferPixel;
                foreach (Site site in PlugIn.ModelCore.Landscape.AllSites)
                {
                    map.ReadBufferPixel();
                    int mapValue = (int)pixel.MapCode.Value;
                    if (site.IsActive)
                    {
                        if (mapValue < minLitter || mapValue > maxLitter)
                            throw new InputValueException(mapValue.ToString(),
                                                          "Litter value {0} is not between {1:0.0} and {2:0.0}. Site_Row={3:0}, Site_Column={4:0}",
                                                          mapValue, minLitter, maxLitter, site.Location.Row, site.Location.Column);

                        SiteVars.Litter[site].InitialMass = mapValue;
                        SiteVars.Litter[site].Mass = mapValue;
                    }
                }
            }
        }

        private static IInputRaster<DoublePixel> MakeDoubleMap(string path)
        {
            PlugIn.ModelCore.UI.WriteLine("  Read in data from {0}", path);

            IInputRaster<DoublePixel> map;

            try
            {
                map = PlugIn.ModelCore.OpenRaster<DoublePixel>(path);
            }
            catch (FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", path);
                throw new System.ApplicationException(mesg);
            }

            if (map.Dimensions != PlugIn.ModelCore.Landscape.Dimensions)
            {
                string mesg = string.Format("Error: The input map {0} does not have the same dimension (row, column) as the scenario ecoregions map", path);
                throw new System.ApplicationException(mesg);
            }

            return map;
        }
    }
}