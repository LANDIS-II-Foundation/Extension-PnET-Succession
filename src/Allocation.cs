using Landis.Core;
using System.Collections.Generic;
using System.Linq;


namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Allocates litters that result from disturbanches. 
    /// Input parameters are fractions of litter that are allocated to different pools
    /// </summary>
    public class Allocation
    {
        // These labels are used as input parameters in the input txt file
        private  static readonly List<string> Disturbances = new List<string>() { "fire", "wind", "bda", "harvest" };

        private static readonly List<string> Reductions = new List<string>() { "WoodReduction", "FolReduction", "RootReduction", "DeadWoodReduction", "LitterReduction" };

        public static void Initialize(string fn,   SortedDictionary<string, Parameter<string>> parameters)
        {
            Dictionary<string, Parameter<string>> DisturbanceReductionParameters = PlugIn.LoadTable(Names.DisturbanceReductions, Reductions, Disturbances);
            foreach (KeyValuePair<string, Parameter<string>> parameter in DisturbanceReductionParameters)
            {
                if (parameters.ContainsKey(parameter.Key)) throw new System.Exception("Parameter " + parameter.Key + " was provided twice");

                foreach (string value in parameter.Value.Values)
                {
                    double v;
                    if (double.TryParse(value, out v) == false) throw new System.Exception("Expecting digit value for " + parameter.Key);

                    if (v > 1 || v < 0) throw new System.Exception("Expecting value for " + parameter.Key + " between 0.0 and 1.0. Found " + v);
                }
            }
            DisturbanceReductionParameters.ToList().ForEach(x => parameters.Add("disturbance:"+x.Key, x.Value));
       
        }

        public static void ReduceDeadPools(object sitecohorts, ExtensionType disturbanceType)
        {


            if (sitecohorts == null)
            {
                throw new System.Exception("sitecohorts should not be null");
            }
            float pdeadwoodlost = 0;
            float plitterlost = 0;
            Parameter<string> parameter;

            if (disturbanceType != null && PlugIn.TryGetParameter(disturbanceType.Name, out parameter))
            {
                // If parameters are available, then set the loss fractions here.
                if (parameter.ContainsKey("DeadWoodReduction"))
                {
                    pdeadwoodlost = float.Parse(parameter["DeadWoodReduction"]);
                }
                if (parameter.ContainsKey("LitterReduction"))
                {
                    plitterlost = float.Parse(parameter["LitterReduction"]);
                }

            }

            ((SiteCohorts)sitecohorts).RemoveWoodyDebris(pdeadwoodlost);
            ((SiteCohorts)sitecohorts).RemoveLitter(plitterlost);


        }
        public static void Allocate(object sitecohorts, Cohort cohort, ExtensionType disturbanceType, double fraction)
        {
            if (sitecohorts == null)
            {
                throw new System.Exception("sitecohorts should not be null");
            }

            ReduceDeadPools(sitecohorts, disturbanceType);

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
            
            // Add new dead wood and litter
            float woodAdded = (float)((1 - pwoodlost) * cohort.Wood * fraction);
            float rootAdded = (float)((1 - prootlost) * cohort.Root * fraction);
            float folAdded = (float)((1 - pfollost) * cohort.Fol * fraction);

            ((SiteCohorts)sitecohorts).AddWoodyDebris(woodAdded, cohort.SpeciesPNET.KWdLit);
            ((SiteCohorts)sitecohorts).AddWoodyDebris(rootAdded, cohort.SpeciesPNET.KWdLit);
            ((SiteCohorts)sitecohorts).AddLitter(folAdded, cohort.SpeciesPNET);

            cohort.AccumulateWoodySenescence((int)(woodAdded + rootAdded));
            cohort.AccumulateFoliageSenescence((int)(folAdded));


        }
    }
}
