using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Core;
using Landis.SpatialModeling;
namespace Landis.Extension.Succession.BiomassPnET
{
    public class DerivedClimate
    {
        public ObservedClimate.DataSet obs_clim;

        public DateTime date;
       
        public bool AnyLeaf_On;  
        public byte Month { get { return (byte)date.Month; } }
        public float Tday;
        public float VPD;
        
        public Landis.Library.Parameters.Ecoregions.AuxParm<float> SnowMelt;
        public Landis.Library.Parameters.Ecoregions.AuxParm<float> SnowPack;
         
        public float Precin;
        private float Maxmonthlysnowmelt;
        public float Year { get { return date.Year + 1F / 12F * (date.Month - 1); } }
        public float gsSlope;
        public float gsInt;

        public Landis.Library.Parameters.Ecoregions.AuxParm<float> PrecInEffective;
        public Landis.Library.Parameters.Species.AuxParm<bool> Leaf_On;
        public Landis.Library.Parameters.Species.AuxParm<float> FTempResp;
        public Landis.Library.Parameters.Species.AuxParm<float> MaintRespFTempResp;
        public Landis.Library.Parameters.Species.AuxParm<float> FTempRespDayRefResp;
        public Landis.Library.Parameters.Species.AuxParm<float> FTempPSN;
        public Landis.Library.Parameters.Species.AuxParm<float> FTempPSNRefNetPsn;
        public Landis.Library.Parameters.Species.AuxParm<float> WUE_CO2_corr;
     
      
        static float[] RespTempResponse(ISpeciesPNET spc, float tday, float tmin, float daylength, float nightlength)
        {
            float[] RespTempResponse = new float[2];


            // day respiration factor
            RespTempResponse[0] = ((float)Math.Pow(spc.Q10, (tday - spc.PsnTOpt) / 10));

            float fTempRespNight = ((float)Math.Pow(spc.Q10, (tmin - spc.PsnTOpt) / 10));

            //weighted day and night respiration factor
            RespTempResponse[1] = (float)Math.Min(1.0, (RespTempResponse[0] * daylength + fTempRespNight * nightlength) / ((float)daylength + (float)nightlength));

            return RespTempResponse;
        }
        
        static float LinearPsnTempResponse(float tday, float PsnTOpt, float PsnTMin)
        {
            if (tday < PsnTMin) return 0;
            else if (tday > PsnTOpt) return 1;

            else return (tday - PsnTMin) / (PsnTOpt - PsnTMin);
        }
        
        private static float CumputeSnowFraction(float Tave)
        {
            if (Tave > 2) return 0;
            else if (Tave < -5) return 1;
            else return (Tave - 2) / -7;
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
            return a * (float)Math.Exp(b * T / (T + c));
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
        
        public DerivedClimate(ObservedClimate.DataSet climate_dataset, DateTime Date, Library.Parameters.Ecoregions.AuxParm<float> snowpack)
        {
            this.date = Date;
             
            this.obs_clim = climate_dataset;

            SnowPack= new Landis.Library.Parameters.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            SnowMelt = new Landis.Library.Parameters.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            PrecInEffective = new Library.Parameters.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            Leaf_On = new Library.Parameters.Species.AuxParm<bool>(PlugIn.ModelCore.Species);
            FTempResp = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            MaintRespFTempResp = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempPSNRefNetPsn = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempRespDayRefResp = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            FTempPSN = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            WUE_CO2_corr = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            
            AnyLeaf_On = false;

            float hr = Calculate_hr(Date.DayOfYear, PlugIn.Latitude);
            float daylength = Calculate_DayLength(hr);
            float nightlength = Calculate_NightLength(hr);

            int dayspan = Calculate_DaySpan(Date.Month);

            float Tave = (float)0.5 * (climate_dataset.Tmin + climate_dataset.Tmax);
            Tday = (float)0.5 * (climate_dataset.Tmax + Tave);
            VPD = Calculate_VPD(Tday, climate_dataset.Tmin);

            float snowfraction = CumputeSnowFraction(Tave);
            float Newsnow = snowfraction * climate_dataset.Prec;//mm
            Maxmonthlysnowmelt = 0.15f * Math.Max(0, Tave) * dayspan;

            Precin = (1 - snowfraction) * climate_dataset.Prec;

            foreach(IEcoregionPNET  ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                PrecInEffective[ecoregion] = Precin * (1 - ecoregion.PrecLossFrac);

                SnowMelt[ecoregion] = Math.Min(SnowPack[ecoregion], Maxmonthlysnowmelt);

                SnowPack[ecoregion] = snowpack[ecoregion] + Newsnow - SnowMelt[ecoregion];
            }

            foreach (ISpeciesPNET spc in PlugIn.Species)
            {
                if (climate_dataset.Tmin > spc.PsnTMin)
                {
                    Leaf_On[spc] = true;
                    AnyLeaf_On = true;
                }
                else
                {
                    Leaf_On[spc] = false;
                }
                
                float DVPD = Math.Max(0, 1 - spc.DVPD1 * (float)Math.Pow(VPD, spc.DVPD2));

                float cicaRatio = (-0.075f * spc.FolN) + 0.875f;
                float ci350 = 350 * cicaRatio;
                float Arel350 = 1.22f * ((ci350 - 68) / (ci350 + 136));

                float ciElev = climate_dataset.CO2 * cicaRatio;
                float ArelElev = 1.22f * ((ciElev - 68) / (ciElev + 136));
                float delamax = 1 + ((ArelElev - Arel350) / Arel350);


                // CO2 effect on photosynthesis
                // Calculate CO2 effect on conductance and set slope and intercept for A-gs relationship
                float Delgs = delamax / ((climate_dataset.CO2 - climate_dataset.CO2 * cicaRatio) / (350.0f - ci350));


                gsSlope = (float)((-1.1309 * delamax) + 1.9762);   // used to determine ozone uptake
                gsInt = (float)((0.4656 * delamax) - 0.9701);

                //wue[ecoregion, spc, date] = (Parameters.WUEcnst[spc] / vpd[ecoregion, date]) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance
                float wue = (spc.WUEcnst / VPD) * (1 + 1 - Delgs);    //DWUE determined from CO2 effects on conductance
                WUE_CO2_corr[spc] = wue / delamax;
               
                // NETPSN
                float amax = delamax * (spc.AmaxA + spc.AmaxB * spc.FolN);

                //Reference net Psn (lab conditions) in gC/timestep
                float RefNetPsn = dayspan * (amax * DVPD * daylength * Constants.MC) / Constants.billion;

                //-------------------FTempPSN (public for output file)
                FTempPSN[spc] = LinearPsnTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin);

                // PSN (g/tstep)
                FTempPSNRefNetPsn[spc] = PlugIn.fIMAX * FTempPSN[spc] * RefNetPsn;

                float[] RespTempResponses = RespTempResponse(spc, Tday, climate_dataset.Tmin, daylength, nightlength);

                // Unitless respiration adjustment: public for output file only
                FTempResp[spc] = RespTempResponses[1];

                MaintRespFTempResp[spc] = spc.MaintResp * FTempResp[spc];
                
                float conversion_factors = PlugIn.fIMAX * dayspan * daylength * Constants.MC / Constants.billion;

                // Respiration gC/timestep (RespTempResponses[0] = day respiration factor)


                bool Wythers = ((Parameter<bool>)PlugIn.GetParameter("Wythers")).Value;

                if (Wythers == true)
                {
                    FTempRespDayRefResp[spc] = (0.14F - 0.002F * Tave) * amax * (3.22F - 0.046F * (float)Math.Pow((0.5F * (Tave + spc.PsnTOpt)), ((Tave - spc.PsnTOpt) / 10))) * conversion_factors;
                }
                else FTempRespDayRefResp[spc] = spc.BFolResp * amax * RespTempResponses[0] * conversion_factors;
            }
        }

        

        
    }
}
