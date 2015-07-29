using System;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    using System;
    using System.Collections.Generic;
    using Edu.Wisc.Forest.Flel.Util;
    

    namespace Landis.Extension.Succession.BiomassPnET
    {
        public class PFCurvesParameterParser
             : TextParser<Dictionary<string, Landis.Library.Parameters.Species.AuxParm<string>>>
        {

            public override string LandisDataValue
            {
                get
                {
                    return PlugIn.ExtensionName;
                }
            }


            static M TryParse<M>(string value, string label)
            {
                M return_value;
                try
                {
                    return_value = (M)System.Convert.ChangeType(value, typeof(M));
                }
                catch
                {
                    throw new System.Exception("Expecting type " + typeof(M) + " for " + label);
                }
                return return_value;
            }
            private InputVar<M> ReadValue<M>(string label, StringReader currentLine)
            {
                InputVar<M> var = new InputVar<M>(label);
                ReadValue(var, currentLine);
                return var;
            }
            private void MakeSureFileContainsKeyWord(string KeyWord)
            {
                int maxLinesBeforeKeyword = 10000;// 
                int tries = 0;
                for (tries = 0; tries < maxLinesBeforeKeyword; tries++)
                {
                    if (ReadOptionalName(KeyWord) == true)
                    {
                        return;
                    }
                }
                throw new System.Exception("Could not find keyword " + KeyWord);
            }
            protected override Dictionary<string, Landis.Library.Parameters.Species.AuxParm<string>> Parse()
            {
                StringReader currentLine = new StringReader(CurrentLine);

                Dictionary<string, Landis.Library.Parameters.Species.AuxParm<string>> Parameters = new Dictionary<string, Library.Parameters.Species.AuxParm<string>>(StringComparer.InvariantCultureIgnoreCase);

                //ParameterList parameters = new ParameterList();
                MakeSureFileContainsKeyWord(Names.PnETGenericParameters);

                while (!AtEndOfInput)
                {
                    string[] Entries = new StringReader(CurrentLine).ReadLine().Trim().Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

                    Landis.Library.Parameters.Species.AuxParm<string> parameter = null;

                    if (Parameters.TryGetValue(Entries[0], out parameter) == false)
                    {
                        Parameters.Add(Entries[0], new Library.Parameters.Species.AuxParm<string>(PlugIn.ModelCore.Species));
                    }
                    foreach (ISpecies spc in PlugIn.ModelCore.Species)
                    {
                        Parameters[Entries[0]][spc] = Entries[1];
                    }

                    GetNextLine();
                }


                return Parameters;

            }

        }
    }
     
}
