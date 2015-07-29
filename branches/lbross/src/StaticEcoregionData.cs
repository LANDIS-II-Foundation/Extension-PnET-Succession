using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Core;
using Landis.SpatialModeling;
namespace Landis.Extension.Succession.BiomassPnET
{
    public class EcoregionDateData  
    {
        private static Landis.Library.Parameters.Ecoregions.AuxParm<SortedDictionary<DateTime, EcoregionDateData>> allstaticdata;
        private static SortedDictionary<string, SortedDictionary<DateTime, EcoregionDateData>> EcoregionsWithIdenticalClimateFile = new SortedDictionary<string, SortedDictionary<DateTime, EcoregionDateData>>();

        private static Landis.Library.Parameters.Ecoregions.AuxParm<DateTime> FirstDate;
        private static Landis.Library.Parameters.Ecoregions.AuxParm<DateTime> LastDate;
        private static Landis.Library.Parameters.Ecoregions.AuxParm<string> ClimateFileName;
        private static float Latitude;
        
        
        public static int MaxPar0 { get; private set; }


       
        public bool AnyLeaf_On { get; private set; }
        public bool AnyLeafChange { get; private set; }
        public float Tday { get; private set; }
        public float PAR0 { get; private set; }
        public float Prec { get; private set; }
        public float VPD { get; private set; }
        public float Newsnow { get; private set; }
        public float Precin { get; private set; }
        public float Maxmonthlysnowmelt { get; private set; }
        DateTime date;

        public byte Month
        {
            get
            {
                return (byte)date.Month;
            }
        }
        public float Year
        {
            get
            {
                return date.Year + 1F / 12F * (date.Month-1);
            }
        }
       


        public Landis.Library.Parameters.Species.AuxParm<bool> Leaf_On { get; private set; }
        public Landis.Library.Parameters.Species.AuxParm<bool> Leaf_Change { get; private set; }
        public Landis.Library.Parameters.Species.AuxParm<float> FTempResp { get; private set; }
        public Landis.Library.Parameters.Species.AuxParm<float> FTempRespMaintResp;
        public Landis.Library.Parameters.Species.AuxParm<float> FTempRespDay;
        public Landis.Library.Parameters.Species.AuxParm<float> FTempRespDayRefResp;
        public Landis.Library.Parameters.Species.AuxParm<float> FTempPSN;
        public Landis.Library.Parameters.Species.AuxParm<float> FTempPSNRefNetPsn;
        public Landis.Library.Parameters.Species.AuxParm<float> WUE_CO2_corr;
        public Landis.Library.Parameters.Species.AuxParm<float> RefResp;
        public Landis.Library.Parameters.Species.AuxParm<float> RefNetPsn;
        public Landis.Library.Parameters.Species.AuxParm<float> DVPD { get; private set; }

       
        public static List<EcoregionDateData> Get(IEcoregion Ecoregion, DateTime from, DateTime to)
        {
            List<EcoregionDateData> data = new List<EcoregionDateData>();
            while (from < to)
            {
                EcoregionDateData monthdata = EcoregionDateData.Get(Ecoregion, from);// 
                data.Add(monthdata);
                from = from.AddMonths(1);
            }
            return data;
        }
        private static EcoregionDateData Get(IEcoregion Ecoregion, DateTime date)
        {
            try
            {
                return allstaticdata[Ecoregion][date];
            }
            catch(System.Exception e)
            {
                if (date < FirstDate[Ecoregion] || date > LastDate[Ecoregion])
                {
                    throw new System.Exception("No data for month/year " + date.Month + "/" + date.Year + " for ecoregion " + Ecoregion.Name + " in climatefile " + ClimateFileName[Ecoregion]);
                }

                throw new System.Exception("No data for month/year " + date.Month + "/" + date.Year + " for ecoregion " + Ecoregion.Name + " in climatefile "+ e.Message);
            }
            
            
        }
        public EcoregionDateData(string[] terms, ColumnNumbers columns)
        {
            Leaf_On = new Library.Parameters.Species.AuxParm<bool>(PlugIn.ModelCore.Species);
            DVPD = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempResp = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempPSNRefNetPsn = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempRespDayRefResp = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempPSN = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            WUE_CO2_corr = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            RefNetPsn = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempRespDay = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            RefResp = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempRespMaintResp = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            Leaf_Change = new Library.Parameters.Species.AuxParm<bool>(PlugIn.ModelCore.Species);
            AnyLeaf_On = false;

            DateTime Date = GetYearsInDateRange(terms[columns.Year], terms[columns.Month])[0];

            float Tmax = CheckInRange<float>(float.Parse(terms[columns.TMax]), -80, 80, "TMax");
            float Tmin = CheckInRange<float>(float.Parse(terms[columns.TMin]), -80, Tmax, "TMin");
            float co2 = CheckInRange<float>(float.Parse(terms[columns.CO2]), 0, float.MaxValue, "CO2");
            PAR0 = CheckInRange<float>(float.Parse(terms[columns.PAR0]), 0, float.MaxValue, "PAR0");
            MaxPar0 = (int)Math.Max(MaxPar0, PAR0);
            Prec = CheckInRange<float>(float.Parse(terms[columns.Prec]), 0, float.MaxValue, "PREC");

            float hr = Calculate_hr(Date.DayOfYear, Latitude);
            float daylength = Calculate_DayLength(hr);
            float nightlength = Calculate_NightLength(hr);
            int dayspan = Calculate_DaySpan(Date.Month);
            float Tave = (float)0.5 * (Tmin + Tmax);
            Tday = (float)0.5 * (Tmax + Tave);
            VPD = Calculate_VPD(Tday, Tmin);

            float snowfraction = CumputeSnowFraction(Tave);
            Newsnow = snowfraction * Prec;//mm
            Maxmonthlysnowmelt = 0.15f * Math.Max(0, Tave) * dayspan;
            Precin = (1 - snowfraction) * Prec;

            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                if (Tmin > spc.PsnTMin())
                {
                    Leaf_On[spc] = true;
                    AnyLeaf_On = true;
                }
                else
                {
                    Leaf_On[spc] = false;
                }

                //ftemppsn[date, spc] = ParabolicPsnTempResponse(tday[ecoregion, date], Parameters.PsnTOpt[spc], Parameters.PsnTMin[spc]);
                FTempPSN[spc] = LinearPsnTempResponse(Tday, spc.PsnTOpt(), spc.PsnTMin());

                DVPD[spc] = Math.Max(0, 1 - spc.DVPD1() * (float)Math.Pow(VPD, spc.DVPD2()));

                float cicaRatio = (-0.075f * spc.FolN()) + 0.875f;
                float ci350 = 350 * cicaRatio;
                float Arel350 = 1.22f * ((ci350 - 68) / (ci350 + 136));

                float ciElev = co2 * cicaRatio;
                float ArelElev = 1.22f * ((ciElev - 68) / (ciElev + 136));
                float delamax = 1 + ((ArelElev - Arel350) / Arel350);


                // CO2 effect on photosynthesis
                // Calculate CO2 effect on conductance and set slope and intercept for A-gs relationship
                float Delgs = delamax / ((co2 - co2 * cicaRatio) / (350.0f - ci350));

                //wue[ecoregion, spc, date] = (Parameters.WUEcnst[spc] / vpd[ecoregion, date]) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance
                float wue = (spc.WUEcnst() / VPD) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance
                WUE_CO2_corr[spc] = wue / delamax;

                // NETPSN
                float amax = delamax * (spc.AmaxA() + spc.AmaxB() * spc.FolN());

                //Reference net Psn (lab conditions)
                RefNetPsn[spc] = dayspan * (amax * DVPD[spc] * daylength * Constants.MC) / Constants.billion;

                FTempRespDay[spc] = ((float)Math.Pow(spc.Q10(), (Tday - spc.PsnTOpt()) / 10));

                float fTempRespNight = ((float)Math.Pow(spc.Q10(), (Tmin - spc.PsnTOpt()) / 10));

                FTempResp[spc] = (float)Math.Min(1.0, (FTempRespDay[spc] * daylength + fTempRespNight * nightlength) / ((float)daylength + (float)nightlength));

                FTempRespMaintResp[spc] = FTempResp[spc] * spc.MaintResp();
                //  gC/day
                RefResp[spc] = spc.BFolResp() * dayspan * (amax * daylength * Constants.MC) / Constants.billion;

                FTempPSNRefNetPsn[spc] = FTempPSN[spc] * RefNetPsn[spc];

                FTempRespDayRefResp[spc] = FTempRespDay[spc] * RefResp[spc];
            }
        }
        public EcoregionDateData(DateTime date, EcoregionDateData e)
        {
            this.date = date;
            this.Leaf_On = e.Leaf_On;
            this.Leaf_Change = e.Leaf_Change;
            this.AnyLeaf_On = e.AnyLeaf_On;
            this.FTempResp = e.FTempResp;
            this.FTempRespDayRefResp = e.FTempRespDayRefResp;
            this.FTempPSNRefNetPsn = e.FTempPSNRefNetPsn;
            this.FTempRespMaintResp = e.FTempRespMaintResp;
            this.FTempRespDay = e.FTempRespDay;
            this.FTempPSN = e.FTempPSN;
            this.WUE_CO2_corr = e.WUE_CO2_corr;
            this.RefResp = e.RefResp;
            this.RefNetPsn = e.RefNetPsn;
            this.DVPD = e.DVPD;
            this.Tday = e.Tday;
            this.PAR0 = e.PAR0;
            this.Prec = e.Prec;
            this.VPD = e.VPD;
            this.Newsnow = e.Newsnow;
            this.Precin = e.Precin;
            this.Maxmonthlysnowmelt = e.Maxmonthlysnowmelt;

        }
        
        public static void Initialize()
        {
            FirstDate = new Library.Parameters.Ecoregions.AuxParm<DateTime>(PlugIn.ModelCore.Ecoregions);
            LastDate = new Library.Parameters.Ecoregions.AuxParm<DateTime>(PlugIn.ModelCore.Ecoregions);

            ClimateFileName = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)PlugIn.GetParameter("climateFileName"); 
 
            Latitude = ((Parameter<float>)PlugIn.GetParameter(Names.Latitude, 0, 90)).Value;

            allstaticdata = new Library.Parameters.Ecoregions.AuxParm<SortedDictionary<DateTime, EcoregionDateData>>(PlugIn.ModelCore.Ecoregions);
            PlugIn.ModelCore.UI.WriteLine("Initializing static data");

            MyClock m = new MyClock(PlugIn.ModelCore.Ecoregions.Count);
            
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) if (ecoregion.Active)
            {
                m.Next();

                m.WriteUpdate();

                EcoregionDateData.allstaticdata[ecoregion] = EcoregionDateData.GetStaticEcoregionData(ecoregion);
            }
        }


        public static SortedDictionary<DateTime, EcoregionDateData> GetStaticEcoregionData(IEcoregion ecoregion)
        {
            SortedDictionary<DateTime, EcoregionDateData> Data = null;

            if (EcoregionsWithIdenticalClimateFile.TryGetValue(ClimateFileName[ecoregion], out Data))
            {
                return Data;
            }

            Data = new SortedDictionary<DateTime, EcoregionDateData>();
            List<string> ClimateFileContent = new List<string>(ReadClimateFile(ClimateFileName[ecoregion]));
            ColumnNumbers columns = new ColumnNumbers(ClimateFileContent[0]);
            ClimateFileContent.Remove(ClimateFileContent[0]);

            foreach (string line in ClimateFileContent)
            {
                string[] terms = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

                EcoregionDateData data = new EcoregionDateData(terms, columns);
                
                DateTime[] DateRange = GetYearsInDateRange(terms[columns.Year], terms[columns.Month]);
 
                foreach (DateTime rdate in DateRange)
                {
                    Data.Add(rdate, new EcoregionDateData(rdate, data));
                }

                
            }
            SetLeafChangeData(Data);

            foreach (System.Collections.Generic.KeyValuePair<DateTime, EcoregionDateData> entry in Data)
            {
                if (entry.Key != entry.Value.date)
                {
                    throw new System.Exception("Data entry error");
                }
            }

            EcoregionsWithIdenticalClimateFile.Add(ClimateFileName[ecoregion], Data);
            return Data;
             
             
        }

        static void SetLeafChangeData(SortedDictionary<DateTime, EcoregionDateData> Data)
        {
            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                EcoregionDateData lastecoregiondata = null;
                foreach (EcoregionDateData ecoregiondata in Data.Values)
                {
                    if (lastecoregiondata != null)
                    {
                        if (ecoregiondata.Leaf_On[spc] == false && lastecoregiondata.Leaf_On[spc] == true)
                        {
                            ecoregiondata.Leaf_Change[spc] = true;
                            ecoregiondata.AnyLeafChange = true;
                        }
                    }
                    lastecoregiondata = ecoregiondata;
                }
            }
        }
        /*
        static void CountEstablishmentMonths(SortedDictionary<DateTime, EcoregionDateData> Data)
        {
            foreach (EcoregionDateData ecoregiondata in Data.Values)
            {
                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    ecoregiondata.PossibleEstmonthCount[spc] = 0;
                    foreach (int month in Enum.GetValues(typeof(Landis.Library.BiomassCohortsPnET.Constants.Months)))
                    {
                        if (ecoregiondata.Leaf_On[spc] == true)
                        {
                            Data[new DateTime(ecoregiondata.date.Year, month, 15)].PossibleEstmonthCount[spc]++;
                        }
                    }


                }
            }
        }
         */
        static DateTime GetDateFrom4charOr9charString(string yearstring, string monthstring, string climatefilename, int FirstLastYear)
        {
            try
            {
                int month;
                if (int.TryParse(monthstring, out month) == false)
                {
                    throw new System.Exception("Cannot parse month from " + monthstring);
                }

                // Range if dates
                if (yearstring.Contains('-'))
                {
                    string[] terms = yearstring.Split('-');

                    int year;
                    if (int.TryParse(terms[FirstLastYear], out year) == false)
                    {
                        throw new System.Exception("Cannot parse year from " + yearstring + " " + monthstring + " in " + climatefilename);
                    }
                    return new DateTime(year, month, 15);
                }
                else
                {
                    int year;
                    if (int.TryParse(yearstring, out year) == false)
                    {
                        throw new System.Exception("Cannot parse year from " + monthstring);
                    }

                    return new DateTime(year, month, 15);
                }
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Cannot get year from " + climatefilename + " " + e.Message);
            }
        }
        
         
        
        static float LinearPsnTempResponse(float tday, float PsnTOpt, float PsnTMin)
        {
            if (tday < PsnTMin) return 0;
            else if (tday > PsnTOpt) return 1;

            else return (tday - PsnTMin) / (PsnTOpt - PsnTMin);
        }
        static DateTime[] GetYearsInDateRange(string yearstring, string monthstring)
        {
            List<DateTime> DateRange = new System.Collections.Generic.List<System.DateTime>();

            int firstyear, lastyear, month;
            if (int.TryParse(monthstring, out month) == false)
            {
                throw new System.Exception("Cannot parse " + monthstring + " to month");
            }
            if (yearstring.Contains("-"))
            {
                if (int.TryParse(yearstring.Split('-')[0], out firstyear) == false)
                {
                    throw new System.Exception("Cannot parse " + yearstring + " to year");
                }
                if (int.TryParse(yearstring.Split('-')[1], out lastyear) == false)
                {
                    throw new System.Exception("Cannot parse " + yearstring + " to year");
                }
            }
            else
            {
                if (int.TryParse(yearstring, out firstyear) == false)
                {
                    throw new System.Exception("Cannot parse " + yearstring + " to year");
                }
                lastyear = firstyear;
            }
            DateTime FirstDate = new System.DateTime(firstyear, month, 15);
            DateTime LastDate = new System.DateTime(lastyear, month, 15);

            while (FirstDate <= LastDate)
            {
                DateRange.Add(FirstDate);
                FirstDate = FirstDate.AddYears(1);
            }

            return DateRange.ToArray();
        }
        static int[] GetAllYears(DateTime from, DateTime to)
        {
            List<int> AllYears = new List<int>();
            while (from < to)
            {
                AllYears.Add(from.Year);
                from = from.AddYears(1);
            }
            return AllYears.ToArray();
        }
        static DateTime[] GetAllMonths(DateTime from, DateTime to)
        {
            List<DateTime> AllDates = new System.Collections.Generic.List<System.DateTime>();
            while (from < to)
            {
                AllDates.Add(from);
                from = from.AddMonths(1);
            }
            return AllDates.ToArray();
        }
        private static float Calculate_VPD(float Tday, float TMin)
        {

            float emean;
            //float delta;

            //saturated vapor pressure
            float es = Calculate_VP(0.61078f, 17.26939f, 237.3f, Tday);
            // 0.61078f * (float)Math.Exp(17.26939f * Tday / (Tday + 237.3f));

            //delta = 4098.0f * es / ((Tday + 237.3f) * (Tday + 237.3f));
            if (Tday < 0)
            {
                es = Calculate_VP(0.61078f, 21.87456f, 265.5f, Tday);
                //0.61078f * (float)Math.Exp(21.87456f * Tday / (Tday + 265.5f));
                //delta = 5808.0f * es / ((Tday + 265.5f) * (Tday + 265.5f));
            }

            emean = Calculate_VP(0.61078f, 17.26939f, 237.3f, TMin);
            //0.61078f * (float)Math.Exp(17.26939f * TMin / (TMin + 237.3f));
            if (TMin < 0) emean = Calculate_VP(0.61078f, 21.87456f, 265.5f, TMin);
            //0.61078f * (float)Math.Exp(21.87456f * TMin / (TMin + 265.5f));

            return es - emean;
        }
        //-------------------------------------------------------------------------------------------------
        private static float CumputeSnowFraction(float Tave)
        {
            if (Tave > 2) return 0;
            else if (Tave < -5) return 1;
            else return (Tave - 2) / -7;
        }
        static int Calculate_DOY(int Month)
        {
            return 30 * Month - 15;
        }
        static float Calculate_NightLength(float hr)
        {
            return 60 * 60 * (24 - hr);
        }
        static float Calculate_DayLength(float hr)
        {
            return 60 * 60 * hr;
        }
        private static float Calculate_VP(float a, float b, float c, float T)
        {
            return  a * (float)Math.Exp(b * T / (T + c));
        }
        private static float Calculate_hr(int DOY, double Latitude)
        {
            float TA;
            float AC;
            float LatRad;
            float r;
            float z;
            float decl;
            float z2;
            float h;

            LatRad = (float)Latitude * (2.0f * (float)Math.PI) / 360.0f;
            r = 1 - (0.0167f * (float)Math.Cos(0.0172f * (DOY - 3)));
            z = 0.39785f * (float)Math.Sin(4.868961f + 0.017203f * DOY + 0.033446f * (float)Math.Sin(6.224111f + 0.017202f * DOY));

            if ((float)Math.Abs(z) < 0.7f) decl = (float)Math.Atan(z / ((float)Math.Sqrt(1.0f - z * z)));
            else decl = (float)Math.PI / 2.0f - (float)Math.Atan((float)Math.Sqrt(1 - z * z) / z);

            if ((float)Math.Abs(LatRad) >= (float)Math.PI / 2.0)
            {
                if (Latitude < 0) LatRad = (-1.0f) * ((float)Math.PI / 2.0f - 0.01f);
                else LatRad = 1 * ((float)Math.PI / 2.0f - 0.01f);
            }
            z2 = -(float)Math.Tan(decl) * (float)Math.Tan(LatRad);

            if (z2 >= 1.0) h = 0;
            else if (z2 <= -1.0) h = (float)Math.PI;
            else
            {
                TA = (float)Math.Abs(z2);
                if (TA < 0.7) AC = 1.570796f - (float)Math.Atan(TA / (float)Math.Sqrt(1 - TA * TA));
                else AC = (float)Math.Atan((float)Math.Sqrt(1 - TA * TA) / TA);
                if (z2 < 0) h = 3.141593f - AC;
                else h = AC;
            }
            return 2 * (h * 24) / (2 * (float)Math.PI);
        }
        private static int Calculate_DaySpan(int Month)
        {
            if (Month == 1) return 31;
            else if (Month == 2) return 28;
            else if (Month == 3) return 31;
            else if (Month == 4) return 30;
            else if (Month == 5) return 31;
            else if (Month == 6) return 30;
            else if (Month == 7) return 31;
            else if (Month == 8) return 31;
            else if (Month == 9) return 30;
            else if (Month == 10) return 31;
            else if (Month == 11) return 30;
            else if (Month == 12) return 31;
            else throw new System.Exception("Cannot calculate DaySpan, month = " + Month);
        }
        //---------------------------------------------------------------------------------------------
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
        //---------------------------------------------------------------------------------------------
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
         
         
    }
}


