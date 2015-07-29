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
        string[] headerlabels;

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

        private string[] CheckHeaderLabels(string HeaderLine, string[] Headers)
        {
            foreach (string hdr in Headers)
            {
                if (HeaderLine.IndexOf(hdr, System.StringComparison.OrdinalIgnoreCase) < 0) throw new System.Exception("Missing headers in species parameter table:" + hdr);
            }
            return HeaderLine.Trim().Split((char[]) null,System.StringSplitOptions.RemoveEmptyEntries);
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
         
        private bool TrySet(string ReadLabel, string VariableLabel, InputValue<float> value, int mininput, int maxinput, ISpecies species, Landis.Library.Biomass.Species.AuxParm<int> SpcVar)
        {
            if (System.String.Compare(ReadLabel, VariableLabel, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                SpcVar[species] = (int)Landis.Library.Biomass.Util.CheckBiomassParm(ReadLabel,value, mininput, maxinput);
                return true;
            }
            return false;
        }
        private bool TrySet(string ReadLabel, string VariableLabel, InputValue<float> value, float mininput, float maxinput, ISpecies species, Landis.Library.Biomass.Species.AuxParm<float> SpcVar)
        {
            // VariableLabel has to occur in headers
            if (System.String.Compare(ReadLabel, VariableLabel, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                SpcVar[species] = Landis.Library.Biomass.Util.CheckBiomassParm(ReadLabel,value, mininput, maxinput);
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
                string[] Headers = new string[] { "TOfol", "FolRet", "TOroot", "TOwood", "GDDFolSt", "AmaxA", "AmaxB", "HalfSat", "BFolResp" , "GrMstSens", "WltPnt", "PsnAgeRed",
                                                   "Q10", "PsnTMin", "PsnTOpt", "SLWmax", "SLWDel", "CDDFolEnd", "k", "FolN",
                                                  "DVPD1","DVPD2","WUEcnst","MaintResp", "DNSC", "RtStRatio" , "EstMoist", "EstRad", "KWdLit", "FolLignin"};

                
                headerlabels = CheckHeaderLabels(line, Headers);
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

                    if (TrySet(label, "TOfol", var.Value, 0, 1000, species, parameters.TOfol)) continue;
                    if (TrySet(label, "FolRet", var.Value, 0, 1, species, parameters.FolRet)) continue;
                    if (TrySet(label, "TOroot", var.Value, 0, 1, species, parameters.TOroot)) continue;
                    if (TrySet(label, "TOwood", var.Value, 0, 1, species, parameters.TOwood)) continue;
                    if (TrySet(label, "GDDFolSt", var.Value, 1, 2000, species, parameters.GDDFolSt)) continue;
                    if (TrySet(label, "AmaxA", var.Value, -500, 500, species, parameters.AmaxA)) continue;
                    if (TrySet(label, "AmaxB", var.Value, 0, float.MaxValue, species, parameters.AmaxB)) continue;
                    if (TrySet(label, "HalfSat", var.Value, 0, float.MaxValue, species, parameters.HalfSat)) continue;
                    if (TrySet(label, "BFolResp", var.Value, 0, 1000, species, parameters.BFolResp)) continue;
                    if (TrySet(label, "GrMstSens", var.Value, 0.1F, float.MaxValue, species, parameters.GrMstSens)) continue;
                    if (TrySet(label, "WltPnt", var.Value, 0, 1, species, parameters.WltPnt)) continue;
                    if (TrySet(label, "PsnAgeRed", var.Value, 1.0F/float.MaxValue, float.MaxValue, species, parameters.PsnAgeRed)) continue;
                    if (TrySet(label, "Q10", var.Value, 0, 10, species, parameters.Q10)) continue;
                    if (TrySet(label, "PsnTMin", var.Value, -10, 10, species, parameters.PsnTMin)) continue;
                    if (TrySet(label, "PsnTOpt", var.Value, 0, 40 , species, parameters.PsnTOpt)) continue;
                    if (TrySet(label, "SLWmax", var.Value,  0, 1000 , species, parameters.SLWmax)) continue;
                    if (TrySet(label, "SLWDel", var.Value, 0, 2 , species, parameters.SLWDel)) continue;
                    if (TrySet(label, "CDDFolEnd", var.Value, 0, 5000, species, parameters.CDDFolEnd)) continue;
                    if (TrySet(label, "k", var.Value, 0, 2 , species, parameters.K)) continue;
                    if (TrySet(label, "FolN", var.Value, 0, 10, species, parameters.FolN)) continue;
                    if (TrySet(label, "DVPD1", var.Value, 0, 5, species, parameters.DVPD1)) continue;
                    if (TrySet(label, "DVPD2", var.Value, 0, 5 , species, parameters.DVPD2)) continue;
                    if (TrySet(label, "WUEcnst", var.Value, 0, float.MaxValue , species, parameters.WUEcnst)) continue;
                    if (TrySet(label, "MaintResp", var.Value, 0, 1 , species, parameters.MaintResp)) continue;
                    if (TrySet(label, "DNSC", var.Value, 0, 10 , species, parameters.DNSC)) continue;
                    if (TrySet(label, "RtStRatio", var.Value, 0, 1 , species, parameters.RtStRatio)) continue;
                    if (TrySet(label, "EstMoist", var.Value, 0, float.MaxValue , species, parameters.EstMoist)) continue;
                    if (TrySet(label, "EstRad", var.Value, 0, float.MaxValue, species, parameters.EstRad)) continue;
                    if (TrySet(label, "KWdLit", var.Value, 0, float.MaxValue, species, parameters.KWdLit)) continue;

                    if (label == "KNWdLit") throw new System.Exception("KNWdLit is depreciated per 6/24/2014, foliage decomposition rate is calculated from leaf lignin (FolLignin) and site specific AET");

                    if (TrySet(label, "FolLignin", var.Value, 0, float.MaxValue, species, parameters.FolLignin)) continue;
                   
                    throw new System.Exception("Undetermined parameter label name " + label);
                    
                    
                }
                GetNextLine();
            }

            foreach (ISpecies spc in PlugIn.modelCore.Species)
            {
                if (AccountedFor[spc.Index] == false)
                {
                    throw new System.Exception(spc.Name +" is not accounted for in PnET-Species parameter file");

                }
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
