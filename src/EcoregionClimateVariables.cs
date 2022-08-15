using Landis.Library.PnETCohorts;
using System;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class EcoregionClimateVariables : IEcoregionClimateVariables
    {
         
        private DateTime _date;
        private IObservedClimate obs_clim;
        private float _vpd;
        private float _dayspan;
        private float _tave;
        private float _tday;
        private float _daylength;
         

        public float VPD
        {
            get
            {
                return _vpd;
            }
        }
        
       
        public byte Month 
        { 
            get 
            { 
                return (byte)_date.Month; 
            } 
        }
        public float Tday {
            get 
            {
                return _tday;
            }
        }
        public float Prec
        {
            get
            {
                return obs_clim.Prec;
            }
        }
        public float O3
        {
            get
            {
                return obs_clim.O3;
            }

        }
        public float CO2 {
            get {
                return obs_clim.CO2;
            }
        
        }
        public float PAR0 {
            get 
            {
                return obs_clim.PAR0;
            }
        }
        public DateTime Date {
            get {
                return _date;
            }
        }

        // Number of days in the month
        public float DaySpan
        {
            get
            {
                return _dayspan;
            }
        }
        public float Year 
        { 
            get 
            { 
                return _date.Year + 1F / 12F * (_date.Month - 1); 
            } 
        }
        public float Tave
        {
            get
            {
                return _tave;
            }
        }
        
        public float Tmin 
        {
            get
            {
                return obs_clim.Tmin;
            }
        }
        public float Tmax
        {
            get
            {
                return obs_clim.Tmax;
            }
        }
        public float Daylength
        {
            get
            {
                return _daylength;
            }
        }
        

       

        #region static computation functions
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
            return a * (float)Math.Exp(b * T / (T + c));
        }

        public static float Calculate_VPD(float Tday, float TMin)
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

        

        public static float LinearPsnTempResponse(float tday, float PsnTOpt, float PsnTMin)
        {
            if (tday < PsnTMin) return 0;
            else if (tday > PsnTOpt) return 1;

            else return (tday - PsnTMin) / (PsnTOpt - PsnTMin);
        }

        public static float CurvelinearPsnTempResponse(float tday, float PsnTOpt, float PsnTMin)
        {
            // Copied from Psn_Resp_Calculations.xlsx[FTempPsn_Mod]
            //=IF(D2>AA$2,1,MAX(0,(($AA$3-D2)*(D2-$AA$1))/((($AA$3-$AA$1)/2)^2)))
            //=IF(tday>PsnTOpt,1,MAX(0,((PsnTMax-tday)*(tday-PsnTMin))/(((PsnTMax-PsnTMin)/2)^2)))
            float PsnTMax = PsnTOpt + (PsnTOpt - PsnTMin);
            if (tday < PsnTMin) return 0;
            else if (tday > PsnTOpt) return 1;

            else return ((PsnTMax-tday)*(tday-PsnTMin))/(float)Math.Pow(((PsnTMax-PsnTMin)/2),2);
        }

        public static float DTempResponse(float tday, float PsnTOpt, float PsnTMin, float PsnTMax)
        {
            // Copied from Psn_Resp_Calculations.xlsx[DTemp]
            //=MAX(0,(($Y$3-D2)*(D2-$Y$1))/((($Y$3-$Y$1)/2)^2))
            //=MAX(0,((PsnTMax-tday)*(tday-PsnTMin))/(((PsnTMax-PsnTMin)/2)^2))

            if (tday < PsnTMin)
                return 0;
            else if (tday > PsnTMax)
                return 0;
            else
            {
                if (tday <= PsnTOpt)
                {
                    float PsnTMaxestimate = PsnTOpt + (PsnTOpt - PsnTMin);
                    return (float)Math.Max(0.0, ((PsnTMaxestimate - tday) * (tday - PsnTMin)) / (float)Math.Pow(((PsnTMaxestimate - PsnTMin) / 2), 2));
                }
                else
                {
                    float PsnTMinestimate = PsnTOpt + (PsnTOpt - PsnTMax);
                    return (float)Math.Max(0.0, ((PsnTMax - tday) * (tday - PsnTMinestimate)) / (float)Math.Pow(((PsnTMax - PsnTMinestimate) / 2), 2));
                }
            }

        }

        public static float Calculate_NightLength(float hr)
        {
            return 60 * 60 * (24 - hr);
        }

        public static float Calculate_DayLength(float hr)
        {
            return 60 * 60 * hr;
        }

        public static float Calculate_hr(int DOY, double Latitude)
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

        #endregion

        private Dictionary<string, SpeciesPnETVariables> speciesVariables;

        public SpeciesPnETVariables this[string species]
        {
            get
            {
                return speciesVariables[species];
            }
        }

        public EcoregionClimateVariables(IObservedClimate climate_dataset, DateTime Date, bool Wythers, bool DTemp, List<ISpeciesPnET> Species, float Latitude)
        {
            
            this._date = Date;
            this.obs_clim = climate_dataset;

            speciesVariables = new Dictionary<string, SpeciesPnETVariables>();

            
            _tave = (float)0.5 * (climate_dataset.Tmin + climate_dataset.Tmax);

            _dayspan = EcoregionClimateVariables.Calculate_DaySpan(Date.Month);

            float hr = Calculate_hr(Date.DayOfYear, Latitude);
            _daylength = Calculate_DayLength(hr);
            float nightlength = Calculate_NightLength(hr);

            _tday = (float)0.5 * (climate_dataset.Tmax + _tave);
            _vpd = EcoregionClimateVariables.Calculate_VPD(Tday, climate_dataset.Tmin);
        }
    }
}
