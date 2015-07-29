using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using Landis.Library.BiomassCohortsPnET;
using Landis.SpatialModeling;
namespace Landis.Extension.Succession.BiomassPnET
{  
    public static class Hydrology 
    {
        private static float newsnow;
        private static float snowmelt;
        private static float WaterIn;
        static float SnowFraction;

        public static ISiteVar<float> SnowPack;
        public static ISiteVar<float> Water; //mm
        
        public static ISiteVar<float> AnnualTranspiration; //mm
        private static ISiteVar<float> transpiration; //mm
        private static ISiteVar<float> infiltration;
        private static ISiteVar<float> runoff;
        private static ISiteVar<float> waterleakage;
        private static ISiteVar<float> evaporation;

        public static Landis.Library.Biomass.Ecoregions.AuxParm<float> WHC;
        public static Landis.Library.Biomass.Ecoregions.AuxParm<float> EvaporationFraction;
        public static Landis.Library.Biomass.Ecoregions.AuxParm<float> LeakageFraction;

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            transpiration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            AnnualTranspiration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            infiltration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            runoff = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            waterleakage = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            evaporation = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            WHC = parameters.WHC;
            EvaporationFraction = parameters.EvaporationFraction;
            LeakageFraction = parameters.LeakageFraction;
            SnowPack = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            Water = PlugIn.ModelCore.Landscape.NewSiteVar<float>();

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                SnowPack[site] = 0;
                Water[site] = 0;
                infiltration[site] = 0;
                runoff[site] = 0;
                evaporation[site] = 0;
                transpiration[site] = 0;
                AnnualTranspiration[site] = 0;
            }
            PlugIn.ModelCore.RegisterSiteVar(AnnualTranspiration, "Succession.AnnualTranspiration");

            PlugIn.ModelCore.RegisterSiteVar(Water, "Succession.SoilWater");
        }
        public static ISiteVar<float> WaterLeakage
        {
            get
            {
                return waterleakage;
            }
        }

        public static ISiteVar<float> Evaporation
        {
            get
            {
                return evaporation;
            }
        }
        public static ISiteVar<float> Infiltration
        {
            get
            {
                return infiltration;
            }
        }
        public static ISiteVar<float> RunOff
        {
            get
            {
                return runoff;
            }
         }

        public static ISiteVar<float> Transpiration
        {
            get
            {
                return transpiration;
            }
        }
        public static void SubstractTranspiration(DateTime date, ActiveSite site, float transpiration)
        {
            Water[site] -= transpiration;
            if (Water[site] < 0) Water[site] = 0;

            Transpiration[site] += transpiration;
            AnnualTranspiration[site] += transpiration;
        }
        public static void Initialize(ActiveSite site)
        {
            Water[site] = WHC[PlugIn.modelCore.Ecoregion[site]];
        }
        public static void UpdateSiteHydrology(DateTime date, ActiveSite site)
        {
            SnowFraction = CumputeSnowFraction(date, site);

            newsnow = SnowFraction * Static.Prec[date];//mm

            snowmelt = Math.Min(SnowPack[site], 0.15f * Math.Max(1, Static.Tave[date]) * Static.DaySpan[date]); //486
            
            SnowPack[site] += newsnow - snowmelt;//mm  

            WaterIn = (1 - SnowFraction) * Static.Prec[date] + snowmelt;//mm  

             
            Evaporation[site] = EvaporationFraction[PlugIn.ModelCore.Ecoregion[site]] * WaterIn;

            Infiltration[site] = WaterIn - Evaporation[site]; 

            Water[site] +=  Infiltration[site];

            RunOff[site] = Math.Max(Water[site] - WHC[PlugIn.ModelCore.Ecoregion[site]], 0);  

            Water[site] -= RunOff[site];

            WaterLeakage[site] = LeakageFraction[PlugIn.ModelCore.Ecoregion[site]] * Water[site]; //1665

            Water[site] -= WaterLeakage[site];

        }

        private static float CumputeSnowFraction(DateTime date, ActiveSite site)
        {
            if (Static.Tave[date] > 2) return 0;
            else if (Static.Tave[date] < -5) return 1;
            else return (Static.Tave[date] - 2) / -7;
        }

    }
}
