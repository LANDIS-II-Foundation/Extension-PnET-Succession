using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.InitialCommunities;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Linq;
using System.Reflection;

namespace Landis.Extension.Succession.BiomassPnET
{
    public static class Ecoregion   
    {
        
         
        private static Landis.Library.Parameters.Ecoregions.AuxParm<float> leakageFrac;
        private static Landis.Library.Parameters.Ecoregions.AuxParm<float> precLossFrac;
        private static Landis.Library.Parameters.Ecoregions.AuxParm<float> rootingDepth;
        private static Landis.Library.Parameters.Ecoregions.AuxParm<float> precIntConst;
        private static Landis.Library.Parameters.Ecoregions.AuxParm<string> soilType;
        private static Landis.Library.Parameters.Ecoregions.AuxParm<string> climateFileName;

        public static float RootingDepth(this IEcoregion ecoregion)
        {
            return rootingDepth[ecoregion];
        }
        public static float PrecLossFrac(this IEcoregion ecoregion)
        {
            return precLossFrac[ecoregion];
        }
        public static float LeakageFrac(this IEcoregion ecoregion)
        {
            return leakageFrac[ecoregion];
        }
        public static float PrecIntConst(this IEcoregion ecoregion)
        {
            return precIntConst[ecoregion];
        }
       
        public static List<string> ParameterNames
        {
            get
            {
                return typeof(Ecoregion).GetFields(BindingFlags.Static | BindingFlags.NonPublic).Select(x => x.Name).ToList();
            }
        }

        public static void Initialize()
        {
             leakageFrac = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("LeakageFrac", 0, float.MaxValue);
            precLossFrac = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PrecLossFrac", 0, float.MaxValue);
            rootingDepth = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("RootingDepth", 0, float.MaxValue);
            precIntConst = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PrecIntConst", 0, float.MaxValue);
            soilType = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)PlugIn.GetParameter("SoilType");
            climateFileName = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)PlugIn.GetParameter("ClimateFileName");

        }
    } 
}
