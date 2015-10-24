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
         

        }
         
    } 
}
