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
            public const string EstMoistureSensitivity = "EstMoistureSensitivity";
            public const string EstRadSensitivity = "EstRadSensitivity";
            public const string PNEToutputsites = "PNEToutputsites";
            
            public const string CanopyLayerBiomassCategories = "CanopyLayerBiomassCategories";
            public const string StartYear = "StartYear";
            public const string Timestep = "Timestep";
            public const string SeedingAlgorithm = "SeedingAlgorithm";
            public const string AgeOnlyDisturbanceParms = "AgeOnlyDisturbances:BiomassParameters";
        }
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
            Percentage dummy = new Percentage();
            SeedingAlgorithmsUtil.RegisterForInputValues();
        }

        //---------------------------------------------------------------------

        public InputParametersParser()
        {
            this.speciesLineNums = new Dictionary<string, int>();
           // this.speciesName = new InputVar<string>("Species");

        }

        //---------------------------------------------------------------------

        protected override IInputParameters Parse()
        {
            InputVar<string> landisData = new InputVar<string>("LandisData");
            ReadVar(landisData);
            if (landisData.Value.Actual != PlugIn.ExtensionName)
                throw new InputValueException(landisData.Value.String, "The value is not \"{0}\"", PlugIn.ExtensionName);

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

            InputVar<string> climateFileName = new InputVar<string>(Names.climateFileName);
            ReadVar(climateFileName);
            parameters.climateFileName = climateFileName.Value;

            InputVar<int> Latitude = new InputVar<int>(Names.Latitude);
            ReadVar(Latitude);
            parameters.Latitude = Util.CheckBiomassParm(Latitude.Value, -90, 90);

            InputVar<string> CanopyLayerBiomassCategories = new InputVar<string>(Names.CanopyLayerBiomassCategories);
            ReadVar(CanopyLayerBiomassCategories);
            CanopyLayerCategories.InitializeCanopyLayers(Names.CanopyLayerBiomassCategories, CanopyLayerBiomassCategories.Value);

            //InputVar<int> CanopyLayerAgeSpan = new InputVar<int>(Names.CanopyLayerAgeSpan);
            //ReadVar(CanopyLayerAgeSpan);
            //parameters.CanopyLayerAgeSpan = int.Parse(Util.CheckBiomassParm(CanopyLayerAgeSpan.Value, 0, int.MaxValue).ToString());
             
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

            InputVar<string> EstRadSensitivity = new InputVar<string>(Names.EstRadSensitivity);
            if (ReadOptionalVar(EstRadSensitivity))
            {
                InitEstablishmentTable(EstRadSensitivity.Value, Names.EstRadSensitivity, parameters.EstRadSensitivity);
            }
            else InitEstablishmentTable(Names.EstRadSensitivity, parameters.EstRadSensitivity);

            InputVar<string> EstMoistureSensitivity = new InputVar<string>(Names.EstMoistureSensitivity);
            if (ReadOptionalVar(EstMoistureSensitivity))
            {
                InitEstablishmentTable(EstMoistureSensitivity.Value, Names.EstMoistureSensitivity, parameters.EstMoistureSensitivity);
            }
            else InitEstablishmentTable(Names.EstMoistureSensitivity, parameters.EstMoistureSensitivity);

            ReadName(Names.EcoregionParameters);

            InputVar<string> ecoregionName = new InputVar<string>("Ecoregion Name");
            InputVar<int> aet = new InputVar<int>("Actual Evapotranspiration");
            InputVar<float> whc = new InputVar<float>("Water holding capacity");
            InputVar<float> evaporationfraction = new InputVar<float>("EvaporationFraction");
            InputVar<float> leakagefraction = new InputVar<float>("LeakageFraction");
             
            Dictionary <string, int> lineNumbers = new Dictionary<string, int>();

            string lastColumn = "the " + aet.Name + " column";

            while (!AtEndOfInput && CurrentName != Names.AgeOnlyDisturbanceParms)
            {
                StringReader currentLine = new StringReader(CurrentLine);

                ReadValue(ecoregionName, currentLine);

                IEcoregion ecoregion = GetEcoregion(ecoregionName.Value,
                                                    lineNumbers);

                ReadValue(aet, currentLine);
                parameters.AET[ecoregion] = Util.CheckBiomassParm(aet.Value, 0, int.MaxValue);  

                ReadValue(whc, currentLine);
                parameters.WHC[ecoregion] = Util.CheckBiomassParm(whc.Value, 0, int.MaxValue);

                ReadValue(evaporationfraction, currentLine);
                parameters.EvaporationFraction[ecoregion] = Util.CheckBiomassParm(evaporationfraction.Value, 0, 1);

                ReadValue(leakagefraction, currentLine);
                parameters.LeakageFraction[ecoregion] = Util.CheckBiomassParm(leakagefraction.Value, 0, 1);   
                	

                CheckNoDataAfter(lastColumn, currentLine);
                GetNextLine();
            }


            string lastParameter = null;
            if (! AtEndOfInput && CurrentName == Names.AgeOnlyDisturbanceParms) {
                InputVar<string> ageOnlyDisturbanceParms = new InputVar<string>(Names.AgeOnlyDisturbanceParms);
                ReadVar(ageOnlyDisturbanceParms);
                parameters.AgeOnlyDisturbanceParms = ageOnlyDisturbanceParms.Value;

                lastParameter = "the " + Names.AgeOnlyDisturbanceParms + " parameter";
            }

            if (lastParameter != null)
                CheckNoDataAfter(lastParameter);

            return parameters;
        }
        static void InitEstablishmentTable(string KeyWord, Landis.Library.Biomass.Species.AuxParm<float[]> SpeciesTables)
        {
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                SpeciesTables[species] = EstablishmentTable.InitProbabilityArray();
            }
        }
        static void InitEstablishmentTable(string FileName, string KeyWord, Landis.Library.Biomass.Species.AuxParm<float[]> SpeciesTables)
        { 
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                SpeciesTables[species] = EstablishmentTable.ReadEstablishmentTable(KeyWord, FileName, species);
            }
        }

        InputParameters ReadSpeciesParameters(string FileName, InputParameters parameters)
        {
            // to get the option to read species parameters from an external file
            SpeciesParameterParser parser = new SpeciesParameterParser();

            IInputParameters speciesparameters = (IInputParameters)PlugIn.ModelCore.Load<IInputParameters>(FileName, parser);

            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                parameters.RootTurnover[species] = speciesparameters.RootTurnover[species];
                parameters.WoodTurnover[species] = speciesparameters.WoodTurnover[species];
                parameters.FoliageTurnover[species] = speciesparameters.FoliageTurnover[species];
                parameters.FolReten[species] = speciesparameters.FolReten[species];
                parameters.AmaxA[species] = speciesparameters.AmaxA[species];
                parameters.AmaxB[species] = speciesparameters.AmaxB[species];
                parameters.BaseFolRespFrac[species] = speciesparameters.BaseFolRespFrac[species];
                parameters.GrowthMoistureSensitivity[species] = speciesparameters.GrowthMoistureSensitivity[species];
                parameters.WiltingPoint[species] = speciesparameters.WiltingPoint[species];
                parameters.DVPD1[species] = speciesparameters.DVPD1[species];
                parameters.DVPD2[species] = speciesparameters.DVPD2[species];
                parameters.FolNCon[species] = speciesparameters.FolNCon[species];
                parameters.HalfSat[species] = speciesparameters.HalfSat[species];
                parameters.EstRadSensitivity[species] = speciesparameters.EstRadSensitivity[species];
                parameters.EstMoistureSensitivity[species] = speciesparameters.EstMoistureSensitivity[species];
                parameters.MaintResp[species] = speciesparameters.MaintResp[species];
                parameters.DNSC[species] = speciesparameters.DNSC[species];
                parameters.RootStemRatio[species] = speciesparameters.RootStemRatio[species];
                parameters.K[species] = speciesparameters.K[species];
                parameters.PsnAgeRed[species] = speciesparameters.PsnAgeRed[species];
                parameters.PsnTMin[species] = speciesparameters.PsnTMin[species];
                parameters.PsnTOpt[species] = speciesparameters.PsnTOpt[species];
                parameters.RespQ10[species] = speciesparameters.RespQ10[species];
                parameters.SenescStart[species] = speciesparameters.SenescStart[species];
                parameters.SLWmax[species] = speciesparameters.SLWmax[species];
                parameters.SLWDel[species] = speciesparameters.SLWDel[species];
                parameters.WUEConst[species] = speciesparameters.WUEConst[species];
                parameters.GDDFolStart[species] = speciesparameters.GDDFolStart[species];
                parameters.GDDFolEnd[species] = speciesparameters.GDDFolEnd[species];
                
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
