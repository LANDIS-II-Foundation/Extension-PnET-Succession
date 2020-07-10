using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Library.DensityCohorts
{
    class SpeciesParameters
    {
        public static biomassUtil biomass_util = new biomassUtil();
        public static SpeciesDensity SpeciesDensity;

        //private static SortedDictionary<string, Parameter<string>> parameters = new SortedDictionary<string, Parameter<string>>(StringComparer.InvariantCultureIgnoreCase);

        public static bool TryGetParameter(string label, out Parameter<string> parameter)
        {
            parameter = null;
            if (label == null)
            {
                return false;
            }

            if (Names.parameters.ContainsKey(label) == false) return false;

            else
            {
                parameter = Names.parameters[label];
                return true;
            }
        }

        public static Parameter<string> GetParameter(string label)
        {
            if (Names.parameters.ContainsKey(label) == false)
            {
                throw new System.Exception("No value provided for parameter " + label);
            }

            return Names.parameters[label];

        }
        public static Parameter<string> GetParameter(string label, float min, float max)
        {
            if (Names.parameters.ContainsKey(label) == false)
            {
                throw new System.Exception("No value provided for parameter " + label);
            }

            Parameter<string> p = Names.parameters[label];

            foreach (KeyValuePair<string, string> value in p)
            {
                float f;
                if (float.TryParse(value.Value, out f) == false)
                {
                    throw new System.Exception("Unable to parse value " + value.Value + " for parameter " + label + " unexpected format.");
                }
                if (f > max || f < min)
                {
                    throw new System.Exception("Parameter value " + value.Value + " for parameter " + label + " is out of range. [" + min + "," + max + "]");
                }
            }
            return p;

        }

        public static void LoadParameters(SpeciesDensity modelSpecies)
        {
            SpeciesDensity = modelSpecies;
        }
    }
}
