//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;
using Landis.Utilities;
using System.Text;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// A parser that reads the tool parameters from text input.
    /// </summary>
    public class DiameterInputsParser
        : TextParser<Dictionary<string,Dictionary<string, IDiameterInputRecord>>>
    {

        public override string LandisDataValue
        {
            get
            {
                return "EcoregionDiameterTable";
            }
        }


        //---------------------------------------------------------------------
        public DiameterInputsParser()
        {
        }

        //---------------------------------------------------------------------

        protected override Dictionary<string,Dictionary<string, IDiameterInputRecord>> Parse()
        {

            //InputVar<string> landisData = new InputVar<string>("LandisData");
            //ReadVar(landisData);
            //if (landisData.Value.Actual != FileName)
            //    throw new InputValueException(landisData.Value.String, "The value is not \"{0}\"", PlugIn.ExtensionName);
            ReadLandisDataVar();

            
            Dictionary<string,Dictionary<string, IDiameterInputRecord>> allData = new Dictionary<string,Dictionary<string, IDiameterInputRecord>>();

            //---------------------------------------------------------------------
            //Read in establishment probability data:
            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion Name");
            InputVar<string> speciesName = new InputVar<string>("Species Name");
            InputVar<int> age = new InputVar<int>("Age to apply diameter");
            InputVar<double> diameter = new InputVar<double>("Diameter");
            //InputVar<Dictionary<int,double>> diameterTable = new InputVar<Dictionary<int,double>>("Diameters by age");


            while (! AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);
                ReadValue(ecoregionName, currentLine);
                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value);
                ReadValue(speciesName, currentLine);
                ISpecies species = GetSpecies(speciesName.Value);
                ReadValue(age, currentLine);
                ReadValue(diameter, currentLine);
                CheckNoDataAfter("the " + diameter.Name + " column",
                 currentLine);

                if (!allData.ContainsKey(ecoregion.Name))
                {
                    Dictionary<string, IDiameterInputRecord> speciesTable = new Dictionary<string, IDiameterInputRecord>();
                    IDiameterInputRecord diameterInputRecord = new DiameterInputRecord();
                    diameterInputRecord.Diameters.Add(age.Value, diameter.Value);
                    speciesTable.Add(species.Name, diameterInputRecord);
                    allData.Add(ecoregion.Name, speciesTable);
                    EcoregionData.ModelCore.UI.WriteLine("  Diameter Input Parser:  Add new ecoregion = {0}.", ecoregion.Name);
                    EcoregionData.ModelCore.UI.WriteLine("  Diameter Input Parser:  Add new species = {0}.", species.Name);
                }
                else
                {
                    if(allData[ecoregion.Name].ContainsKey(species.Name))
                    {
                        // Add age & diameter to existing species table
                       IDiameterInputRecord diameterInputRecord = allData[ecoregion.Name][species.Name];
                       diameterInputRecord.Diameters.Add(age.Value, diameter.Value);
                       allData[ecoregion.Name][species.Name] = diameterInputRecord;
                    }
                    else
                    {
                        //Create new species table and add to dictionary
                        Dictionary<string, IDiameterInputRecord> speciesTable = new Dictionary<string, IDiameterInputRecord>();
                        IDiameterInputRecord diameterInputRecord = new DiameterInputRecord();
                        diameterInputRecord.Diameters.Add(age.Value, diameter.Value);
                        allData[ecoregion.Name].Add(species.Name, diameterInputRecord);
                        EcoregionData.ModelCore.UI.WriteLine("  Diameter Input Parser:  Add new species = {0}.", species.Name);
                    }
                }
                GetNextLine();

            }

            return allData;
        }

        //---------------------------------------------------------------------

        private IEcoregion GetEcoregion(InputValue<string>      ecoregionName)
        {
            IEcoregion ecoregion = EcoregionData.ModelCore.Ecoregions[ecoregionName.Actual];
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String,
                                              "{0} is not an ecoregion name.",
                                              ecoregionName.String);
     

            return ecoregion;
        }

        //---------------------------------------------------------------------

        private ISpecies GetSpecies(InputValue<string> speciesName)
        {
            ISpecies species = EcoregionData.ModelCore.Species[speciesName.Actual];
            if (species == null)
                throw new InputValueException(speciesName.String,
                                              "{0} is not a recognized species name.",
                                              speciesName.String);

            return species;
        }


    }
}
