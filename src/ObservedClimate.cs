using Landis.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class ObservedClimate : IObservedClimate
    {
        #region private variables
        // One observedclimate object  
        private static Dictionary<string, IObservedClimate> ClimateData = new Dictionary<string, IObservedClimate>(); 
        private static Landis.Library.Parameters.Ecoregions.AuxParm<string> ClimateFileName; 

        private string year;
        private string month;
        private float par0;
        private float prec;
        private float tmin;
        private float tmax;
        private float co2;
        private float o3;
        private List<ObservedClimate> data_lines = new List<ObservedClimate>();
        #endregion

        #region public accessors
        public float CO2
        {
            get
            {
                return co2;
            }
        }
        public string Year
        {
            get
            {
                return year;
            }
        }
        public string Month
        {
            get
            {
                return month;
            }
        }
        public float PAR0
        {
            get
            {
                return par0;
            }
        }
        public float Prec
        {
            get
            {
                return prec;
            }
        }
        public float Tmin
        {
            get
            {
                return tmin;
            }
        }
        public float O3 
         { 
             get 
             { 
                 return o3; 
             } 
         } 

        public float Tmax
        {
            get
            {
                return tmax;
            }
        }

        #endregion

         
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


        public static ObservedClimate GetData(IEcoregion ecoregion, DateTime date)
        {
            // get the appropriate values as read in from a climate txt file
            IObservedClimate observed_climate = GetClimateData(ecoregion);

            try
            {
                return GetData(observed_climate, date);
            }
            catch
            {
                throw new System.Exception("Can't get climate data for ecoregion "+ ecoregion.Name + " and date " + date.ToString());
            }
            
        }
        
        public static ObservedClimate GetData(IObservedClimate observed_climate, DateTime date)
        {
            foreach (ObservedClimate d in observed_climate)
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
            public int O3;

            private static int GetColNr(string[] Headers, string Label)
            {
                for (int h = 0; h < Headers.Count(); h++)
                {
                    if (System.Globalization.CultureInfo.InvariantCulture.CompareInfo.IndexOf(Headers[h], Label, System.Globalization.CompareOptions.IgnoreCase) >= 0) return h;
                }
                return -1;
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
                O3 = GetColNr(Headers, "O3");
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
        ObservedClimate()
        { 
        
        }
        public ObservedClimate(string filename)
        {
            List<string> ClimateFileContent = new List<string>(ReadClimateFile(filename));
            ColumnNumbers columns = new ColumnNumbers(ClimateFileContent[0]);
            ClimateFileContent.Remove(ClimateFileContent[0]);
             
            foreach (string line in ClimateFileContent)
            {
                ObservedClimate climate = new ObservedClimate();
                string[] terms = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

                // Get one state of static information for the line in the climate file
                climate.tmax = CheckInRange<float>(float.Parse(terms[columns.TMax]), -80, 80, "TMax");
                climate.tmin = CheckInRange<float>(float.Parse(terms[columns.TMin]), -80, climate.tmax, "TMin");
                climate.co2 = CheckInRange<float>(float.Parse(terms[columns.CO2]), 0, float.MaxValue, "CO2");
                climate.par0 = (ushort)CheckInRange<float>(float.Parse(terms[columns.PAR0]), 0, float.MaxValue, "PAR0");
                climate.prec = CheckInRange<float>(float.Parse(terms[columns.Prec]), 0, float.MaxValue, "PREC");
                climate.o3 = columns.O3 > 0 ? CheckInRange<float>(float.Parse(terms[columns.O3]), 0, float.MaxValue, "O3") : 0;

                climate.year = terms[columns.Year];
                climate.month = terms[columns.Month];

                data_lines.Add(climate);
            }

        }
        public IEnumerator<ObservedClimate> GetEnumerator()
        {
            return data_lines.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
