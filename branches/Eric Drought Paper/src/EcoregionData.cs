//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class EcoregionData
    {

        //user-defined by ecoregion
        public static Landis.Extension.Succession.Biomass.Ecoregions.AuxParm<int> AET;
        public static Landis.Extension.Succession.Biomass.Ecoregions.AuxParm<int> ActiveSiteCount;
        public static Landis.Extension.Succession.Biomass.Ecoregions.AuxParm<float> WHC;
        public static Landis.Extension.Succession.Biomass.Ecoregions.AuxParm<float> EvaporationFraction; 
        public static Landis.Extension.Succession.Biomass.Ecoregions.AuxParm<float> LeakageFraction; 
        
        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            AET = parameters.AET;
            WHC = parameters.WHC;
            EvaporationFraction = parameters.EvaporationFraction;
            LeakageFraction = parameters.LeakageFraction;
            ActiveSiteCount     = new Landis.Extension.Succession.Biomass.Ecoregions.AuxParm<int>(PlugIn.ModelCore.Ecoregions);
            
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
                ActiveSiteCount[ecoregion]++;
            }
        }
        


    }
}
