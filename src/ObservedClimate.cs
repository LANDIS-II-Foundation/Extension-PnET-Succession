using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class ObservedClimate : IObservedClimate
    {
        // One observedclimate object 
        private static Dictionary<string, IObservedClimate> ClimateData = new Dictionary<string, IObservedClimate>();

        public static Landis.Library.Parameters.Ecoregions.AuxParm<string> ClimateFileName ;

        List<ClimateDataSet> data_lines = new List<ClimateDataSet>();
         
        public static void Initialize()
        {
            ClimateFileName = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)PlugIn.GetParameter("climateFileName");

            Dictionary<IEcoregion, IObservedClimate> dict = new Dictionary<IEcoregion, IObservedClimate>();

            foreach(IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (ecoregion.Active == false) continue;

                else
                {
                    if (dict.ContainsKey(ecoregion))
                    {
                        ClimateData[ClimateFileName[ecoregion]] = dict[ecoregion];
                    }
                    else ClimateData[ClimateFileName[ecoregion]] = new ObservedClimate(ClimateFileName[ecoregion]);
                }
            }
        }
        public static IObservedClimate GetClimateData(IEcoregion ecoregion)
        {
            return ClimateData[ClimateFileName[ecoregion]];
        }


        public static ClimateDataSet GetData(IEcoregion ecoregion, DateTime date)
        {
            // get the appropriate values as read in from a climate txt file
            IObservedClimate observed_climate = GetClimateData(ecoregion);

            try
            {
                return GetData(date, observed_climate);
            }
            catch
            {
                throw new System.Exception("Can't get climate data for ecoregion "+ ecoregion.Name);
            }
            
        }

        public static ClimateDataSet GetData(DateTime date, IObservedClimate observed_climate)
        {
            foreach (ClimateDataSet d in observed_climate)
            {
                if (d.Year.Length == 4)
                {
                    if (int.Parse(d.Month) == date.Month && date.Year == int.Parse(d.Year))
                    {
                        return d;
                    }
                }
                else
                {
                    string[] yearExtremes = d.Year.Split('-');

                    if (int.Parse(d.Month) == date.Month && int.Parse(yearExtremes[0]) <= date.Year && date.Year <= int.Parse(yearExtremes[1]))
                    {
                        return d;
                    }
                }
            }
            throw new System.Exception("No climate entry for ecoregion date " + date);
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
                ClimateDataSet climate_dataset = new ClimateDataSet();

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
        public IEnumerator<ClimateDataSet> GetEnumerator()
        {
            return data_lines.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
