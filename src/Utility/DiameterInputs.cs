//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;
using System.IO;

namespace Landis.Extension.Succession.BiomassPnET
{

    public class DiameterInputs
    {
        private static Dictionary<int, IDiameterInputRecord> allData;
        private static IDiameterInputRecord timestepData;

        public DiameterInputs()
        {
        }

        public static Dictionary<int, IDiameterInputRecord> AllData
        {
            get {
                return allData;
            }
        }
        //---------------------------------------------------------------------
        public static IDiameterInputRecord TimestepData
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
            foreach(ISpecies species in PlugIn.ModelCore.Species)
            {
                foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
                {
                    if (!ecoregion.Active)
                        continue;

                    PlugIn.ModelCore.UI.WriteLine("Spp={0}, Eco={1}, Pest={2:0.0}.", species.Name, ecoregion.Name,
                        timestepData.Diameters);

                }
            }

        }
        //---------------------------------------------------------------------
        public static void Initialize(string filename, bool writeOutput)
        {
            PlugIn.ModelCore.UI.WriteLine("   Loading diameter input data from file \"{0}\" ...", filename);
            DiameterInputsParser parser = new DiameterInputsParser();
            try
            {
                allData = Landis.Data.Load<Dictionary<int, IDiameterInputRecord>>(filename, parser);
            }
            catch (FileNotFoundException)
            {
                string mesg = string.Format("Error: The file {0} does not exist", filename);
                throw new System.ApplicationException(mesg);
            }

            timestepData = allData[0];
        }
    }

}
