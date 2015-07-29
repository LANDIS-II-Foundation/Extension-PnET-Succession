using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using Landis.Library.Biomass;

namespace Landis.Extension.Succession.BiomassPnET
{  
    public class Hydrology 
    {
        private IEcoregion ecoregion;
        private static float snowmelt;
        private static float WaterIn;
         
        float snowpack;
        float water; //mm

        public float Water { get { return water; } }

        public float AnnualTranspiration; //mm
        private float transpiration; //mm
        private float infiltration;
        private float runoff;
        private float waterleakage;
        private float preciploss;

        static Landis.Library.Biomass.Ecoregions.AuxParm<int> WHC;
        static Landis.Library.Biomass.Ecoregions.AuxParm<float> PrecipLossFrac;
        static Landis.Library.Biomass.Ecoregions.AuxParm<int> Porosity;
        static Landis.Library.Biomass.Ecoregions.AuxParm<float> LeakageFrac;

        public float RunOff
        {
            get
            {
                return runoff;
            }
        }
        public float SnowPack
        {
            get
            {
                return snowpack;
            }
        }

        public float Transpiration
        {
            get
            {
                return transpiration;
            }
        }
        public float WaterLeakage
        {
            get
            {
                return waterleakage;
            }
        }
        public float PrecipLoss
        {
            get
            {
                return preciploss;
            }
        }
        
        public static void Initialize(IInputParameters parameters)
        {
            WHC = parameters.WHC;
            PrecipLossFrac = parameters.PrecipLossFrac;
            Porosity = parameters.Porosity;
            LeakageFrac = parameters.LeakageFrac;
        }
       
        //---------------------------------------------------------------------
        public Hydrology(IEcoregion ecoregion)
        {
            this.ecoregion = ecoregion;

            snowpack = 0;
            water = WHC[ecoregion];
            infiltration = 0;
            runoff = 0;
            preciploss = 0;
            transpiration = 0;
            AnnualTranspiration = 0;  
        }
        
        public void SubtractTranspiration(DateTime date, float cohort_transpiration)
        {
            water -= cohort_transpiration;

            
            transpiration += cohort_transpiration;
            AnnualTranspiration += cohort_transpiration;
        }
        public void UpdateSiteHydrology(DateTime date)
        {

            transpiration = 0;

            snowmelt = Math.Min(snowpack, StaticVariables.MaxMonthlySnowMelt[ecoregion, date]);

            snowpack += StaticVariables.NewSnow[ecoregion, date] - snowmelt;

            WaterIn = StaticVariables.PrecIn[ecoregion, date] + snowmelt;//mm  

            preciploss = WaterIn * PrecipLossFrac[ecoregion];

            infiltration = WaterIn - preciploss;

            water += infiltration;

            // instant runoff
            runoff = Math.Max(water - Porosity[ecoregion], 0);

            water -= runoff;

            waterleakage =  Math.Max(0, LeakageFrac[ecoregion] * (water - WHC[ecoregion]));

            water -= waterleakage;

           
        }

        

    }
}
