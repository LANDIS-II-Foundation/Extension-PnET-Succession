//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.Succession;
using Landis.Core;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// A parser that reads biomass succession parameters from text input.
    /// </summary>
    public class InputParametersParser
        : TextParser<IInputParameters>
    {
        public static class Names
        {
            public const string Latitude = "Latitude";
            public const string climateFileName = "climateFileName";
            public const string EcoregionParameters = "EcoregionParameters";
            public const string PnETSpeciesParameters = "PnETSpeciesParameters";
            public const string SpeciesParameterFile = "SpeciesParameterFile";
            public const string EstMoist = "EstMoist";
            public const string EstRad = "EstRad";
            public const string PNEToutputsites = "PNEToutputsites";

            public const string CanopyLayersMax = "CanopyLayersMax";
            public const string StartYear = "StartYear";
            public const string Timestep = "Timestep";
            public const string SeedingAlgorithm = "SeedingAlgorithm";
           
        }
        private string dataFile;
        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }

        //---------------------------------------------------------------------

        private Dictionary<string, int> speciesLineNums;
        


        //---------------------------------------------------------------------

        static InputParametersParser()
        {
            
            SeedingAlgorithmsUtil.RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        public InputParametersParser(string dataFile)
        {
            this.dataFile = dataFile;
            this.speciesLineNums = new Dictionary<string, int>();
           // this.speciesName = new InputVar<string>("Species");

        }

        //---------------------------------------------------------------------
        private bool TrySet(string ReadLabel, string VariableLabel, InputValue<string> value, float mininput, float maxinput, IEcoregion ecoregion, Landis.Library.Biomass.Ecoregions.AuxParm<int> EcoPar)
        {
            double value_in_double;

            if (double.TryParse(value.Actual, out value_in_double) == false) return false;

            InputValue<double> dv = new InputValue<double>(value_in_double, "");
            // VariableLabel has to occur in headers
            if (System.String.Compare(ReadLabel, VariableLabel, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                try
                {
                    EcoPar[ecoregion] = (int)Landis.Library.Biomass.Util.CheckBiomassParm(ReadLabel, dv, mininput, maxinput);
                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Cannot set " + ReadLabel + e.Message); 
                }
                return true;
            }
            return false;
        }
        private bool TrySet(string ReadLabel, string VariableLabel, InputValue<string> value,  IEcoregion ecoregion, Landis.Library.Biomass.Ecoregions.AuxParm<string> EcoPar)
        {
            // VariableLabel has to occur in headers
            if (System.String.Compare(ReadLabel, VariableLabel, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                EcoPar[ecoregion] = value;
                return true;
            }
            return false;
        }
        private bool TrySet(string ReadLabel, string VariableLabel, InputValue<string> value, float mininput, float maxinput, IEcoregion ecoregion, Landis.Library.Biomass.Ecoregions.AuxParm<float> EcoPar)
        {
            double value_in_double;

            if (double.TryParse(value.Actual, out value_in_double) == false) return false;

            InputValue<double> dv = new InputValue<double>(value_in_double, "");
            if (System.String.Compare(ReadLabel, VariableLabel, System.StringComparison.OrdinalIgnoreCase) == 0)
            {
                EcoPar[ecoregion] = (float)Landis.Library.Biomass.Util.CheckBiomassParm(ReadLabel,dv, mininput, maxinput);
                return true;
            }
            return false;
        }
        private void AssureAllHeadersAreProvided(string[] ReadHeaders, string[] ExpectedHeaders)
        { 
            foreach(string s in ExpectedHeaders)
            {
                bool Found = false;
                foreach (string r in ReadHeaders)
                {
                    if (System.String.Compare(r, s, System.StringComparison.OrdinalIgnoreCase) == 0) Found = true;
                }
                if (!Found) throw new System.Exception("Expected ecoregion header/variable " + s + " is not provided in " + dataFile);
            }
        }
        protected override IInputParameters Parse()
        {
            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != PlugIn.ExtensionName)
                throw new InputValueException(landisData.Value.String, "The value "+landisData.Value.Actual +" not \"{0}\"" + dataFile, PlugIn.ExtensionName);

            InputParameters parameters = new InputParameters();

            InputVar<int> timestep = new InputVar<int>(Names.Timestep);
            ReadVar(timestep);
            parameters.Timestep = timestep.Value;

            InputVar<int> StartYear = new InputVar<int>(Names.StartYear);
            ReadVar(StartYear);
            parameters.StartYear = StartYear.Value;
            
            InputVar<SeedingAlgorithms> seedAlg = new InputVar<SeedingAlgorithms>(Names.SeedingAlgorithm);
            ReadVar(seedAlg);
            parameters.SeedAlgorithm = seedAlg.Value;

            //---------------------------------------------------------------------------------

            InputVar<string> initCommunities = new InputVar<string>("InitialCommunities");
            ReadVar(initCommunities);
            parameters.InitialCommunities = initCommunities.Value;

            InputVar<string> communitiesMap = new InputVar<string>("InitialCommunitiesMap");
            ReadVar(communitiesMap);
            parameters.InitialCommunitiesMap = communitiesMap.Value;

            
            InputVar<int> Latitude = new InputVar<int>(Names.Latitude);
            ReadVar(Latitude);
            parameters.Latitude = Landis.Library.Biomass.Util.CheckBiomassParm(Latitude.Value, -90, 90);

            if (ReadOptionalName("CanopyLayerBiomassCategories")) throw new System.Exception("CanopyLayerBiomassCategories are depreciated, use " + Names.CanopyLayersMax);

            if(ReadOptionalName(Names.CanopyLayersMax))while (!AtEndOfInput && CurrentName != Names.PNEToutputsites && CurrentName != Names.SpeciesParameterFile)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                InputVar<string> age = new InputVar<string>("Output age");
                ReadValue(age, currentLine);
                    
                int MyAge;

                if (int.TryParse(age.Value, out MyAge))parameters.CanopyLayerAges.Add(MyAge);
                else if (age.Value == "MAX")
                {
                    throw new System.Exception("MAX is no longer valid for CanopyLayerAges " + dataFile + " the model automatically uses the number of layers associated with the max entered age when age exceeds max age of the top layer.");
                }
                else throw new System.Exception("Cannot add canopy age " + age.Value + " in "+ dataFile +" expecting integer");

                InputVar<string> numbers = new InputVar<string>("Max foliage layers");
                ReadValue(numbers, currentLine);

                int MyNumbers;
                if (int.TryParse(numbers.Value, out MyNumbers))
                {
                    parameters.CanopyLayerNumbers.Add(MyNumbers);
                }
               
                else throw new System.Exception("Expected integers or for CanopyLayerAges in " + dataFile);
                    
                    
                GetNextLine();
            }

            

            if (ReadOptionalName(Names.PNEToutputsites) == true)
            {
                while (!AtEndOfInput && CurrentName != Names.SpeciesParameterFile)
                {
                    StringReader currentLine = new StringReader(CurrentLine);

                    InputVar<int> row = new InputVar<int>("Output row");
                    InputVar<int> col = new InputVar<int>("Output column");

                    ReadValue(row, currentLine);
                    ReadValue(col, currentLine);

                    foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                    {
                        if (site.Location.Row == row.Value && site.Location.Column == col.Value)
                        {
                            parameters.HasSiteOutput[site] = true;
                        }
                    }
                   

                    GetNextLine();
                }
            }
             
            //-------------------------
            //  SpeciesParameters table

            InputVar<string> SpeciesParameterFile = new InputVar<string>(Names.SpeciesParameterFile);
            ReadVar(SpeciesParameterFile);
            parameters = ReadSpeciesParameters(SpeciesParameterFile.Value, parameters);

            
            ReadName(Names.EcoregionParameters);


            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            InputVar<int> aet = new InputVar<int>("AET");
            InputVar<float> whc = new InputVar<float>("WHC");
            InputVar<float> PrecipLossFrac = new InputVar<float>("PrecipLossFrac");
            InputVar<float> LeakageFraction = new InputVar<float>("LeakageFraction");
            InputVar<float> Porosity = new InputVar<float>("Porosity");
            InputVar<string> climateFileName = new InputVar<string>(Names.climateFileName);

            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();

            string[] ReadHeaders = CurrentLine.Split(new char[] { ',', '\t', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (ReadHeaders[0] != "Ecoregion") throw new System.Exception("The first column header in ecoregion parameters in "+dataFile+" should be 'Ecoregion'. Note that the headerline should NOT be outcommented as in Biomass Succession");

            string[] ExpectedHeaders = new string[] { "Ecoregion", "AET", "WHC", "PrecLossFrac", "LeakageFraction", "Porosity", Names.climateFileName };


            AssureAllHeadersAreProvided(ReadHeaders, ExpectedHeaders);

            GetNextLine();

            while (!AtEndOfInput)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value, lineNumbers);

                foreach (string label in ReadHeaders)
                {
                    if (label == "Ecoregion") continue;

                    InputVar<string> var = new InputVar<string>(label);

                    ReadValue(var, currentLine);

                    if (TrySet(label, "AET", var.Value, 0, 1000, ecoregion, parameters.AET)) continue;
                    else if (TrySet(label, "WHC", var.Value, 0, 1000, ecoregion, parameters.WHC)) continue;
                    else if (TrySet(label, "PrecLossFrac", var.Value, 0, 1000, ecoregion, parameters.PrecipLossFrac)) continue;
                    else if (TrySet(label, "LeakageFraction", var.Value, 0, 1000, ecoregion, parameters.LeakageFrac)) continue;
                    else if (TrySet(label, "Porosity", var.Value, 0, 1000, ecoregion, parameters.Porosity)) continue;
                    else if (TrySet(label, Names.climateFileName, var.Value,ecoregion, parameters.climateFileName)) continue;
                    else throw new System.Exception("Cannot assign parameter "+ label);
                }
                GetNextLine();        
            }


            foreach (IEcoregion ecoregion in PlugIn.modelCore.Ecoregions)
            {
                if (parameters.Porosity[ecoregion] < parameters.WHC[ecoregion]) throw new System.Exception("Porosity for " + ecoregion + " cannot exceed water holding capacity in " + dataFile);
            }

            return parameters;
        }
         
        InputParameters ReadSpeciesParameters(string FileName, InputParameters parameters)
        {
            // to get the option to read species parameters from an external file
            SpeciesParameterParser parser = new SpeciesParameterParser();

            IInputParameters speciesparameters = (IInputParameters)Landis.Data.Load<IInputParameters>(FileName, parser);

            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                parameters.TOroot[species] = speciesparameters.TOroot[species];
                parameters.TOwood[species] = speciesparameters.TOwood[species];
                parameters.TOfol[species] = speciesparameters.TOfol[species];
                parameters.FolRet[species] = speciesparameters.FolRet[species];
                parameters.AmaxA[species] = speciesparameters.AmaxA[species];
                parameters.AmaxB[species] = speciesparameters.AmaxB[species];
                parameters.BFolResp[species] = speciesparameters.BFolResp[species];
                parameters.GrMstSens[species] = speciesparameters.GrMstSens[species];
                parameters.WltPnt[species] = speciesparameters.WltPnt[species];
                parameters.DVPD1[species] = speciesparameters.DVPD1[species];
                parameters.DVPD2[species] = speciesparameters.DVPD2[species];
                parameters.FolN[species] = speciesparameters.FolN[species];
                parameters.HalfSat[species] = speciesparameters.HalfSat[species];
                parameters.MaintResp[species] = speciesparameters.MaintResp[species];
                parameters.DNSC[species] = speciesparameters.DNSC[species];
                parameters.RtStRatio[species] = speciesparameters.RtStRatio[species];
                parameters.K[species] = speciesparameters.K[species];
                parameters.PsnAgeRed[species] = speciesparameters.PsnAgeRed[species];
                parameters.PsnTMin[species] = speciesparameters.PsnTMin[species];
                parameters.PsnTOpt[species] = speciesparameters.PsnTOpt[species];
                parameters.Q10[species] = speciesparameters.Q10[species];
                parameters.CDDFolEnd[species] = speciesparameters.CDDFolEnd[species];
                parameters.SLWmax[species] = speciesparameters.SLWmax[species];
                parameters.SLWDel[species] = speciesparameters.SLWDel[species];
                parameters.WUEcnst[species] = speciesparameters.WUEcnst[species];
                parameters.GDDFolSt[species] = speciesparameters.GDDFolSt[species];
                parameters.EstMoist[species] = speciesparameters.EstMoist[species];
                parameters.EstRad[species] = speciesparameters.EstRad[species];
                parameters.KWdLit[species] = speciesparameters.KWdLit[species];
                parameters.FolLignin[species] = speciesparameters.FolLignin[species];
               
                
              }

            return parameters;
        }
        //---------------------------------------------------------------------

         
        private IEcoregion GetEcoregion(InputValue<string>      ecoregionName,
                                        Dictionary<string, int> lineNumbers)
        {
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregions[ecoregionName.Actual];
            if (ecoregion == null)
                throw new InputValueException(ecoregionName.String,
                                              "{0} is not an ecoregion name.",
                                              ecoregionName.String);
            int lineNumber;
            if (lineNumbers.TryGetValue(ecoregion.Name, out lineNumber))
                throw new InputValueException(ecoregionName.String,
                                              "The ecoregion {0} was previously used on line {1}",
                                              ecoregionName.String, lineNumber);
            else
                lineNumbers[ecoregion.Name] = LineNumber;

            return ecoregion;
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Reads ecoregion names as column headings
        /// </summary>
        private List<IEcoregion> ReadEcoregions()
        {
            if (AtEndOfInput)
                throw NewParseException("Expected a line with the names of 1 or more active ecoregions.");

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion");
            List<IEcoregion> ecoregions = new List<IEcoregion>();
            StringReader currentLine = new StringReader(CurrentLine);
            TextReader.SkipWhitespace(currentLine);
            while (currentLine.Peek() != -1)
            {
                ReadValue(ecoregionName, currentLine);
                IEcoregion ecoregion = PlugIn.ModelCore.Ecoregions[ecoregionName.Value.Actual];
                if (ecoregion == null)
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "{0} is not an ecoregion name.",
                                                  ecoregionName.Value.String);
                if (!ecoregion.Active)
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "{0} is not an active ecoregion",
                                                  ecoregionName.Value.String);
                if (ecoregions.Contains(ecoregion))
                    throw new InputValueException(ecoregionName.Value.String,
                                                  "The ecoregion {0} appears more than once.",
                                                  ecoregionName.Value.String);
                ecoregions.Add(ecoregion);
                TextReader.SkipWhitespace(currentLine);
            }
            GetNextLine();

            if(ecoregions.Count == 0)
                throw new InputValueException("", "No ecoregions read in correctly.","");

            return ecoregions;
        }
    }
}
