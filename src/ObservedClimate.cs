using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class ObservedClimate
    {
        // One observedclimate object 
        private static Dictionary<string, ObservedClimate> ClimateData = new Dictionary<string, ObservedClimate>();

        public static Landis.Library.Parameters.Ecoregions.AuxParm<string> ClimateFileName ;

        List<DataSet> data_lines = new List<DataSet>();

        static int line_counter = 0;
       
        public static void Initialize()
        {
            ClimateFileName = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)PlugIn.GetParameter("climateFileName");

            Dictionary<IEcoregion, ObservedClimate> dict = new Dictionary<IEcoregion, ObservedClimate>();

            foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (ecoregion.Active == false) continue;

                else
                {
                    ObservedClimate obs = new ObservedClimate(ClimateFileName[ecoregion]);

                    if (dict.ContainsKey(ecoregion))
                    {
                        ClimateData[ClimateFileName[ecoregion]] = dict[ecoregion];
                    }
                    else ClimateData[ClimateFileName[ecoregion]] = new ObservedClimate(ClimateFileName[ecoregion]);
                }
            }
        }

        public static DataSet GetData(IEcoregion ecoregion, DateTime date)
        {
            // get the appropriate values as read in from a climate txt file
            ObservedClimate o = ClimateData[ClimateFileName[ecoregion]];

            int index = line_counter + 1;
            while (index != line_counter)
            {
                if (index == o.data_lines.Count) index = 0;

                DataSet d = o.data_lines[index];

                if (d.Year.Length == 4)
                {
                    if (int.Parse(d.Month) == date.Month && date.Year == int.Parse(d.Year))
                    {
                        line_counter = index;
                        return d;
                    }
                }
                else
                {
                    string[] yearExtremes = d.Year.Split('-');

                    if (int.Parse(d.Month) == date.Month && int.Parse(yearExtremes[0]) <= date.Year && date.Year <= int.Parse(yearExtremes[1]))
                    {
                        line_counter = index;
                        return d;
                    }
                }
                index++;
                
            }
            throw new System.Exception("Not climate entry for ecoregion " + ecoregion.Name +" date "+ date);
        }
         
        public struct DataSet
        {
            // One line in a climate file
            public string Year;
            public string Month;
            public float CO2;
            public string O3;
            public ushort PAR0;
            public float Prec;
            public float Tmin;
            public float Tmax;
        }

        public struct ColumnNumbers
        {
            public int Year;
            public int Month;
            public int TMax;
            public int TMin;
            public int CO2;
            public int PAR0;
            public int Prec;

            private static int GetColNr(string[] Headers, string Label)
            {
                for (int h = 0; h < Headers.Count(); h++)
                {
                    if (System.Globalization.CultureInfo.InvariantCulture.CompareInfo.IndexOf(Headers[h], Label, System.Globalization.CompareOptions.IgnoreCase) >= 0) return h;
                }
                throw new System.Exception("Cannot find header " + Label);
            }

            public ColumnNumbers(string HeaderLine)
            {
                string[] Headers = HeaderLine.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

                Year = GetColNr(Headers, "Year");
                Month = GetColNr(Headers, "Month");
                TMax = GetColNr(Headers, "TMax");
                TMin = GetColNr(Headers, "TMin");
                CO2 = GetColNr(Headers, "CO2");
                PAR0 = GetColNr(Headers, "PAR");
                Prec = GetColNr(Headers, "Prec");
            }
        }

        static string[] ReadClimateFile(string climatefilename)
        {
            string[] ClimateFileContent = System.IO.File.ReadAllLines(climatefilename).Where(l => !String.IsNullOrEmpty(l.Trim())).ToArray();
            for (int line = 0; line < ClimateFileContent.Count(); line++)
            {
                int startcomment = ClimateFileContent[line].IndexOf(">>");
                if (startcomment > 0)
                {
                    ClimateFileContent[line] = ClimateFileContent[line].Remove(startcomment, ClimateFileContent[line].Count() - startcomment);
                }
            }
            return ClimateFileContent;
        }

        private static T CheckInRange<T>(T value, T min, T max, string label)
           where T : System.IComparable<T>
        {
            if (Landis.Library.Parameters.InputValue_ExtensionMethods.GreaterThan<T>(value, max))
            {
                throw new System.Exception(label + " is out of range " + min + " " + max);
            }
            if (Landis.Library.Parameters.InputValue_ExtensionMethods.LessThan<T>(value, min))
            {
                throw new System.Exception(label + " is out of range " + min + " " + max);
            }
            return value;
        }

        public ObservedClimate(string filename)
        {
            List<string> ClimateFileContent = new List<string>(ReadClimateFile(filename));
            ColumnNumbers columns = new ColumnNumbers(ClimateFileContent[0]);
            ClimateFileContent.Remove(ClimateFileContent[0]);
             
            foreach (string line in ClimateFileContent)
            {
                DataSet climate_dataset = new DataSet();

                string[] terms = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

                // Get one state of static information for the line in the climate file
                climate_dataset.Tmax = CheckInRange<float>(float.Parse(terms[columns.TMax]), -80, 80, "TMax");
                climate_dataset.Tmin = CheckInRange<float>(float.Parse(terms[columns.TMin]), -80, climate_dataset.Tmax, "TMin");
                climate_dataset.CO2 = CheckInRange<float>(float.Parse(terms[columns.CO2]), 0, float.MaxValue, "CO2");
                climate_dataset.PAR0 = (ushort)CheckInRange<float>(float.Parse(terms[columns.PAR0]), 0, float.MaxValue, "PAR0");

                climate_dataset.Prec = CheckInRange<float>(float.Parse(terms[columns.Prec]), 0, float.MaxValue, "PREC");

                climate_dataset.Year = terms[columns.Year];
                climate_dataset.Month = terms[columns.Month];

                data_lines.Add(climate_dataset);
            }

        }
    }
}
