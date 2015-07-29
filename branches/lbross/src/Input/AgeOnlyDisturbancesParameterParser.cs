using System;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class AgeOnlyDisturbancesParameterParser
         : TextParser<Dictionary<string, Percentage>>  
    {
        string FileName;

        public static string GetLabel(string pool, string disturbance)
        {
            return pool+disturbance;
        }

        public override string LandisDataValue
        {
            get
            {
                return PlugIn.ExtensionName;
            }
        }

        public AgeOnlyDisturbancesParameterParser(string FileName)
        {
            this.FileName = FileName;
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
       
        
        protected override Dictionary<string, Percentage> Parse()
        {

            Dictionary<string, Percentage> parameters = new Dictionary<string, Percentage>(StringComparer.InvariantCultureIgnoreCase);
             
            // to get the option to read species parameters from an external file
            MakeSureFileContainsKeyWord(Names.AgeOnlyDisturbances);
            
            StringReader currentLine = new StringReader(CurrentLine);

            string line = currentLine.ReadLine().Trim();
            Dictionary<string,int> HeaderLabels = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
            if (line.Contains("Pool"))
            {
                List<string> Terms = new List<string>(line.Trim().Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));
                for (int t = 0; t < Terms.Count; t++)
                {
                    HeaderLabels.Add(Terms[t], t);
                }
                GetNextLine();
            }

            while (!AtEndOfInput)
            {
                currentLine = new StringReader(CurrentLine);

                List<string> Terms = new List<string>(currentLine.ReadLine().Trim().Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries));
                parameters[GetLabel(Terms[HeaderLabels["Pool"]],Terms[HeaderLabels["Disturbance"]])] = new Percentage(double.Parse(Terms[HeaderLabels["FracLost"]].Replace("%", "")));

                GetNextLine();
            }

             
            return parameters;
            
        }
        
    }
}
