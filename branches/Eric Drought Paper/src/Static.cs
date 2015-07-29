using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using Edu.Wisc.Forest.Flel.Util;
using ExtensionMethods;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class Static
    {
        static int lastline = 1;
        static int YearCol;
        static int MonthCol;
        static int TMaxCol;
        static int TMinCol;
        static int CO2Col;
        static int PAR0col;
        static int Preccol;
        static int FirstYearOnFile;
        static int LastYearOnFile;
        static int ColumnCount = -1;
        private static char delim;

        private static string climateFileName;

        private static DateVar<int> wettestmonth;
        private static DateVar<float> tmax;
        private static DateVar<float> tmin;
        private static DateVar<float> co2;
        private static DateVar<float> tday;
        private static DateVar<float> par0;
        private static DateVar<float> tave; 
        private static DateVar<float> prec;
        private static DateVar<float> vpd;
        private static DateVar<int> dayspan;
        private static DateVar<bool> treesareactive;


        private static Landis.Library.Biomass.Species.AuxParm<float> DVPD1;
        private static Landis.Library.Biomass.Species.AuxParm<float> DVPD2;
        private static Landis.Library.Biomass.Species.AuxParm<float> AmaxA;
        private static Landis.Library.Biomass.Species.AuxParm<float> AmaxB;
        private static Landis.Library.Biomass.Species.AuxParm<float> FolNCon;
        private static Landis.Library.Biomass.Species.AuxParm<float> BaseFolRespFrac;
        private static Landis.Library.Biomass.Species.AuxParm<float> WUEConst;
        
        private static Landis.Library.Biomass.Species.AuxParm<float> RespQ10;
        private static Landis.Library.Biomass.Species.AuxParm<float> PsnTOpt;
        private static Landis.Library.Biomass.Species.AuxParm<float> PsnTMin;

        private static DateVar<Landis.Library.Biomass.Species.AuxParm<int>> firstactivemonthspc;
         
        private static DateVar<int> firstpossibleestmonth;
        private static DateVar<int> lastpossibleestmonth;
        private static DateVar<bool> possibleestmonth;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> refnetpsn;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> refresp;
        
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> dTempResp;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> dTempRespDay;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> dTempRespNight;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> wue;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> resp_folbase;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<bool>> isactive; // photosynthesis possible in view of temp
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> delamax;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> dtemppsn;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> dvpd;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> gdd;
        private static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> wue_co2_corr;

        
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> DVPD 
        { 
            get
            {
                return dvpd; 
            }
        }
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> DTempRespDay 
        { 
            get
            {
                return dTempRespDay; 
            }
        }
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> DTempRespNight 
        { 
            get
            {
                return dTempRespNight; 
            }
        }
        
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> DTempResp 
        { 
            get
            {
                return dTempResp; 
            }
        }
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> DTempPSN 
        { 
            get
            { 
                return dtemppsn; 
            }
        }
        
        
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> WUE_CO2_corr
        {
            get 
            { 
                return wue_co2_corr; 
            } 
        }
         
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> GDD
        {
            get
            {
                return gdd;
            }
        }

        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> RefResp
        {
            get
            {
                return refresp;
            }
        }
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> RefNetPsn
        {
            get
            {
                return refnetpsn;
            }
        }
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<bool>> IsActive
        {
            get
            {
                return isactive;
            }

        }
        public static DateVar<Landis.Library.Biomass.Species.AuxParm<float>> DelAmax
        {
            get
            {
                return delamax;
            }
        }
        public static DateVar<float> VPD
        {
            get
            {
                return vpd;
            }
        }
        public static DateVar<int> DaySpan
        {
            get
            {
                return dayspan;
            }
        }
        public static DateVar<float> Tave
        {
            get
            {
                return tave;
            }
        }

        public static DateVar<float> Prec
        {
            get
            {
                return prec;
            }
        }

        public static DateVar<bool> PossibleEstmonth
        {
            get
            {
                return possibleestmonth;
            }
        }
        public static DateVar<float> PAR0
        {
            get
            {
                return par0;
            }
        }
        public static DateVar<float> Tday
        {
            get
            {
                return tday;
            }
        }
        
        public static DateVar<float> TMin
        {
            get
            {
                return tmin;
            }
        }
        public static float LinearPsnTempResponse(float tday, float PsnTOpt, float PsnTMin)
        {
            if (tday < PsnTMin) return 0;
            else if (tday > PsnTOpt) return 1;

            else return (tday - PsnTMin) / (PsnTOpt - PsnTMin);
        }
        public static float ParabolicPsnTempResponse(float tday, float PsnTOpt, float PsnTMin)
        {
            float psntmax = PsnTOpt + (PsnTOpt - PsnTMin);    // assumed symmetrical
            float PsnTRange = psntmax - PsnTMin;
            float PsnTRangeHalfSquare = (float)Math.Pow((PsnTRange) / 2.0, 2);
 
            return (float)Math.Max(((psntmax - tday) * (tday - PsnTMin)) / (PsnTRangeHalfSquare), 0);
        }

        public static void Initialize(IInputParameters Parameters)
        {
            PlugIn.ModelCore.UI.WriteLine("   Reading climate date from " + Parameters.climateFileName);

            climateFileName = Parameters.climateFileName;
            SetDelimitor();
            string[] Content = ReadContent();
            SetFirstAndLastYearOnFile(Content);

            firstpossibleestmonth = new DateVar<int>("firstpossibleestmonth", FirstYearOnFile, LastYearOnFile);
            lastpossibleestmonth = new DateVar<int>("lastpossibleestmonth", FirstYearOnFile, LastYearOnFile);
            possibleestmonth = new DateVar<bool>("PossibleEstmonth", FirstYearOnFile, LastYearOnFile);
            tmax = new DateVar<float>("tmax", FirstYearOnFile , LastYearOnFile);
            tmin = new DateVar<float>("tmin", FirstYearOnFile, LastYearOnFile);
            co2 = new DateVar<float>("co2", FirstYearOnFile, LastYearOnFile);
            tday = new DateVar<float>("tday", FirstYearOnFile, LastYearOnFile);
            par0 = new DateVar<float>("par0", FirstYearOnFile, LastYearOnFile);
            tave = new DateVar<float>("tave", FirstYearOnFile, LastYearOnFile);
            prec = new DateVar<float>("prec", FirstYearOnFile, LastYearOnFile);
            vpd = new DateVar<float>("vpd", FirstYearOnFile, LastYearOnFile);
            dayspan = new DateVar<int>("dayspan", FirstYearOnFile, LastYearOnFile);
            treesareactive = new DateVar<bool>("treesareactive", FirstYearOnFile, LastYearOnFile);


            SetClimateFileHeaders(Content[0]);

            FolNCon = Parameters.FolNCon;
            BaseFolRespFrac = Parameters.BaseFolRespFrac;
            DVPD1 = Parameters.DVPD1;
            DVPD2 = Parameters.DVPD2;
            AmaxA = Parameters.AmaxA;
            AmaxB = Parameters.AmaxB;
            WUEConst = Parameters.WUEConst;
             
            RespQ10 = Parameters.RespQ10;
            PsnTOpt = Parameters.PsnTOpt;
            PsnTMin = Parameters.PsnTMin;


            firstactivemonthspc = new DateVar<Landis.Library.Biomass.Species.AuxParm<int>>("wettestmonth", FirstYearOnFile, LastYearOnFile);
            wettestmonth = new DateVar<int>("wettestmonth", FirstYearOnFile, LastYearOnFile);
            dtemppsn = new DateVar<Library.Biomass.Species.AuxParm<float>>("dtemppsn", FirstYearOnFile, LastYearOnFile);
            wue = new DateVar<Library.Biomass.Species.AuxParm<float>>("wue", FirstYearOnFile, LastYearOnFile);
            delamax = new DateVar<Library.Biomass.Species.AuxParm<float>>("delamax", FirstYearOnFile, LastYearOnFile);
            refnetpsn = new DateVar<Library.Biomass.Species.AuxParm<float>>("refnetpsn", FirstYearOnFile, LastYearOnFile);
            refresp = new DateVar<Library.Biomass.Species.AuxParm<float>>("refresp", FirstYearOnFile, LastYearOnFile);
            dTempResp = new DateVar<Library.Biomass.Species.AuxParm<float>>("dTempResp", FirstYearOnFile, LastYearOnFile);
            dTempRespDay = new DateVar<Library.Biomass.Species.AuxParm<float>>("dTempRespDay", FirstYearOnFile, LastYearOnFile);
            dTempRespNight = new DateVar<Library.Biomass.Species.AuxParm<float>>("dTempRespNight", FirstYearOnFile, LastYearOnFile);
            wue_co2_corr = new DateVar<Library.Biomass.Species.AuxParm<float>>("wue_co2_corr", FirstYearOnFile, LastYearOnFile);
            dvpd = new DateVar<Library.Biomass.Species.AuxParm<float>>("dvpd", FirstYearOnFile, LastYearOnFile);
            resp_folbase = new DateVar<Library.Biomass.Species.AuxParm<float>>("resp_folbase", FirstYearOnFile, LastYearOnFile);
            isactive = new DateVar<Library.Biomass.Species.AuxParm<bool>>("IsActive", FirstYearOnFile, LastYearOnFile);
            gdd = new DateVar<Library.Biomass.Species.AuxParm<float>>("gdd", FirstYearOnFile, LastYearOnFile);
           
            DateTime date = new DateTime(FirstYearOnFile, 1, 15);
            while (date.CompareTo(new DateTime(LastYearOnFile, 12, 15)) <= 0)
            {
                // GetLine checks if year/month according to the loop is equal to the date read in
                string[] terms = GetLine(date.Year, date.Month, Content).Split(delim);
                if (terms.Count() != ColumnCount)
                {
                    throw new System.Exception("Unexpected number of columns in " + Parameters.climateFileName + " date = " + date);
                }

                if (int.Parse(terms[0]) != date.Year || int.Parse(terms[1]) != date.Month)
                {
                    throw new System.Exception("Missing month " + date + " in climate file");
                }
                try
                {
                    tmax[date] = CheckInputValue(float.Parse(terms[TMaxCol]), -80, 80, "date = " + date);
                    tmin[date] = CheckInputValue(float.Parse(terms[TMinCol]), -80, tmax[date], "date = " + date);
                    co2[date] = CheckInputValue(float.Parse(terms[CO2Col]), 0, float.MaxValue, "date = " + date);
                    par0[date] = CheckInputValue(float.Parse(terms[PAR0col]), 0, float.MaxValue, "date = " + date);
                    prec[date] = CheckInputValue(float.Parse(terms[Preccol]), 0, float.MaxValue, "date = " + date);
                    if (prec[date] < 0) throw new System.Exception("Precipitation = " + prec + "\t" + date);

                    float hr = Calculate_hr(date.DayOfYear, Parameters.Latitude);
                    float daylength = Calculate_DayLength(hr);
                    float nightlength = Calculate_NightLength(hr);
                    dayspan[date] = Calculate_DaySpan(date.Month);
                    tave[date] = (float)0.5 * (tmin[date] + tmax[date]);
                    tday[date] = (float)0.5 * (tmax[date] + tave[date]);
                    vpd[date] = Calculate_VPD(Tday[date], TMin[date]);

                    gdd[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    isactive[date] = new Library.Biomass.Species.AuxParm<bool>(PlugIn.ModelCore.Species);
                    dtemppsn [date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    wue[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    delamax[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    refnetpsn[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    refresp[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    dTempResp [date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    dTempRespDay[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    dTempRespNight[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    wue_co2_corr[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    resp_folbase[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    gdd[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                    dvpd[date] = new Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);

                    foreach (ISpecies spc in PlugIn.ModelCore.Species)
                    {
                        

                        float dGDD = Math.Max(0, (Tave[date] - PsnTMin[spc]) * DaySpan[date]);
                        if (date.Month == 1) gdd[date][spc] = dGDD;
                        else
                        {
                            DateTime last_month = new DateTime(date.Year, date.Month - 1, 15);
                            gdd[date][spc] = gdd[last_month][spc] + dGDD;
                        }

                        if (tday[date] < PsnTMin[spc]) IsActive[date][spc] = false;
                        else
                        {
                            IsActive[date][spc] = true;
                            treesareactive[date] = true;
                        }

                         
                        //dtemppsn[date][spc] = ParabolicPsnTempResponse(tday[date], PsnTOpt[spc], PsnTMin[spc]);
                        dtemppsn[date][spc] = LinearPsnTempResponse(tday[date], PsnTOpt[spc], PsnTMin[spc]);
                        
                         
                        dvpd[date][spc] = Math.Max(0, 1 - DVPD1[spc] * (float)Math.Pow(vpd[date], DVPD2[spc]));

                        float cicaRatio = (-0.075f * FolNCon[spc]) + 0.875f;
                        float ci350 = 350 * cicaRatio;
                        float Arel350 = 1.22f * ((ci350 - 68) / (ci350 + 136));

                        float ciElev = co2[date] * cicaRatio;
                        float ArelElev = 1.22f * ((ciElev - 68) / (ciElev + 136));
                        DelAmax[date][spc] = 1 + ((ArelElev - Arel350) / Arel350);


                        // CO2 effect on photosynthesis
                        // Calculate CO2 effect on conductance and set slope and intercept for A-gs relationship
                        float Delgs = DelAmax[date][spc] / ((co2[date] - co2[date] * cicaRatio) / (350.0f - ci350));

                        wue[date][spc] = (WUEConst[spc] / vpd[date]) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance

                        wue_co2_corr[date][spc] = wue[date][spc] / DelAmax[date][spc];

                        /*
                        wue_co2_corr[spc] = WUEConst[spc];
                        Delgs = DelAmax / ((site.CaMo[mo] - CiElev) / (350.0 - Ci350));
                        DWUE = 1.0 + (1 - Delgs);
                        gsSlope = (-1.1309 * DelAmax) + 1.9762;   // used to determine ozone uptake
                        gsInt = (0.4656 * DelAmax) - 0.9701;
                        */

                        // NETPSN
                        float amax =  DelAmax[date][spc] * (AmaxA[spc] + AmaxB[spc] * FolNCon[spc]);
 
                         //Reference net Psn (lab conditions)
                        RefNetPsn[date][spc] = Static.DaySpan[date] * (amax * dvpd[date][spc] * daylength * Constants.MC) / Constants.billion;

                        dTempRespDay[date][spc] = ((float)Math.Pow(RespQ10[spc], (Tday[date] - PsnTOpt[spc]) / 10));
                        dTempRespNight[date][spc] = ((float)Math.Pow(RespQ10[spc], (TMin[date] - PsnTOpt[spc]) / 10));

                        dTempResp[date][spc] = (float)Math.Min(1.0, (dTempRespDay[date][spc] * daylength + dTempRespNight[date][spc] * nightlength) / ((float)daylength + (float)nightlength));

                        //  gC/day
                        RefResp[date][spc] = BaseFolRespFrac[spc] * Static.DaySpan[date] * (amax * daylength  * Constants.MC) / Constants.billion;

                         
                    }
                    date = date.AddMonths(1);
                }
                catch
                {
                    throw new System.Exception("Cannot find climate date for (year,month) " + date.Year.ToString() + " " + date.Month.ToString());
                }

            }

           
            while (date.CompareTo(new DateTime(LastYearOnFile, 12, 15)) < 0)
            {
                if (Parameters.EstMoistureSensitivity != null)
                {
                    wettestmonth[date] = GetWettestMonth(date, 10);
                }
                if (Parameters.EstRadSensitivity != null)
                {
                    firstactivemonthspc[date] = GetFirstActiveMonth(date, 10);
                }

                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    int Firstpossibleestmonth = Math.Min(wettestmonth[date] - 1, firstactivemonthspc[date][spc]);
                    int Lastpossibleestmonth = Math.Max(wettestmonth[date] + 1, firstactivemonthspc[date][spc] + 3);

                    if (date.Month >= Firstpossibleestmonth  && date.Month <= Lastpossibleestmonth )
                    {
                        possibleestmonth[date] = true;
                    }
                
                }

                

                date = date.AddMonths(1);
            }

            
        }
        public static DateVar<bool> TreesAreActive
        {
            get 
            {
                return treesareactive;
            }
        }
        
        enum FirstLastYear
        {
            First = 0,
            Last = 1
        }
        static int GetWettestMonth(DateTime date, int timespan)
        {
            DateTime DateCounter = date.AddYears(-5);

            float[] P = new float[13];
            while (DateCounter.CompareTo(date.AddYears(5)) < 1)
            {
                if (DateCounter.Year >= FirstYearOnFile && DateCounter.Year<= LastYearOnFile)
                {
                    P[DateCounter.Month] += prec[DateCounter];
                }
                DateCounter = DateCounter.AddMonths(1);
            }
            int MonthMaxPrec = 0;
            float MaxPrec = float.MinValue;

            for (int f = 1; f < P.Count(); f++)
            {
                if (P[f] > MaxPrec)
                {
                    MaxPrec = P[f];
                    MonthMaxPrec = f;
                }
            }
            return MonthMaxPrec;
        }
        static Library.Biomass.Species.AuxParm<int>GetFirstActiveMonth(DateTime date, int timespan)
        {
            Library.Biomass.Species.AuxParm<int> FirstActiveMonth = new Library.Biomass.Species.AuxParm<int>(PlugIn.ModelCore.Species);

            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                DateTime DateCounter = date.AddYears(-5);

                 
                bool FoundFirstActiveMonth= false;
                int SumFirstActiveMonth=0;
                int c=0;
                while (DateCounter.CompareTo(date.AddYears(5)) < 1)
                {
                    if (DateCounter.Month == 1)
                    {
                        FoundFirstActiveMonth = false;
                    }
                    if (DateCounter.Year >= FirstYearOnFile && DateCounter.Year < LastYearOnFile)
                    {
                        if (isactive[DateCounter][spc] && FoundFirstActiveMonth == false)
                        {
                            FoundFirstActiveMonth = true;
                            SumFirstActiveMonth += DateCounter.Month;
                            c++;
                        }
                    }
                    
                    DateCounter = DateCounter.AddMonths(1);
                }
                FirstActiveMonth[spc] = (int)SumFirstActiveMonth / c;
            }
            return FirstActiveMonth;
        }
        
        public static T CheckInputValue<T>(T value, T min, T max, string label)
        {
            if (double.Parse(value.ToString()) > double.Parse(max.ToString()))
            {
                throw new System.Exception("Input value exceeds maximum " + label);
            }
            if (double.Parse(value.ToString()) < double.Parse(min.ToString()))
            {
                throw new System.Exception("Input value below minimum " + label);
            }
            return value;
        }
        static string GetLine(int Year, int Month, string[] Content)
        {
            // Assumes that the location is one line below the line where the Year/Month combination was last found
            string line;
            int FirstYear, LastYear, month;
            try
            {
                lastline++;
                if (lastline < Content.Count() - 1)
                {
                    line = Content[lastline];
                    FirstYear = GetYearFrom4charOr9charString(line.Split(delim)[0], FirstLastYear.First);
                    LastYear = GetYearFrom4charOr9charString(line.Split(delim)[0], FirstLastYear.Last);
                    month = int.Parse(line.Split(delim)[MonthCol]);
                    if (month == Month && Year >= FirstYear && Year <= LastYear)
                    {
                        return line;
                    }
                }

                for (lastline = 1; lastline < Content.Count(); lastline++)
                {
                    line = Content[lastline];
                    FirstYear = GetYearFrom4charOr9charString(line.Split(delim)[0], FirstLastYear.First);
                    LastYear = GetYearFrom4charOr9charString(line.Split(delim)[0], FirstLastYear.Last);
                    month = int.Parse(line.Split(delim)[MonthCol]);

                    if (month == Month && Year >= FirstYear && Year <= LastYear)
                    {

                        return line;
                    }

                }
            }
            catch (System.Exception e)
            {
                throw e;
            }
            throw new System.Exception("Cannot retrieve climate data for (year, month) " + Year + "," + Month);
        }
        static int GetYearFrom4charOr9charString(string s, FirstLastYear f)
        {
            // Two possible input formats: 
            // A range of years: 1600-1700: Take term #p
            // Just a year: 1600: Take this year
            try
            {
                if (s.Contains('-'))
                {
                    string[] terms = s.Split('-');

                    if (f == FirstLastYear.First) return int.Parse(terms[0]);
                    else if (f == FirstLastYear.Last) return int.Parse(terms[1]);
                    else throw new System.Exception("Cannot retrieve a year from the string " + s + " check file " + climateFileName);
                }
                else
                {
                    return int.Parse(s);
                }
            }
            catch
            {
                throw new System.Exception("Cannot retrieve a year from the string " + s + " check file " + climateFileName);
            }
        }
        private static int GetColNr(string[] Headers, string Label)
        {
            for (int h = 0; h < Headers.Count(); h++)
            {
                string hdr = Headers[h];
                if (hdr.Contains(Label, StringComparison.OrdinalIgnoreCase))
                {
                    return h;
                }
                if (Headers[h].Contains(Label, StringComparison.OrdinalIgnoreCase))
                {
                    return h;
                }
            }
            throw new System.Exception("Cannot find header " + Label + " in " + climateFileName);
        }
        private static void SetFirstAndLastYearOnFile(string[] Content)
        {
            try
            {
                FirstYearOnFile = GetYearFrom4charOr9charString(Content[1].Split(delim)[0], FirstLastYear.First);
                LastYearOnFile = GetYearFrom4charOr9charString(Content[Content.Count() - 1].Split(delim)[YearCol], FirstLastYear.Last);
            }
            catch
            {
                throw new System.Exception("Irregular table format in " + climateFileName);
            }
        }
        private static void SetClimateFileHeaders(string HeaderLine)
        {

            string[] Headers = HeaderLine.Split(delim);
            ColumnCount = Headers.Count();
            YearCol = GetColNr(Headers, "Year");
            MonthCol = GetColNr(Headers, "Month");
            TMaxCol = GetColNr(Headers, "TMax");
            TMinCol = GetColNr(Headers, "TMin");
            CO2Col = GetColNr(Headers, "CO2");
            PAR0col = GetColNr(Headers, "PAR");
            Preccol = GetColNr(Headers, "Prec");
        }
        private static string[] ReadContent()
        {
            string[] Content = System.IO.File.ReadAllLines(climateFileName);
            for (int line = 0; line < Content.Count(); line++)
            {
                Content[line] = Content[line].Replace("\t\t", "\t");
            }
            return Content;
        }
        private static void SetDelimitor()
        {
            if (climateFileName.Contains(".csv", StringComparison.OrdinalIgnoreCase))
            {
                delim = ',';
            }
            else if (climateFileName.Contains(".txt", StringComparison.OrdinalIgnoreCase))
            {
                delim = '\t';
            }
            else throw new System.Exception("Cannot determine delimitor in " + climateFileName + " use extension csv for comma-delimited, or txt for tab-delimited climate input");
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
        
        private static float Calculate_hr(int DOY, float Latitude)
        {
            float TA;
            float AC;
            float LatRad;
            float r;
            float z;
            float decl;
            float z2;
            float h;

            LatRad = Latitude * (2.0f * (float)Math.PI) / 360.0f;
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
        private static float Calculate_VP(float a, float b, float c, float T)
        {
            float vp = a * (float)Math.Exp(b * T / (T + c));
            return vp;
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
    }
}
