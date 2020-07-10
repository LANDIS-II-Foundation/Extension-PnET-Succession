using Landis.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.DensityCohorts
{
    public class ParameterTableParser
         : TextParser<Dictionary<string, Parameter<string>>>  
    {

        public Dictionary<string, Parameter<string>> Parameters { get; private set; }
        public List<string> ExpectedRowLabels { get; private set; }
        public List<string> ExpectedColumnHeaders { get; private set; }
        
        private Dictionary<string, int> speciesLineNums;
        private InputVar<string> speciesName;
        private string FileName;
        private string KeyWord;
        private bool transposed;

        public override string LandisDataValue
        {
            get
            {
                return Names.ExtensionName ;
            }
        }

        public ParameterTableParser(string FileName, string KeyWord, List<string> ExpectedRowLabels, List<string> ExpectedColumnHeaders, bool transposed = false)
        {
            this.ExpectedColumnHeaders = ExpectedColumnHeaders;
            this.ExpectedRowLabels = ExpectedRowLabels;
            this.transposed = transposed;
            this.KeyWord = KeyWord;
            this.FileName = FileName;
            this.speciesLineNums = new Dictionary<string, int>();
            this.speciesName = new InputVar<string>("Species");
             
        }
        static bool ListContains(List<string> List, string value)
        { 
            return List.Any(s => s.Equals(value, StringComparison.OrdinalIgnoreCase));
        }
        void CheckHeaderCount(StringReader s, int ReadHeaderLabelsCount)
        {
            string headerline = s.ReadLine();
            if (headerline.Contains(">>"))
            {
                int fi =headerline.IndexOf(">>");
                while (headerline.Length>fi)
                {
                    headerline = headerline.Remove(headerline.Length - 1);
                }
            }
            List<string> Terms = new List<string>(headerline.Trim().Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));
            if (Terms.Count != ReadHeaderLabelsCount)
            {
                throw new System.Exception("Headers/column numbers unequal");
            }
        }
        protected override Dictionary<string, Parameter<string>> Parse()
        {
            try
            {
                Dictionary<string, Parameter<string>> parameters = new Dictionary<string, Parameter<string>>(StringComparer.InvariantCultureIgnoreCase);

                InputVar<string> landisData = new InputVar<string>("LandisData");
                ReadVar(landisData);

                if (landisData.Value.Actual != this.KeyWord)
                {
                    throw new InputValueException(landisData.Value.String, "Landis Keyword expected " + this.KeyWord + " but read \"{0}\"" + this.FileName, landisData.Value.Actual);
                }

                string line = new StringReader(CurrentLine).ReadLine().Trim();
                
                while (line.Length == 0)
                {
                    GetNextLine();
                }

                List<string> ReadHeaderLabels  = new List<string>(line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));
                if (line.ToLower().Contains(KeyWord.ToLower()) == false) throw new System.Exception("Expecting keyword "+ KeyWord +" in headerline");

                if (ExpectedColumnHeaders != null)
                {
                    ExpectedColumnHeaders.Add(KeyWord);

                    foreach (string label in ReadHeaderLabels) if (ListContains(ExpectedColumnHeaders, label) == false)
                    {
                            throw new PnetSpeciesParameterFileFormatException("Unrecognized column header " + label + " in " + FileName + "./nExpected headers are: " + string.Join(",", ExpectedColumnHeaders.ToArray()));
                    }
                }

                GetNextLine();

                speciesLineNums.Clear();


                while (!AtEndOfInput)
                {
                    //string line2 = new StringReader(CurrentLine).ReadLine().Trim();
                    //List<string> Terms = new List<string>(new StringReader(CurrentLine).ReadLine().Trim().Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));

                    

                    InputVar<string> RowLabel = new InputVar<string>("RowLabel");

                    StringReader s = new StringReader(CurrentLine);

                    CheckHeaderCount(new StringReader(CurrentLine), ReadHeaderLabels.Count);
                    

                    for (int column = 0; column < ReadHeaderLabels.Count; column++)
                    {

                        Parameter<string> parameter = null;
                        string parameterlabel = null;
                         
                        string valuekey = null;

                        InputVar<string> var = new InputVar<string>(ReadHeaderLabels[column]);
                        ReadValue(var, s);

                        if (column == 0)
                        {
                            RowLabel = var;
                            if (ExpectedRowLabels != null && ListContains(ExpectedRowLabels, RowLabel.Value) == false)
                            {
                                throw new PnetSpeciesParameterFileFormatException("Unknown parameter label [" + var.Value + "] in "+ FileName + ".\nExpected labels are: [" + string.Join(" ,", ExpectedRowLabels.ToArray())+"].");
                            }

                            continue;
                        }
                        
                         

                        switch (transposed)
                        {
                            case true:
                                parameterlabel =  RowLabel.Value;
                                valuekey = ReadHeaderLabels[column]; 
                                break;
                            case false:
                                parameterlabel = ReadHeaderLabels[column];
                                
                                valuekey = RowLabel.Value;
                                break;
                        }
                        if (parameters.TryGetValue(parameterlabel, out parameter) == false)
                        {
                            parameter = new Parameter<string>(parameterlabel);
                            parameters.Add(parameterlabel, parameter);
                        }
                        if (parameter.ContainsKey(valuekey))
                        {
                            throw new System.Exception("Duplicate parameter label [" + var.Value + "] for parameter " + parameterlabel);
                        }

                        parameter.Add(valuekey, var.Value);

                    }
                    GetNextLine();
                }


                return parameters;
            }
            catch (System.Exception e)
            {
                if (e is PnetSpeciesParameterFileFormatException)
                {
                    throw e;
                }
                else
                {
                    throw new System.Exception("Unexpected file format (dir,file) (" +
                        System.IO.Directory.GetCurrentDirectory() + "," +
                        this.FileName + ")" +
                        " " + e.Message + "\n\nNOTE header line is mandatory");
                }
            }
        }
        

    }
    class PnetSpeciesParameterFileFormatException : Exception
    {
        public PnetSpeciesParameterFileFormatException()
        {
            
        }
        public PnetSpeciesParameterFileFormatException(string msg)
            :base(msg)
        {

        }
    }

    
/*    public class BiomassParam { }

    class BiomassParamParser : Landis.TextParser<BiomassParam>
    {
        public override string LandisDataValue
        {
            get { return "BiomassCoefficients"; }
        }

        //read biomass coefficients from a file into matrix, (float) BioMassData(int ID,2). No Return Value
        //Read a ID first from the file, and ID is the size of BioMassData;
        //Read the two variable in to BioMassData(v1,v2)
        protected override BiomassParam Parse()
        {
            ReadLandisDataVar();

            InputVar<int> speciesnum = new InputVar<int>("Number_of_species_class");
            ReadVar(speciesnum);
            PlugIn.biomass_util.SetBiomassNum(speciesnum.Value.Actual);

            InputVar<float> biomassThreshold = new InputVar<float>("minimum_DBH_for_calculating_biomass");
            ReadVar(biomassThreshold);
            PlugIn.biomass_util.BiomassThreshold = biomassThreshold.Value.Actual;

            InputVar<float> float_val = new InputVar<float>("V0 or V1 value for each species");

            for (int i = 1; i <= speciesnum.Value.Actual; i++)
            {
                if (AtEndOfInput)
                    throw NewParseException("Expected a line here");

                Landis.Utilities.StringReader currentLine = new Landis.Utilities.StringReader(CurrentLine);

                ReadValue(float_val, currentLine);

                PlugIn.biomass_util.SetBiomassData(i, 1, float_val.Value.Actual);


                ReadValue(float_val, currentLine);

                PlugIn.biomass_util.SetBiomassData(i, 2, float_val.Value.Actual);

                //CheckNoDataAfter("the Ecoregion " + float_val + " column",
                //                 currentLine);

                GetNextLine();
            }

            return null;
        }
    }
*/    
}

