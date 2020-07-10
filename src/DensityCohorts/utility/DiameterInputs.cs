//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;
using System.IO;

namespace Landis.Library.DensityCohorts
{

    public class DiameterInputs
    {
        private static Dictionary<string,Dictionary<string, IDiameterInputRecord>> allData;
        private static IDiameterInputRecord timestepData;

        public DiameterInputs()
        {
        }

        public static Dictionary<string,Dictionary<string, IDiameterInputRecord>> AllData
        {
            get {
                return allData;
            }
        }
        //---------------------------------------------------------------------

        public static void Write()
        {
            foreach(ISpecies species in EcoregionData.ModelCore.Species)
            {
                foreach(IEcoregion ecoregion in EcoregionData.ModelCore.Ecoregions)
                {
                    if (!ecoregion.Active)
                        continue;

                    EcoregionData.ModelCore.UI.WriteLine("Spp={0}, Eco={1}, Diameters={2:0.0}.", species.Name, ecoregion.Name,
                        allData[ecoregion.Name][species.Name].Diameters);

                }
            }

        }
        //---------------------------------------------------------------------
        public static void Initialize(string filename, bool writeOutput)
        {
            EcoregionData.ModelCore.UI.WriteLine("   Loading diameter input data from file \"{0}\" ...", filename);
            DiameterInputsParser parser = new DiameterInputsParser();
            try
            {
                allData = Landis.Data.Load<Dictionary<string,Dictionary<string, IDiameterInputRecord>>>(filename, parser);
            }
            catch (FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", filename);
                throw new System.ApplicationException(mesg);
            }

        }
    }

}
