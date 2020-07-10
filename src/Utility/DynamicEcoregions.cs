//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;
using System.IO;

namespace Landis.Extension.Succession.BiomassPnET
{

    public class DynamicEcoregions
    {
        private static Dictionary<int, IDynamicEcoregionRecord[]> ecoRegData;
        private static IDynamicEcoregionRecord[] timestepData;
        public static ICore ModelCore;

        public static Landis.Library.Parameters.SpeciesEcoregionAuxParm<double> EstablishProbability;
        public DynamicEcoregions()
        {
        }

        public static Dictionary<int, IDynamicEcoregionRecord[]> EcoRegData
        {
            get {
                return ecoRegData;
            }
        }
        //---------------------------------------------------------------------
        public static IDynamicEcoregionRecord[] TimestepData
        {
            get {
                return timestepData;
            }
            set {
                timestepData = value;
            }
        }

        public static void Write()
        {

            foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (!ecoregion.Active)
                    continue;

                PlugIn.ModelCore.UI.WriteLine("Eco={0}, GSO_1={1:0.0}, GSO_2={2:0.0}, GSO_3={3:0.0}, GSO_4={4:0.0}.", ecoregion.Name,
                    timestepData[ecoregion.Index].GSO1, timestepData[ecoregion.Index].GSO2, timestepData[ecoregion.Index].GSO3,
                    timestepData[ecoregion.Index].GSO4);

            }
            

        }
        //---------------------------------------------------------------------
        public static void Initialize(string filename, bool writeOutput)
        {
            PlugIn.ModelCore.UI.WriteLine("   Loading dynamic input data from file \"{0}\" ...", filename);
            DynamicEcoregionParser parser = new DynamicEcoregionParser();
            try
            {
                ecoRegData = Landis.Data.Load<Dictionary<int, IDynamicEcoregionRecord[]>>(filename, parser);
            }
            catch (FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", filename);
                throw new System.ApplicationException(mesg);
            }

            timestepData = ecoRegData[0];
        }

        public static void ChangeDynamicParameters(int year)
        {

            if (DynamicInputs.AllData.ContainsKey(year))
            {
                EstablishProbability = new Landis.Library.Parameters.SpeciesEcoregionAuxParm<double>(PlugIn.ModelCore.Species, PlugIn.ModelCore.Ecoregions);
                
                DynamicInputs.TimestepData = DynamicInputs.AllData[year];

                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                    {
                        if (!ecoregion.Active)
                            continue;

                        if (DynamicInputs.TimestepData[species.Index, ecoregion.Index] == null)
                            continue;

                        EstablishProbability[species, ecoregion] = DynamicInputs.TimestepData[species.Index, ecoregion.Index].ProbEst;
                    }
                }
            }

        }
    }

}
