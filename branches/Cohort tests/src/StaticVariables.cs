using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using Edu.Wisc.Forest.Flel.Util;
 

namespace Landis.Extension.Succession.BiomassPnET
{
    // This class calculates all variables that are static, like temperature, incident par etc. 
    // It includes a number of calculations such as potential photosynthesis etc that depend on a species and on a temperature
    // but not on the vegetation. These are saved in arrays that define the value of these vars according to the date.
    public class StaticVariables
    {
        private static DateTime[] DateRange;
        private static int lastline = 1; // to prevent that the climate file has to be searched for each date

        private enum FirstLastYear
        {
            First = 0,
            Last = 1
        }
        private struct ColumnNumbers
        {
            public static int Year;
            public static int Month;
            public static int TMax;
            public static int TMin;
            public static int CO2;
            public static int PAR0;
            public static int Prec;
        }
        
        private static int ColumnCount = -1;

        private static VarEcoregionSpecies<float> wiltingpoint_mm;
        private static Library.Biomass.Ecoregions.AuxParm<string> climateFileName;
        private static VarEcoregionDate<float> tmax;
        private static VarEcoregionDate<float> tmin;
        private static VarEcoregionDate<float> tday;
        private static VarEcoregionDate<float> par0;
        private static VarEcoregionDate<float> tave;
        private static VarEcoregionDate<float> prec;
        private static VarEcoregionDate<float> vpd;
        private static VarEcoregionSpeciesDate<bool> possibleestmonth;
        private static VarEcoregionDate<float> newsnow;
        private static VarEcoregionDate<float> precin;
        private static VarEcoregionDate<float> maxmonthlysnowmelt;
        private static VarEcoregionSpeciesDate<float> refnetpsn;
        private static VarEcoregionSpeciesDate<float> refresp;
        private static VarEcoregionSpeciesDate<float> fTempResp;
        private static VarEcoregionSpeciesDate<float> fTempRespDay;
        private static VarEcoregionSpeciesDate<float> ftemppsn;
        private static VarEcoregionSpeciesDate<float> dvpd;
        private static VarEcoregionSpeciesDate<bool> leaf_on;
        private static VarEcoregionSpeciesDate<float> gdd;
        private static VarEcoregionSpeciesDate<float> hdd;
        private static VarEcoregionSpeciesDate<float> wue_co2_corr;
        public static VarEcoregionSpeciesDate<bool> Leaf_On { get { return leaf_on; } }
        public static VarEcoregionSpeciesDate<float> DTempRespDay { get { return fTempRespDay; } }
        public static VarEcoregionSpeciesDate<float> FTempResp { get { return fTempResp; } }
        public static VarEcoregionSpeciesDate<float> FTempPSN { get { return ftemppsn; } }
        public static VarEcoregionSpeciesDate<float> WUE_CO2_corr { get { return wue_co2_corr; } }
        public static VarEcoregionSpeciesDate<float> GDD { get { return gdd; } }
        public static VarEcoregionSpeciesDate<float> HDD { get { return hdd; } }
        public static VarEcoregionSpeciesDate<float> RefResp { get { return refresp; } }
        public static VarEcoregionSpeciesDate<float> RefNetPsn { get { return refnetpsn; } }
        public static VarEcoregionDate<float> VPD { get { return vpd; } }
        public static VarEcoregionDate<float> Tave { get { return tave; } }
        public static VarEcoregionDate<float> Prec { get { return prec; } }
        public static VarEcoregionDate<float> MaxMonthlySnowMelt { get { return maxmonthlysnowmelt; } }
        public static VarEcoregionDate<float> PrecIn { get { return precin; } }
        public static VarEcoregionDate<float> NewSnow { get { return newsnow; } }
        public static VarEcoregionSpeciesDate<bool> PossibleEstmonth { get { return possibleestmonth; } }
        public static VarEcoregionDate<float> PAR0 { get { return par0; } }
        public static VarEcoregionDate<float> Tday { get { return tday; } }
        public static VarEcoregionDate<float> TMin { get { return tmin; } }
        public static VarEcoregionSpecies<float> WiltingPoint_mm { get { return wiltingpoint_mm; } }

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
        private static float CumputeSnowFraction(float Tave)
        {
            if (Tave > 2) return 0;
            else if (Tave < -5) return 1;
            else return (Tave - 2) / -7;
        }

        private static VarEcoregionSpecies<float> GetWiltingPoint(IInputParameters Parameters)
        {
            wiltingpoint_mm = new VarEcoregionSpecies<float>("wiltingpoint_mm");
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                foreach (ISpecies spc in PlugIn.modelCore.Species)
                {
                    wiltingpoint_mm[ecoregion,spc] = Parameters.WltPnt[spc] * Parameters.WHC[ecoregion];
                }
            }
            return wiltingpoint_mm;
        }
        static DateTime[] GetDataRange()
        {
            DateTime[] NewDateRange = null;
            DateTime[] OldDateRange = null;

            string OldClimFile = null;
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (ecoregion.Active == false) continue;
                string[] Content = ReadContent(climateFileName[ecoregion]);
                NewDateRange = GetFirstAndLastYearOnFile(Content);
                if (OldDateRange == null)
                {
                    OldDateRange = NewDateRange;
                    OldClimFile = climateFileName[ecoregion];
                }
                else
                {
                    if (NewDateRange[0].Year != OldDateRange[0].Year) throw new System.Exception("Climate data " + climateFileName[ecoregion] + " does not have identical timespan as " + OldClimFile);
                }
            }
            return NewDateRange;
        }
        static bool TryCopyVarsFromOtherEcoRegion(IEcoregion ecoregion)
        {
            foreach (IEcoregion previous_ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (previous_ecoregion == ecoregion) return false;
                if (climateFileName[previous_ecoregion] == climateFileName[ecoregion])
                {
                    foreach (IEcoregion eco in PlugIn.modelCore.Ecoregions)
                    {
                        foreach (ISpecies spc in PlugIn.modelCore.Species)
                        {
                            DateTime d = DateRange[0];
                            while (d < DateRange[1])
                            {
                                DTempRespDay[ecoregion, spc, d] = DTempRespDay[previous_ecoregion, spc, d];
                                FTempResp[ecoregion, spc, d] = FTempResp[previous_ecoregion, spc, d];
                                FTempPSN[ecoregion, spc, d] = FTempPSN[previous_ecoregion, spc, d];
                                WUE_CO2_corr[ecoregion, spc, d] = WUE_CO2_corr[previous_ecoregion, spc, d];
                                GDD[ecoregion, spc, d] = GDD[previous_ecoregion, spc, d];
                                HDD[ecoregion, spc, d] = HDD[previous_ecoregion, spc, d];
                                RefResp[ecoregion, spc, d] = RefResp[previous_ecoregion, spc, d];
                                RefNetPsn[ecoregion, spc, d] = RefNetPsn[previous_ecoregion, spc, d];
                                VPD[ecoregion, d] = VPD[previous_ecoregion, d];
                                Tave[ecoregion, d] = Tave[previous_ecoregion, d];
                                Prec[ecoregion, d] = Prec[previous_ecoregion, d];
                                VPD[ecoregion, d] = VPD[previous_ecoregion, d];
                                MaxMonthlySnowMelt[ecoregion, d] = MaxMonthlySnowMelt[previous_ecoregion, d];
                                PrecIn[ecoregion, d] = PrecIn[previous_ecoregion, d];
                                NewSnow[ecoregion, d] = NewSnow[previous_ecoregion, d];
                                PossibleEstmonth[ecoregion,spc, d] = PossibleEstmonth[previous_ecoregion,spc, d];
                                PAR0[ecoregion, d] = PAR0[previous_ecoregion, d];
                                Tday[ecoregion, d] = Tday[previous_ecoregion, d];
                                TMin[ecoregion, d] = TMin[previous_ecoregion, d];
                                WiltingPoint_mm[ecoregion, spc] = WiltingPoint_mm[previous_ecoregion, spc];

                                d = d.AddMonths(1);
                            }
                        }
                    }
                    return true;
                }
            }
            throw new System.Exception("Error copying vars from previous ecoregion, unexpected error.");
        }
        public static void InitializeStatic(IInputParameters Parameters)
        {
            climateFileName = Parameters.climateFileName;

            DateRange = GetDataRange();

            possibleestmonth = new VarEcoregionSpeciesDate<bool>("PossibleEstmonth", DateRange);
            newsnow = new VarEcoregionDate<float>("newsnow", DateRange);
            maxmonthlysnowmelt = new VarEcoregionDate<float>("maxmonthlysnowmelt", DateRange);
            precin = new VarEcoregionDate<float>("precin", DateRange);
            tmax = new VarEcoregionDate<float>("tmax", DateRange);
            tmin = new VarEcoregionDate<float>("tmin", DateRange);
            tday = new VarEcoregionDate<float>("tday", DateRange);
            par0 = new VarEcoregionDate<float>("par0", DateRange);
            tave = new VarEcoregionDate<float>("tave", DateRange);
            prec = new VarEcoregionDate<float>("prec", DateRange);
            vpd = new VarEcoregionDate<float>("vpd", DateRange);

            
            ftemppsn = new VarEcoregionSpeciesDate<float>("dtemppsn", DateRange);
            refresp = new VarEcoregionSpeciesDate<float>("refresp", DateRange);
            fTempResp = new VarEcoregionSpeciesDate<float>("dTempResp", DateRange);
            fTempRespDay = new VarEcoregionSpeciesDate<float>("dTempRespDay", DateRange);
            wue_co2_corr = new VarEcoregionSpeciesDate<float>("wue_co2_corr", DateRange);
            dvpd = new VarEcoregionSpeciesDate<float>("dvpd", DateRange);
            leaf_on = new VarEcoregionSpeciesDate<bool>("leaf_on", DateRange);
            gdd = new VarEcoregionSpeciesDate<float>("gdd", DateRange);
            hdd = new VarEcoregionSpeciesDate<float>("hdd", DateRange);

            refnetpsn = new VarEcoregionSpeciesDate<float>("refnetpsn", DateRange);

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (ecoregion.Active == false) continue;
                
                if (TryCopyVarsFromOtherEcoRegion(ecoregion))
                {
                    PlugIn.ModelCore.UI.WriteLine("   Copied static climate data for ecoregion " + ecoregion);
                    continue;
                }
                PlugIn.ModelCore.UI.WriteLine("   Initializing static climate data for ecoregion " + ecoregion + " from " + Parameters.climateFileName[ecoregion]);

                string[] Content = ReadContent(climateFileName[ecoregion]);
               
                SetClimateFileHeaders(Content[0]);

                wiltingpoint_mm = GetWiltingPoint(Parameters);

                DateTime date = DateRange[0];
                while (date.CompareTo(DateRange[1]) <= 0)
                {
                    // GetLine checks if year/month according to the loop is equal to the date read in
                    string[] terms = GetLine(date.Year, date.Month, Content).Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                    if (terms.Count() != ColumnCount)throw new System.Exception("Unexpected number of columns in " + Parameters.climateFileName + " date = " + date);
                   
                    if (date.Year < GetYearFrom4charOr9charString(terms[0], FirstLastYear.First) ||
                        date.Year > GetYearFrom4charOr9charString(terms[0], FirstLastYear.Last) ||
                         int.Parse(terms[1]) != date.Month)
                    {
                        throw new System.Exception("Missing month " + date + " in climate file");
                    }

                    try
                    {
                        tmax[ecoregion,date] = CheckInputValue(float.Parse(terms[ColumnNumbers.TMax]), -80, 80, "TMax");
                        tmin[ecoregion, date] = CheckInputValue(float.Parse(terms[ColumnNumbers.TMin]), -80, tmax[ecoregion, date], "TMin");
                        float co2 = CheckInputValue(float.Parse(terms[ColumnNumbers.CO2]), 0, float.MaxValue, "CO2");
                        par0[ecoregion, date] = CheckInputValue(float.Parse(terms[ColumnNumbers.PAR0]), 0, float.MaxValue, "PAR0");
                        prec[ecoregion, date] = CheckInputValue(float.Parse(terms[ColumnNumbers.Prec]), 0, float.MaxValue, "PREC");


                        float hr = Calculate_hr(date.DayOfYear, Parameters.Latitude);
                        float daylength = Calculate_DayLength(hr);
                        float nightlength = Calculate_NightLength(hr);
                        int dayspan = Calculate_DaySpan(date.Month);
                        tave[ecoregion, date] = (float)0.5 * (tmin[ecoregion, date] + tmax[ecoregion, date]);
                        tday[ecoregion, date] = (float)0.5 * (tmax[ecoregion, date] + tave[ecoregion, date]);
                        vpd[ecoregion, date] = Calculate_VPD(Tday[ecoregion, date], TMin[ecoregion,date]);

                        float snowfraction  = CumputeSnowFraction(tave[ecoregion, date]);
                        newsnow[ecoregion, date] = snowfraction * prec[ecoregion, date];//mm
                        maxmonthlysnowmelt[ecoregion, date] = 0.15f * Math.Max(1, Tave[ecoregion, date]) * dayspan;
                        PrecIn[ecoregion,date] = (1 - snowfraction) * prec[ecoregion, date];

                        foreach (ISpecies spc in PlugIn.ModelCore.Species)
                        {

                            float dGDD = Math.Max(0, (Tave[ecoregion, date] - Parameters.PsnTMin[spc]) * dayspan);
                            if (date.Month == 1) gdd[ecoregion, spc, date] = dGDD;
                            else
                            {
                                DateTime last_month = date.AddMonths(-1); 
                                gdd[ecoregion, spc, date] = gdd[ecoregion, spc, last_month] + dGDD;
                            }

                            float dHDD = Math.Max(0, (Parameters.PsnTOpt[spc] - tmin[ecoregion, date]) * dayspan);
                            if (date.Month <= 8) hdd[ecoregion, spc, date] = 0;
                            else  
                            {
                                DateTime last_month = date.AddMonths(-1);
                                hdd[ecoregion, spc, date] = hdd[ecoregion, spc, last_month] + dHDD;
                            }

                            if (StaticVariables.GDD[ecoregion, spc, date] < Parameters.GDDFolSt[spc] || StaticVariables.HDD[ecoregion, spc, date] > Parameters.CDDFolEnd[spc]) 
                            {
                                Leaf_On[ecoregion,spc,date] = false;
                            }
                            else //if (StaticVariables.HDD[ecoregion, spc, date] > Parameters.CDDFolEnd[spc] && Leaf_On[ecoregion, spc, date] == true)
                            {
                                Leaf_On[ecoregion, spc, date] = true;
                            }

                            //ftemppsn[date, spc] = ParabolicPsnTempResponse(tday[ecoregion, date], Parameters.PsnTOpt[spc], Parameters.PsnTMin[spc]);
                            ftemppsn[ecoregion, spc, date] = LinearPsnTempResponse(tday[ecoregion, date], Parameters.PsnTOpt[spc], Parameters.PsnTMin[spc]);

                            dvpd[ecoregion, spc, date] = Math.Max(0, 1 - Parameters.DVPD1[spc] * (float)Math.Pow(vpd[ecoregion, date], Parameters.DVPD2[spc]));

                            float cicaRatio = (-0.075f * Parameters.FolN[spc]) + 0.875f;
                            float ci350 = 350 * cicaRatio;
                            float Arel350 = 1.22f * ((ci350 - 68) / (ci350 + 136));

                            float ciElev = co2 * cicaRatio;
                            float ArelElev = 1.22f * ((ciElev - 68) / (ciElev + 136));
                            float delamax   = 1 + ((ArelElev - Arel350) / Arel350);


                            // CO2 effect on photosynthesis
                            // Calculate CO2 effect on conductance and set slope and intercept for A-gs relationship
                            float Delgs = delamax / ((co2 - co2 * cicaRatio) / (350.0f - ci350));

                             //wue[ecoregion, spc, date] = (Parameters.WUEcnst[spc] / vpd[ecoregion, date]) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance
                            float wue = (Parameters.WUEcnst[spc] / vpd[ecoregion, date]) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance
                            wue_co2_corr[ecoregion, spc, date] = wue / delamax;

                            // NETPSN
                            float amax = delamax * (Parameters.AmaxA[spc] + Parameters.AmaxB[spc] * Parameters.FolN[spc]);

                            //Reference net Psn (lab conditions)
                            RefNetPsn[ecoregion, spc, date] = dayspan * (amax * dvpd[ecoregion , spc,date] * daylength * Constants.MC) / Constants.billion;


                            fTempRespDay[ecoregion, spc, date] = ((float)Math.Pow(Parameters.Q10[spc], (Tday[ecoregion, date] - Parameters.PsnTOpt[spc]) / 10));
                            float fTempRespNight  = ((float)Math.Pow(Parameters.Q10[spc], (TMin[ecoregion, date] - Parameters.PsnTOpt[spc]) / 10));

                            fTempResp[ecoregion, spc, date] = (float)Math.Min(1.0, (fTempRespDay[ecoregion, spc, date] * daylength + fTempRespNight * nightlength) / ((float)daylength + (float)nightlength));

                            //  gC/day
                            RefResp[ecoregion, spc, date] = Parameters.BFolResp[spc] * dayspan * (amax * daylength * Constants.MC) / Constants.billion;
                        }
                        date = date.AddMonths(1);
                    }
                    catch (System.Exception e)
                    {
                        throw new System.Exception("Error in climate " + date.ToString("MM/yyyy") + " " + e.Message);
                    }

                }
                SetEstablishmentMonths( Parameters, ecoregion);
            }
            PlugIn.ModelCore.UI.WriteLine("Ready initializing static data");
        }
         
        private static void SetEstablishmentMonths(IInputParameters Parameters, IEcoregion ecoregion)
        {
            DateTime date = DateRange[0];

            int wettestmonth=-1;

            Library.Biomass.Species.AuxParm<int> firstactivemonthspc = null;
            Library.Biomass.Species.AuxParm<int> Firstpossibleestmonth = new Library.Biomass.Species.AuxParm<int>(PlugIn.modelCore.Species);
            Library.Biomass.Species.AuxParm<int> Lastpossibleestmonth = new Library.Biomass.Species.AuxParm<int>(PlugIn.modelCore.Species);
            
            while (date.CompareTo(DateRange[1]) < 0)
            {
                if (date.Month == 1 || wettestmonth < 0) wettestmonth = GetWettestMonth(ecoregion, date.Year, 10);
                if (date.Month == 1 || firstactivemonthspc == null)
                {
                    firstactivemonthspc = new Library.Biomass.Species.AuxParm<int>(PlugIn.modelCore.Species);
                    foreach (ISpecies spc in PlugIn.ModelCore.Species)
                    {
                        firstactivemonthspc[spc] = GetFirstActiveMonth(spc, Parameters, date, ecoregion, 10);
                        Firstpossibleestmonth[spc] = Math.Min(wettestmonth - 1, firstactivemonthspc[spc]);
                        Lastpossibleestmonth[spc] = Math.Max(wettestmonth + 1, firstactivemonthspc[spc] + 3);
                    }
                }
                
                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    if (date.Month >= Firstpossibleestmonth[spc] && date.Month <= Lastpossibleestmonth[spc])
                    {
                        possibleestmonth[ecoregion, spc, date] = true;
                    }

                }
                date = date.AddMonths(1);
            }
        }
         
        static int GetWettestMonth(IEcoregion ecoregion, int year, int timespan)
        {
            // Get the wettest month from a timespan date - 0.5*timespan yr to date + 0.5 * timespan yr
            int HalfTimeSpan = (int)Math.Round(0.5 * timespan, 0);

            DateTime date = new DateTime(year, 1, 15);
            DateTime DateCounter =  date.AddYears(-HalfTimeSpan);

            // Sum precipitation per month over the period timespan date - 0.5*timespan yr to date + 0.5 * timespan yr
            float[] P = new float[13];
            while (DateCounter <  date.AddYears(HalfTimeSpan))
            {
                if (DateCounter.Year >= DateRange[0].Year && DateCounter.Year <= DateRange[1].Year)
                {
                    P[DateCounter.Month] += prec[ecoregion, DateCounter];
                }
                DateCounter = DateCounter.AddMonths(1);
            }
            int MonthMaxPrec = 0;
            float MaxPrec = float.MinValue;

            for (int Month = 1; Month < P.Count(); Month++)
            {
                if (P[Month] > MaxPrec)
                {
                    MaxPrec = P[Month];
                    MonthMaxPrec = Month;
                }
            }
            return MonthMaxPrec;
        }
         
        static int GetFirstActiveMonth(ISpecies spc, IInputParameters Parameters, DateTime date, IEcoregion ecoregion, int timespan)
        {
            int HalfTimeSpan = (int)Math.Round(0.5 * timespan, 0);
            DateTime DateCounter = date.AddYears(-5);
            int FirstActiveMonth = -1;

            bool FoundFirstActiveMonth= false;
            int SumFirstActiveMonth=0;
            int c=0;
            while (DateCounter.CompareTo(date.AddYears(5)) < 1)
            {
                if (DateCounter.Month == 1)
                {
                    FoundFirstActiveMonth = false;
                }
                if (DateCounter.Year >= DateRange[0].Year && DateCounter.Year < DateRange[1].Year)
                {
                    if (GDD[ecoregion, spc, DateCounter] > Parameters.GDDFolSt[spc] && FoundFirstActiveMonth == false)
                    {
                        FoundFirstActiveMonth = true;
                        SumFirstActiveMonth += DateCounter.Month;
                        c++;
                    }
                }
                  
                DateCounter = DateCounter.AddMonths(1);
            }
            // Take the average over the designated time span
            FirstActiveMonth = (int)SumFirstActiveMonth / c;

            return FirstActiveMonth;
        }
         
        public static T CheckInputValue<T>(T value, T min, T max, string label)
        {
            if (double.Parse(value.ToString()) > double.Parse(max.ToString()))
            {
                throw new System.Exception("Input value" + value.ToString() + "exceeds maximum " + label);
            }
            if (double.Parse(value.ToString()) < double.Parse(min.ToString()))
            {
                throw new System.Exception("Input value " + value.ToString() + " below minimum " + label);
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

                    string[] terms = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                    FirstYear = GetYearFrom4charOr9charString(terms[0], FirstLastYear.First);
                    LastYear = GetYearFrom4charOr9charString(terms[0], FirstLastYear.Last);
                    month = int.Parse(terms[ColumnNumbers.Month]);
                    if (month == Month && Year >= FirstYear && Year <= LastYear)
                    {
                        return line;
                    }
                }

                for (lastline = 1; lastline < Content.Count(); lastline++)
                {
                    line = Content[lastline];
                    string[] terms = line.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);

                    FirstYear = GetYearFrom4charOr9charString(terms[0], FirstLastYear.First);
                    LastYear = GetYearFrom4charOr9charString(terms[0], FirstLastYear.Last);
                    month = int.Parse(terms[ColumnNumbers.Month]);

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
                if (Landis.Library.Biomass.Util.Contains(hdr,Label, StringComparison.OrdinalIgnoreCase))
                {
                    return h;
                }
                if (Landis.Library.Biomass.Util.Contains(hdr, Label, StringComparison.OrdinalIgnoreCase))
                {
                    return h;
                }
            }
            throw new System.Exception("Cannot find header " + Label + " in " + climateFileName);
        }
        private static DateTime[] GetFirstAndLastYearOnFile(string[] Content)
        {
            DateTime[] FirstLastDate = new DateTime[2];
            try
            {
                string[] terms = Content[1].Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                int FirstYearOnFile = GetYearFrom4charOr9charString(terms[0], FirstLastYear.First);
                FirstLastDate[0] = new DateTime(FirstYearOnFile,1,15);

                terms = Content[Content.Count() - 1].Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
                int LastYearOnFile = GetYearFrom4charOr9charString(terms[ColumnNumbers.Year], FirstLastYear.Last);
                FirstLastDate[1] = new DateTime(LastYearOnFile, 1, 15);
            }
            catch
            {
                throw new System.Exception("Irregular table format in " + climateFileName);
            }
            return FirstLastDate;
        }
        private static void SetClimateFileHeaders(string HeaderLine)
        {
            string[] Headers = HeaderLine.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            ColumnCount = Headers.Count();
            ColumnNumbers.Year = GetColNr(Headers, "Year");
            ColumnNumbers.Month = GetColNr(Headers, "Month");
            ColumnNumbers.TMax = GetColNr(Headers, "TMax");
            ColumnNumbers.TMin = GetColNr(Headers, "TMin");
            ColumnNumbers.CO2 = GetColNr(Headers, "CO2");
            ColumnNumbers.PAR0 = GetColNr(Headers, "PAR");
            ColumnNumbers.Prec = GetColNr(Headers, "Prec");
        }
        private static string[] ReadContent(string climateFileName)
        {
            string[] Content = System.IO.File.ReadAllLines(climateFileName);
            for (int line = 0; line < Content.Count(); line++)
            {
                Content[line] = Content[line].Replace("\t\t", "\t");
            }
            return Content;
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
