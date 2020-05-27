using Landis.Core;
using Landis.Utilities;
using Landis.Library.DensityCohorts;
using System.Text;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Landis.Library.DensityCohorts.InitialCommunities
{
    /// <summary>
    /// A parser that reads a dataset of initial communities from text input.
    /// </summary>
    public class DatasetParser
        : TextParser<IDataset>
    {
        private int successionTimestep;
        private ISpeciesDataset speciesDataset;

        public override string LandisDataValue
        {
            get
            {
                return "Initial Communities";
            }
        }



        //---------------------------------------------------------------------

        public DatasetParser(int              successionTimestep,
                             ISpeciesDataset speciesDataset)
        {
            this.successionTimestep = successionTimestep;
            this.speciesDataset = speciesDataset;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Creates a new InputValueException for an invalid percentage input
        /// value.
        /// </summary>
        /// <returns></returns>
        public static InputValueException MakeInputValueException(string value,
                                                                  string message)
        {
            return new InputValueException(value,
                                           string.Format("\"{0}\" is not a valid aboveground biomass input", value.TrimStart('(')),
                                           new MultiLineText(message));
        }
        //---------------------------------------------------------------------

        protected override IDataset Parse()
        {
            ReadLandisDataVar();
            
            Dataset dataset = new Dataset();

            InputVar<uint> mapCode = new InputVar<uint>("MapCode");
            InputVar<string> speciesName = new InputVar<string>("Species");
            InputVar<ushort> age = new InputVar<ushort>("Age and Biomass");
            //InputVar<uint> biomass = new InputVar<uint>("Biomass gm-2");
            uint biomass = 0;

            Dictionary <uint, int> mapCodeLineNumbers = new Dictionary<uint, int>();

            while (! AtEndOfInput) {
                //  Read initial community
                int mapCodeLineNum = LineNumber;
                ReadVar(mapCode);
                int lineNumber;
                if (mapCodeLineNumbers.TryGetValue(mapCode.Value.Actual, out lineNumber))
                    throw new InputValueException(mapCode.Value.String,
                                                  "The map code {0} was previously used on line {1}",
                                                  mapCode.Value.Actual, lineNumber);
                else
                    mapCodeLineNumbers[mapCode.Value.Actual] = mapCodeLineNum;

                //  Read species and their ages
                List<ISpeciesCohorts> speciesCohortsList;
                
                speciesCohortsList = new List<ISpeciesCohorts>();
                Dictionary <string, int> speciesLineNumbers = new Dictionary<string, int>();
                while (! AtEndOfInput && CurrentName != mapCode.Name) {
                    StringReader currentLine = new StringReader(CurrentLine);

                    ReadValue(speciesName, currentLine);
                    ISpecies species = speciesDataset[speciesName.Value.Actual];
                    if (species == null)
                        throw new InputValueException(speciesName.Value.String,
                                                      "{0} is not a species name.",
                                                      speciesName.Value.String);
                    if (speciesLineNumbers.TryGetValue(species.Name, out lineNumber))
                        throw new InputValueException(speciesName.Value.String,
                                                      "The species {0} was previously used on line {1}",
                                                      speciesName.Value.String, lineNumber);
                    else
                        speciesLineNumbers[species.Name] = LineNumber;

                    //  Read ages
                    Dictionary<ushort, uint> ageBio = new Dictionary<ushort, uint>();
                    TextReader.SkipWhitespace(currentLine);
                    while (currentLine.Peek() != -1) {
                        ReadValue(age, currentLine);
                        if (age.Value.Actual == 0)
                            throw new InputValueException(age.Value.String,
                                                          "Ages must be > 0.");
                        if (ageBio.ContainsKey(age.Value.Actual))
                            throw new InputValueException(age.Value.String,
                                                          "The age {0} appears more than once.",
                                                          age.Value.String);
                        if (age.Value.Actual > species.Longevity)
                            throw new InputValueException(age.Value.String,
                                                          "The age {0} is more than longevity ({1}).",
                                                          age.Value.String, species.Longevity);
                        //TextReader.SkipWhitespace(currentLine);

                        //Landis.Library.Succession.Model.Core.UI.WriteLine("New read in biomass value.");
                        biomass = ReadBiomass(currentLine);
                        TextReader.SkipWhitespace(currentLine);
                        ageBio.Add(age.Value.Actual, biomass);
                    }
                    if (ageBio.Count == 0)
                        //  Try reading age which will throw exception
                        ReadValue(age, currentLine);

                    ageBio = BinAges(ageBio);

                    //JSF - Ask Brian about this, initialWoodBiomass is float, SpeciesCohorts receives int
                    foreach (ushort age_key in ageBio.Keys)
                    {
                        uint initialWoodBiomass = ageBio[age_key];
                        if (initialWoodBiomass <= 0.0)
                            throw new InputValueException(speciesName.Value.String,
                                                          "Cohort {0}, age {1} has zero or negative biomass, line {2}",
                                                          species.Name, age_key, lineNumber);

                        //JSF FIX speciesCohortsList.Add(new SpeciesCohorts(species, age_key, initialWoodBiomass, (float)0.0));
                        speciesCohortsList.Add(new SpeciesCohorts(species, age_key, (int)initialWoodBiomass));
                    }
                    
                    GetNextLine();
                }

                dataset.Add(new Community(mapCode.Value.Actual, speciesCohortsList));
            }

            return dataset;
        }

        //---------------------------------------------------------------------

        private Dictionary<ushort, uint> BinAges(Dictionary<ushort, uint> ageBios)
        {
            if (successionTimestep <= 0)
                return ageBios;

            Dictionary<ushort, uint> newList = new Dictionary<ushort, uint>();

            //ageBios.Sort();
            //for (int i = 0; i < ages.Count; i++) {
            //    ushort age = ages[i];
            //    if (age % successionTimestep != 0)
            //        ages[i] = (ushort) (((age / successionTimestep) + 1) * successionTimestep);
            //}

            foreach(ushort age in ageBios.Keys)
            {
                if (age % successionTimestep == 0)
                {
                    if (newList.ContainsKey(age))
                    {
                        newList[age] += ageBios[age];
                    }
                    else
                    {
                        newList.Add(age, ageBios[age]);
                    }
                }
                else
                {
                    ushort new_age = (ushort)(((age / successionTimestep) + 1) * successionTimestep);
                    if (newList.ContainsKey(new_age))
                        newList[new_age] += ageBios[age];
                    else
                        newList.Add(new_age, ageBios[age]);
                }
            }

            //    Remove duplicates, by going backwards through list from last
            //    item to the 2nd item, comparing each item with the one before
            //    it.
            //for (int i = ages.Count - 1; i >= 1; i--) {
            //    if (ages[i] == ages[i-1])
            //        ages.RemoveAt(i);
            //}

            return newList; 
        }

        public static InputValue<uint> ReadBiomass(StringReader reader)
        {
            TextReader.SkipWhitespace(reader);
            //index = reader.Index;

            //  Read left parenthesis
            int nextChar = reader.Peek();
            if (nextChar == -1)
                throw new InputValueException();  // Missing value
            if (nextChar != '(')
                throw MakeInputValueException(TextReader.ReadWord(reader),
                                              "Value does not start with \"(\"");
            
            StringBuilder valueAsStr = new StringBuilder();
            valueAsStr.Append((char)(reader.Read()));

            //  Read whitespace between '(' and percentage
            valueAsStr.Append(ReadWhitespace(reader));

            //  Read percentage
            string word = ReadWord(reader, ')');
            if (word == "")
                throw MakeInputValueException(valueAsStr.ToString(),
                                              "No biomass after \"(\"");
            valueAsStr.Append(word);
            uint biomass;
            try
            {
                biomass = (uint) Int32.Parse(word); 
            }
            catch (System.FormatException exc)
            {
                throw MakeInputValueException(valueAsStr.ToString(),
                                              exc.Message);
            }

            if (biomass < 0.0 || biomass > 500000)
                throw MakeInputValueException(valueAsStr.ToString(),
                                              string.Format("{0} is not between 0 and 500,000", biomass));

            //  Read whitespace and ')'
            valueAsStr.Append(ReadWhitespace(reader));
            char? ch = TextReader.ReadChar(reader);
            if (!ch.HasValue)
                throw MakeInputValueException(valueAsStr.ToString(),
                                              "Missing \")\"");
            valueAsStr.Append(ch.Value);
            if (ch != ')')
                throw MakeInputValueException(valueAsStr.ToString(),
                                              string.Format("Value ends with \"{0}\" instead of \")\"", ch));
            
            //Landis.Library.Succession.Model.Core.UI.WriteLine("Read in biomass value: {0}", biomass);

            return new InputValue<uint>(biomass, "Biomass gm-2"); 
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Reads whitespace from a string reader.
        /// </summary>
        public static string ReadWhitespace(StringReader reader)
        {
            StringBuilder whitespace = new StringBuilder();
            int i = reader.Peek();
            while (i != -1 && char.IsWhiteSpace((char)i))
            {
                whitespace.Append((char)reader.Read());
                i = reader.Peek();
            }
            return whitespace.ToString();
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Reads a word from a string reader.
        /// </summary>
        /// <remarks>
        /// The word is terminated by whitespace, the end of input, or a
        /// particular delimiter character.
        /// </remarks>
        public static string ReadWord(StringReader reader,
                                      char delimiter)
        {
            StringBuilder word = new StringBuilder();
            int i = reader.Peek();
            while (i != -1 && !char.IsWhiteSpace((char)i) && i != delimiter)
            {
                word.Append((char)reader.Read());
                i = reader.Peek();
            }
            return word.ToString();
        }
    }
}
