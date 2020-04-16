//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;
using Landis.Utilities;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// A parser that reads the tool parameters from text input.
    /// </summary>
    public class DynamicEcoregionParser
        : TextParser<Dictionary<int, IDynamicEcoregionRecord[]>>
    {

        //private string FileName = "Dynamic Input Data";

        public override string LandisDataValue
        {
            get
            {
                return "Dynamic Ecoregion Input Data";
            }
        }


        //---------------------------------------------------------------------
        public DynamicEcoregionParser()
        {
        }

        //---------------------------------------------------------------------

        protected override Dictionary<int, IDynamicEcoregionRecord[]> Parse()
        {
            ReadLandisDataVar();
            
            Dictionary<int, IDynamicEcoregionRecord[]> ecoRegData = new Dictionary<int, IDynamicEcoregionRecord[]>();

            //---------------------------------------------------------------------
            //Read in growing space data:
            InputVar<int>    year       = new InputVar<int>("Time step for updating values");
            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion Name");
            InputVar<double> gs_1 = new InputVar<double>("Growing Space Threshold 1");
            InputVar<double> gs_2 = new InputVar<double>("Growing Space Threshold 2");
            InputVar<double> gs_3 = new InputVar<double>("Growing Space Threshold 3");
            InputVar<double> gs_4 = new InputVar<double>("Growing Space Threshold 4");

            while (! AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(year, currentLine);
                int yr = year.Value.Actual;

                if(!ecoRegData.ContainsKey(yr))
                {
                    IDynamicEcoregionRecord[] inputTable = new IDynamicEcoregionRecord[PlugIn.ModelCore.Ecoregions.Count];
                    ecoRegData.Add(yr, inputTable);
                    PlugIn.ModelCore.UI.WriteLine("  Dynamic Ecoregion Parser:  Add new year = {0}.", yr);
                }

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value);

                IDynamicEcoregionRecord dynamicEcoregionRecord = new DynamicEcoregionRecord();

                ReadValue(gs_1, currentLine);
                dynamicEcoregionRecord.GSO1 = gs_1.Value;

                ReadValue(gs_2, currentLine);
                dynamicEcoregionRecord.GSO2 = gs_2.Value;

                ReadValue(gs_3, currentLine);
                dynamicEcoregionRecord.GSO3 = gs_3.Value;

                ReadValue(gs_4, currentLine);
                dynamicEcoregionRecord.GSO4 = gs_4.Value;

                ecoRegData[yr][ecoregion.Index] = dynamicEcoregionRecord;

                //CheckNoDataAfter("the " + pest.Name + " column", currentLine);

                GetNextLine();

            }

            return ecoRegData;
        }

        //---------------------------------------------------------------------

        private IEcoregion GetEcoregion(InputValue<string>      ecoregionName)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregions[ecoregionName.Actual];
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String,
                                              "{0} is not an ecoregion name.",
                                              ecoregionName.String);
     

            return ecoregion;
        }

        //---------------------------------------------------------------------

        private ISpecies GetSpecies(InputValue<string> speciesName)
        {
            ISpecies species = PlugIn.ModelCore.Species[speciesName.Actual];
            if (species == null)
                throw new InputValueException(speciesName.String,
                                              "{0} is not a recognized species name.",
                                              speciesName.String);

            return species;
        }


    }
}
