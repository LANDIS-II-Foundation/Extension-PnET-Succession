using System.Collections.Generic;
using System;

namespace Landis.Library.DensityCohorts
{ 
    public static class Names
    {
        public static SortedDictionary<string, Parameter<string>> parameters = new SortedDictionary<string, Parameter<string>>(StringComparer.InvariantCultureIgnoreCase); 
        
        public const string ExtensionName = "PnET-Succession";
        public const string PNEToutputsites = "PNEToutputsites";
        public const string EcoregionParameters = "EcoregionParameters";
        public const string DisturbanceReductions = "DisturbanceReductions";
        public const string PnETGenericParameters = "PnETGenericParameters";
        public const string DensitySpeciesParameters = "DensitySpeciesParameters";
        public const string StartYear = "StartYear";
        public const string Timestep = "Timestep";
        public const string SeedingAlgorithm = "SeedingAlgorithm";
        public const string InitialCommunities = "InitialCommunities";
        public const string InitialCommunitiesMap = "InitialCommunitiesMap";
        public const string ClimateConfigFile = "ClimateConfigFile";
        public const string MapCoordinates = "MapCoordinates";
        public const string BiomassVariables = "BiomassVariableFile";
        public const string DynamicInputFile = "DynamicInputFile";
        public const string DynamicEcoregionFile = "DynamicEcoregionFile";
        public const string DiameterInputFile = "DiameterInputFile";

        //Ecoregion parameters
        public const string Latitude = "Latitude";
        public const string climateFileName = "climateFileName";

        public static void AssureIsName(string name)
        {
            if (IsName(name) == false)
            {
                string msg = name + " is not a keyword keywords are /n"+ string.Join("\n\t", AllNames.ToArray());
                throw new System.Exception(msg);
            }
        }
        public static bool IsName(string name)
        {
            List<string> Names = AllNames;
            foreach (string _name in AllNames)
            {
                if (System.String.Compare(_name, name, System.StringComparison.OrdinalIgnoreCase) == 0) return true;
            }
            return false;
        }
        public static List<string> AllNames
        {
            get
            {
                List<string> Names = new List<string>();
                foreach (var name in typeof(Names).GetFields())
                {
                    string value = name.GetValue(name).ToString();
                   
                    Names.Add(value);
                    //Console.WriteLine(value);
                }
                return Names;
            }
        }

        public static void LoadParameters(SortedDictionary<string, Parameter<string>> modelParameters)
        {
            parameters = modelParameters;
        }

    }
}
