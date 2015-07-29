using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class CClimDay
    {
        private DateTime currentdate;
        public DateTime CurrentDate
        {
            get { return currentdate; }
            private set
            {
                currentdate = value;
            }
        }
        public bool TreesAreActive;
        private void SetTreesAreActive()
        {
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                if (isactive[species] == true)
                {
                    TreesAreActive = true;
                    break;
                }
            }
        }

        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> dtemp;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> dvpd;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> netpsn_pot;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> fResp;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> resp_pot;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> grossPsn_pot;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> wue;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> delamax;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> wue_co2_corr;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> resp_folbase;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> dt_day;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> dt_night;
        private Landis.Extension.Succession.Biomass.Species.AuxParm<bool> isactive; // photosynthesis possible in view of temp
        private Landis.Extension.Succession.Biomass.Species.AuxParm<float> gdd;
        
        float cicaRatio;
        float ci350;
        float Arel350;
        float ciElev;
        float ArelElev;
        float par0;
        float vpd;

        public float VPD
        {
            get
            {
                return vpd;
            }
        }

        float tmin;
        float tmax;
        float tday;
        float tave;
        float prec;
        float co2;
        int dayspan;
        float daylength;
        float nightlength;
        public float DayLength
        {
            get
            {
                return daylength;
            }
        }
        public float NightLength
        {
            get
            {
                return nightlength;
            }
        }

        public Landis.Extension.Succession.Biomass.Species.AuxParm<bool> IsActive
        {
            get
            {
                return isactive;
            }
            private set
            {
                isactive = value;
            }
        }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> DelAmax
        {
            get
            {
                return delamax;
            }
            private set
            {
                delamax = value;
            }
        }
         
        public float PAR0 { get { return par0; } }
        public float Tday { get { return tday; } }
        public float Tave { get { return tave; } }
        public float Prec { get { return prec; } }
        public int DaySpan { get { return dayspan; } }
         
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> WUE { get { return wue; } }           
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> Fresp { get { return fResp; } }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> DTemp { get { return dtemp; } }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> DVPD { get { return dvpd; } }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> NetPsn_Pot { get { return netpsn_pot; } }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> GrossPsn_pot { get { return grossPsn_pot; } } // micro Mol/m2 leaf/s (order of magn 100-150)
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> WUE_CO2_corr { get { return wue_co2_corr; } }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> Resp_FolBase { get { return resp_folbase; } }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> GDD { get { return gdd; } }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> Dt_day { get { return dt_day; } }
        public Landis.Extension.Succession.Biomass.Species.AuxParm<float> Dt_night { get { return dt_night; } }
         
        public CClimDay(int Latitude, DateTime CurrentDate ,float PAR0, float TMin, float TMax, float Prec, float CO2)
        {
            try
            {
                this.CurrentDate = CurrentDate;
                par0 = PAR0;
                tmin = TMin;
                tmax = TMax;
                prec = Prec;

                if (prec < 0) throw new System.Exception("Precipitation = " + prec + "\t" + CurrentDate);

                co2 = CO2;

                float hr = Calculate_hr(CurrentDate.DayOfYear, Latitude);
                daylength = Calculate_DayLength(hr);
                nightlength = Calculate_NightLength(hr);
                dayspan = Calculate_DaySpan(CurrentDate.Month);
                tave = Calculate_Tave(TMin, TMax);
                tday = Calculate_TDay(TMax, Tave);
                vpd = Calculate_VPD(Tday, TMin);

                dtemp = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                dvpd = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                netpsn_pot = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                wue = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                delamax = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                fResp = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                resp_pot = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                wue_co2_corr = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                grossPsn_pot = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                resp_folbase = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                IsActive = new Biomass.Species.AuxParm<bool>(PlugIn.ModelCore.Species);
                dt_night = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                dt_day = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                gdd = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in PlugIn.ModelCore.Species) gdd[spc] = 0;

                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    float dGDD = Math.Max(0, (Tave - SpeciesData.PsnTMin[spc]) * DaySpan);
                    if (CurrentDate.Month == 1) gdd[spc] = dGDD;
                    else
                    {
                        DateTime last_month = new DateTime(CurrentDate.Year, CurrentDate.Month - 1, 15);
                        gdd[spc] = ClimateData.Data[last_month].GDD[spc] + dGDD;
                    }

                    if (tday < SpeciesData.PsnTMin[spc]) IsActive[spc] = false;
                    else IsActive[spc] = true;

                    float psntmax = SpeciesData.PsnTOpt[spc] + (SpeciesData.PsnTOpt[spc] - SpeciesData.PsnTMin[spc]);    // assumed symmetrical
                    float PsnTRange = psntmax - SpeciesData.PsnTMin[spc];
                    float PsnTRangeHalfSquare = (float)Math.Pow((PsnTRange) / 2.0, 2);

                    dtemp[spc] = (float)Math.Max(((psntmax - tday) * (tday - SpeciesData.PsnTMin[spc])) / (PsnTRangeHalfSquare), 0);
                    dvpd[spc] = Math.Max(0, 1 - SpeciesData.DVPD1[spc] * (float)Math.Pow(vpd, SpeciesData.DVPD2[spc]));


                    cicaRatio = (-0.075f * SpeciesData.FolNCon[spc]) + 0.875f;
                    ci350 = 350 * cicaRatio;
                    Arel350 = 1.22f * ((ci350 - 68) / (ci350 + 136));

                    ciElev = co2 * cicaRatio;
                    ArelElev = 1.22f * ((ciElev - 68) / (ciElev + 136));
                    DelAmax[spc] = 1 + ((ArelElev - Arel350) / Arel350);


                    // CO2 effect on photosynthesis
                    // Calculate CO2 effect on conductance and set slope and intercept for A-gs relationship
                    //List<string> file = new List<string>();
                    //for (int c = 0; c <= 10000; c++)
                    //{
                    //    co2 = 10 * c;
                    //    float dlgs = DelAmax[spc] / ((co2 - co2 * cicaRatio) / (350.0f - ci350));
                    //    file.Add(c + "\t" + dlgs);
                    //}
                    //System.IO.File.WriteAllLines("output/delgs.txt", file.ToArray());
                    float Delgs = DelAmax[spc] / ((co2 - co2 * cicaRatio) / (350.0f - ci350));

                    wue[spc] = (SpeciesData.WUEConst[spc] / vpd) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance

                    wue_co2_corr[spc] = wue[spc] / DelAmax[spc];


                    //wue_co2_corr[spc] = SpeciesData.WUEConst[spc];

                    //float wue_co2_cor1 = wue_co2_corr[spc];
                    //float wue_co2_cor2 = wue_co2_corr[spc] = wue[spc] / DelAmax[spc];

                    /*
                    Delgs = DelAmax / ((site.CaMo[mo] - CiElev) / (350.0 - Ci350));
                    DWUE = 1.0 + (1 - Delgs);
                    gsSlope = (-1.1309 * DelAmax) + 1.9762;   // used to determine ozone uptake
                    gsInt = (0.4656 * DelAmax) - 0.9701;
                    */

                    netpsn_pot[spc] = delamax[spc] * (SpeciesData.AmaxA[spc] + SpeciesData.AmaxB[spc] * SpeciesData.FolNCon[spc]);  // nmole CO2/g Fol.sec

                    dt_day[spc] = (float)Math.Pow(SpeciesData.RespQ10[spc], (tday - SpeciesData.PsnTOpt[spc]) / 10);
                    dt_night[spc] = (float)Math.Pow(SpeciesData.RespQ10[spc], (tmin - SpeciesData.PsnTOpt[spc]) / 10);


                    fResp[spc] = (dt_day[spc] * daylength + dt_night[spc] * nightlength) / (float)daylength;
                    resp_pot[spc] = SpeciesData.BaseFolRespFrac[spc] * netpsn_pot[spc]; //

                    float HoursPerDay = 24 * daylength / (daylength + nightlength);
                    grossPsn_pot[spc] = Constants.DaySpan * Constants.SecondsPerHour * HoursPerDay * Constants.MC * Constants.ngPerG * (netpsn_pot[spc] + resp_pot[spc]); // gr/gr/mo
                }

                SetTreesAreActive();
            }
            catch
            {
                throw new System.Exception("Cannot find climate date for (year,month) " + CurrentDate.Year.ToString() + " " + CurrentDate.Month.ToString());
            }
        }
        
        static float Calculate_TDay(float TMax, float Tave)
        {
            return (float)0.5 * (TMax + Tave);
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
        static float Calculate_Tave(float TMin, float TMax)
        {
            return (float)0.5 * (TMin + TMax);
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
        
        private static float Calculate_VPD(float Tday, float TMin)
        {
            float emean;
            //float delta;
             
            float es = 0.61078f * (float)Math.Exp(17.26939f * Tday / (Tday + 237.3f));
            //delta = 4098.0f * es / ((Tday + 237.3f) * (Tday + 237.3f));
            if (Tday < 0)
            {
                es = 0.61078f * (float)Math.Exp(21.87456f * Tday / (Tday + 265.5f));
                //delta = 5808.0f * es / ((Tday + 265.5f) * (Tday + 265.5f));
            }

            emean = 0.61078f * (float)Math.Exp(17.26939f * TMin / (TMin + 237.3f));
            if (TMin < 0) emean = 0.61078f * (float)Math.Exp(21.87456f * TMin / (TMin + 265.5f));

            return es - emean;
        }
    }
}
