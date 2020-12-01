using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{

    public static class Names
    {
        public const string ExtensionName = "PnET-Succession";
        public const string PNEToutputsites = "PNEToutputsites";
        public const string EcoregionParameters = "EcoregionParameters";
        public const string DisturbanceReductions = "DisturbanceReductions";
        public const string PnETGenericParameters = "PnETGenericParameters";
        public const string PnETGenericDefaultParameters = "PnETGenericDefaultParameters";
        public const string VanGenuchten = "VanGenuchten";
        public const string SaxtonAndRawls = "SaxtonAndRawlsParameters";
        public const string PnETSpeciesParameters = "PnETSpeciesParameters";
        public const string StartYear = "StartYear";
        public const string Timestep = "Timestep";
        public const string SeedingAlgorithm = "SeedingAlgorithm";
        public const string MaxDevLyrAv = "MaxDevLyrAv";
        public const string MaxCanopyLayers = "MaxCanopyLayers";
        public const string IMAX = "IMAX";
        public const string InitialCommunities = "InitialCommunities";
        public const string InitialCommunitiesMap = "InitialCommunitiesMap";
        public const string LitterMap = "LitterMap";
        public const string WoodyDebrisMap = "WoodyDebrisMap";
        public const string ClimateConfigFile = "ClimateConfigFile";
        public const string MapCoordinates = "MapCoordinates";
        public const string PNEToutputSiteCoordinates = "PNEToutputSiteCoordinates";
        public const string PNEToutputSiteLocation = "PNEToutputSiteLocation";
        public const string PressureHeadCalculationMethod = "PressureHeadCalculationMethod";
        public const string Wythers = "Wythers";
        public const string DTemp = "DTemp";
        public const string CO2AMaxBEff = "CO2AMaxBEff";       
        public const string Permafrost = "Permafrost";
        public const string LeakageFrostDepth = "LeakageFrostDepth";
        public const string CohortBinSize = "CohortBinSize";
        public const string InvertPest = "InvertPest";
        //public const string PermafrostMinVegBiomass = "PermafrostMinVegBiomass";
        

        //Ecoregion parameters
        public const string LeakageFrac = "LeakageFrac";
        public const string RunoffCapture = "RunoffCapture";
        public const string PrecLossFrac = "PrecLossFrac";
        public const string RootingDepth = "RootingDepth";
        public const string SoilType = "SoilType";
        public const string PrecIntConst = "PrecIntConst";
        public const string SnowSublimFrac = "SnowSublimFrac";
        public const string PrecipEvents = "PrecipEvents";
        public const string Latitude = "Latitude";
        public const string climateFileName = "climateFileName";
        public const string WinterSTD = "WinterSTD";
        public const string MossDepth = "MossDepth";


        //Species parameters
        public const string FolNShape = "FolNShape";
        public const string MaxFolN = "MaxFolN";
        public const string FracFolShape = "FracFolShape";
        public const string MaxFracFol = "MaxFracFol";
        public const string O3Coeff = "O3GrowthSens";
        public const string LeafOnMinT = "LeafOnMinT"; // Optional

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

    }
}
