using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class SpeciesParameterParser
         : TextParser<IInputParameters>
    {

        private Dictionary<string, int> speciesLineNums;
        private InputVar<string> speciesName;
        List<string> headerlabels;

        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }

        public SpeciesParameterParser()
        {
            
            this.speciesLineNums = new Dictionary<string, int>();
            this.speciesName = new InputVar<string>("Species");

        }
        
        private void MakeSureAllHeadersAreThere(string HeaderLine)
        {
            headerlabels = new List<string>(HeaderLine.Trim().Split('\t'));
            foreach (string hdr in headerlabels)
            {
                if (HeaderLine.Contains(hdr) == false) throw new System.Exception("Missing headers in species parameter table:" + hdr);
            }
            

        }
        
        private void MakeSureFileContainsKeyWord(string KeyWord)
        {
            int maxLinesBeforeKeyword = 10000;// 
            int tries = 0;
            for (tries = 0; tries < maxLinesBeforeKeyword; tries++)
            {
                if (ReadOptionalName(KeyWord) == false)
                {
                    return;
                }
            }
            throw new System.Exception("Could not find keyword " + InputParametersParser.Names.PnETSpeciesParameters);
        }
        private void AssureVariableHeaderIsThere(string VariableLabel)
        {
            foreach (string hdr in headerlabels) if (hdr.Trim() == VariableLabel) return;
            throw new System.Exception("Missing variable " + VariableLabel + " in species parameter file");
        }
        private bool TrySet(string ReadLabel, string VariableLabel, InputValue<float> value, int mininput, int maxinput, ISpecies species, Landis.Library.Biomass.Species.AuxParm<int> SpcVar)
        {
            AssureVariableHeaderIsThere(VariableLabel);
             if (System.String.Compare(ReadLabel, VariableLabel, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                SpcVar[species] = (int)Util.CheckBiomassParm(value, mininput, maxinput);
                return true;
            }
            return false;
        }
        private bool TrySet(string ReadLabel, string VariableLabel, InputValue<float> value, float mininput, float maxinput, ISpecies species, Landis.Library.Biomass.Species.AuxParm<float> SpcVar)
        {
            // VariableLabel has to occur in headers
            AssureVariableHeaderIsThere(VariableLabel);
            if (System.String.Compare(ReadLabel, VariableLabel, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                SpcVar[species] = Util.CheckBiomassParm(value, mininput, maxinput);
                return true;
            }
            return false;
        }
        protected override IInputParameters Parse()
        {
            StringReader currentLine;
            InputParameters parameters = new InputParameters();

        
            // to get the option to read species parameters from an external file
            MakeSureFileContainsKeyWord(InputParametersParser.Names.PnETSpeciesParameters);
            
            
            currentLine = new StringReader(CurrentLine);

            string line = currentLine.ReadLine().Trim();

            if (line.Contains("SpeciesName"))
            {
                line = line.Replace("SpeciesName", "");
                MakeSureAllHeadersAreThere(line);
                GetNextLine();
            }
            
            

            speciesLineNums.Clear();  //  If parser re-used (i.e., for testing purposes)

            InputVar<string> SpeciesNameHeader = new InputVar<string>("SpeciesName");
 
            // Make sure that all index numbers are called, if not throw an error
            bool[] AccountedFor = new bool[PlugIn.ModelCore.Species.Count];

            while (!AtEndOfInput && CurrentName != InputParametersParser.Names.EcoregionParameters)
            {
                currentLine = new StringReader(CurrentLine);

                ISpecies species = ReadSpecies(currentLine);
                AccountedFor[species.Index] = true;

                foreach (string label in headerlabels)
                {
                    InputVar<float> var = new InputVar<float>(label);

                    ReadValue(var, currentLine);

                    if (TrySet(label, "FoliageTurnover", var.Value, 0, 1000, species, parameters.FoliageTurnover)) continue;
                    if (TrySet(label, "FolReten", var.Value, 0, 1, species, parameters.FolReten)) continue;
                    if (TrySet(label, "RootTurnover", var.Value, 0, 1, species, parameters.RootTurnover)) continue;
                    if (TrySet(label, "WoodTurnover", var.Value, 0, 1, species, parameters.WoodTurnover)) continue;
                    if (TrySet(label, "GDDFolStart", var.Value, 1, 2000, species, parameters.GDDFolStart)) continue;
                    if (TrySet(label, "GDDFolEnd", var.Value, parameters.GDDFolStart[species], float.MaxValue , species, parameters.GDDFolEnd)) continue;
                    if (TrySet(label, "AmaxA", var.Value, -500, 500, species, parameters.AmaxA)) continue;
                    if (TrySet(label, "AmaxB", var.Value, 0, float.MaxValue, species, parameters.AmaxB)) continue;
                    if (TrySet(label, "HalfSat", var.Value, 0, float.MaxValue, species, parameters.HalfSat)) continue;
                    if (TrySet(label, "BaseFolRespFrac", var.Value, 0, 1000, species, parameters.BaseFolRespFrac)) continue;
                    if (TrySet(label, "GrowthMoistureSensitivity", var.Value, 0, float.MaxValue, species, parameters.GrowthMoistureSensitivity)) continue;
                    if (TrySet(label, "WiltingPoint", var.Value, 0, 1, species, parameters.WiltingPoint)) continue;
                    if (TrySet(label, "PsnAgeRed", var.Value, 0, float.MaxValue, species, parameters.PsnAgeRed)) continue;
                    if (TrySet(label, "RespQ10", var.Value, 0, 10, species, parameters.RespQ10)) continue;
                    if (TrySet(label, "PsnTMin", var.Value, 0, 10, species, parameters.PsnTMin)) continue;
                    if (TrySet(label, "PsnTOpt", var.Value, 0, 40 , species, parameters.PsnTOpt)) continue;
                    if (TrySet(label, "SLWmax", var.Value,  0, 1000 , species, parameters.SLWmax)) continue;
                    if (TrySet(label, "SLWDel", var.Value, 0, 2 , species, parameters.SLWDel)) continue;
                    if (TrySet(label, "SenescStart", var.Value, 0, 365 , species, parameters.SenescStart)) continue;
                    if (TrySet(label, "k", var.Value, 0, 2 , species, parameters.K)) continue;
                    if (TrySet(label, "FolNCon", var.Value, 0, 10, species, parameters.FolNCon)) continue;
                    if (TrySet(label, "DVPD1", var.Value, 0, 5, species, parameters.DVPD1)) continue;
                    if (TrySet(label, "DVPD2", var.Value, 0, 5 , species, parameters.DVPD2)) continue;
                    if (TrySet(label, "WUEConst", var.Value, 0, float.MaxValue , species, parameters.WUEConst)) continue;
                    if (TrySet(label, "MaintResp", var.Value, 0, 1 , species, parameters.MaintResp)) continue;
                    if (TrySet(label, "DNSC", var.Value, 0, 1 , species, parameters.DNSC)) continue;
                    if (TrySet(label, "RootStemRatio", var.Value, 0, 1 , species, parameters.RootStemRatio)) continue;
                     
                    throw new System.Exception("Undetermined parameter label name " + label);
                    
                    
                }
                GetNextLine();
            }
           
            return parameters;
            
        }
        /// <summary>
        /// Reads a species name from the current line, and verifies the name.
        /// </summary>
        private ISpecies ReadSpecies(StringReader currentLine)
        {
            ReadValue(speciesName, currentLine);
            ISpecies species = PlugIn.ModelCore.Species[speciesName.Value.Actual];
            if (species == null)
                throw new InputValueException(speciesName.Value.String,
                                              "{0} is not a species name.",
                                              speciesName.Value.String);
            int lineNumber;
            if (speciesLineNums.TryGetValue(species.Name, out lineNumber))
                throw new InputValueException(speciesName.Value.String,
                                              "The species {0} was previously used on line {1}",
                                              speciesName.Value.String, lineNumber);
            else
                speciesLineNums[species.Name] = LineNumber;
            return species;
        }

    }
}
