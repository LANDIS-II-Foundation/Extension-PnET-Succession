using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Succession;
using Landis.Library.InitialCommunities;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Linq;
using Landis.Library.Parameters.Species;


namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Allocates litters that result from disturbanches. 
    /// Input parameters are fractions of litter that are allocated to different pools
    /// </summary>
    public class Allocation
    {
        // These labels are used as input parameters in the input txt file
        public static List<string> Disturbances = new List<string>() { "disturbance:fire", "disturbance:wind", "disturbance:bda", "disturbance:harvest" };

        public static List<string> Reductions = new List<string>() { "WoodReduction", "FolReduction", "RootReduction"};
 
        public static void Allocate(object sitecohorts, Cohort cohort, ExtensionType disturbanceType)
        {
            if (sitecohorts == null) return;// Deaths in spinup are not added

            // By default, all material is allocated to the woody debris or the litter pool
            float pwoodlost = 0;
            float prootlost = 0;
            float pfollost = 0;

            Parameter<string> parameter;

            if (disturbanceType != null && PlugIn.TryGetParameter(disturbanceType.Name, out parameter))
            {
                // If parameters are available, then set the loss fractions here.
                if (parameter.ContainsKey("WoodReduction"))
                {
                    pwoodlost = float.Parse(parameter["WoodReduction"]);
                }
                if (parameter.ContainsKey("RootReduction"))
                {
                    prootlost = float.Parse(parameter["RootReduction"]);
                }
                if (parameter.ContainsKey("FolReduction"))
                {
                    pfollost = float.Parse(parameter["FolReduction"]);
                }

            }

            ((SiteCohorts)sitecohorts).AddWoodyDebris((float)((1 - pwoodlost) * cohort.Wood), cohort.SpeciesPNET.KWdLit);
            ((SiteCohorts)sitecohorts).AddWoodyDebris((float)((1 - prootlost) * cohort.Root), cohort.SpeciesPNET.KWdLit);
            ((SiteCohorts)sitecohorts).AddLitter((float)((1 - pfollost) * cohort.Fol), cohort.SpeciesPNET);


        }
    }
}
