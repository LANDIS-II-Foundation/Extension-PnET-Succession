//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.Utilities;
using System.Collections.Generic;

namespace Landis.Extension.Output.Density
{
    /// <summary>
    /// Methods for working with the template for filenames of dead biomass
    /// maps.
    /// </summary>
    public static class DensityMapNames
    {
        public const string DensVar = "variable";
        public const string TimestepVar = "timestep";

        private static IDictionary<string, bool> knownVars;
        private static IDictionary<string, string> varValues;

        //---------------------------------------------------------------------

        static DensityMapNames()
        {
            knownVars = new Dictionary<string, bool>();
            knownVars[DensVar] = true;
            knownVars[TimestepVar] = true;

            varValues = new Dictionary<string, string>();
        }

        //---------------------------------------------------------------------

        public static void CheckTemplateVars(string            template)
                                             //string selectedPools)
        {
            OutputPath.CheckTemplateVars(template, knownVars);
        }

        //---------------------------------------------------------------------

        public static string ReplaceTemplateVars(string template,
                                                 string variable,
                                                 int    timestep)
        {
            varValues[DensVar] = variable;
            varValues[TimestepVar] = timestep.ToString();
            return OutputPath.ReplaceTemplateVars(template, varValues);
        }
    }
}
