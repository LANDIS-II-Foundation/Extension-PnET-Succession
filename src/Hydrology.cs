using System;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class Hydrology : IHydrology
    {
        private float water;
        private float frozenWaterPct;
        private float frozenDepth;

        // volumetric water (mm/m)
        public float Water
        {
            get
            {
                return water;
            }
        }
        // volumetric water with the frozen root zone
        public float FrozenWaterPct
        {
            get
            {
                return frozenWaterPct;
            }
        }
        public float FrozenDepth
        {
            get
            {
                return frozenDepth;
            }
        }

        private static PressureHeadSaxton_Rawls pressureheadtable;

        public float GetPressureHead(IEcoregionPnET ecoregion)
        {
            return pressureheadtable[ecoregion, (int)Math.Round(water * 100.0)];
        }

        public static float PET;
        public static float DeliveryPotential;
        public static float Evaporation;
        public static float Leakage;
        public static float RunOff;

        public static float SurfaceWater = 0; // Volume of water captured above saturatino on the surface

        // Add mm water 
        public bool AddWater(float addwater)
        {
            water += addwater;

            if (water >= 0) return true;
            else return false;
        }
        // Add mm water to vlumetric water content (mm/m)
        public bool AddWater(float addwater, float activeSoilDepth)
        {
            float adjWater = 0;
            if (activeSoilDepth > 0)
            {
                adjWater = addwater / activeSoilDepth;
                           }
            water += adjWater;
            if (water < 0)
                water = 0;

            if (water >= 0)
                return true;
            else
            {
                int q = 0;
                return false;
            }
        }

        // mm of water per m of active soil
        public Hydrology(float water)
        {
            this.water = water;

        }

        public bool SetFrozenWaterPct (float water)
        {
            this.frozenWaterPct = water;
            if (water >= 0) return true;
            else return false;
        }
        public bool SetFrozenDepth(float depth)
        {
            this.frozenDepth = depth;
            if (depth >= 0) return true;
            else return false;
        }
        public static void Initialize()
        {

            Parameter<string> PressureHeadCalculationMethod = null;
            if (PlugIn.TryGetParameter(Names.PressureHeadCalculationMethod, out PressureHeadCalculationMethod))
            {
                Parameter<string> p = PlugIn.GetParameter(Names.PressureHeadCalculationMethod);

                pressureheadtable = new PressureHeadSaxton_Rawls();
            }
            else
            {
                string msg = "Missing presciption for calculating pressurehead, expected keyword " + Names.PressureHeadCalculationMethod + " in " + PlugIn.GetParameter(Names.PnETGenericParameters).Value + " or in " + PlugIn.GetParameter(Names.ExtensionName).Value; 
                throw new System.Exception(msg);
            }
            
            PlugIn.ModelCore.UI.WriteLine("Eco\tSoilt\tWiltPnt\tFieldCap(mm)\tFC-WP\tPorosity");
            foreach (IEcoregionPnET eco in EcoregionPnET.Ecoregions) if (eco.Active)
            {

                    // Volumetric water content (mm/m) at field capacity
                    //  −33 kPa (or −0.33 bar)        
                    // Convert kPA to mH2o (/9.804139432) = 3.37
                    eco.FieldCap = (float)pressureheadtable.CalculateWaterContent(33, eco.SoilType);

                    // Volumetric water content (mm/m) at wilting point
                    //  −1500 kPa (or −15 bar)  
                    // Convert kPA to mH2o (/9.804139432) = 153.00
                    eco.WiltPnt = (float)pressureheadtable.CalculateWaterContent(1500, eco.SoilType);

                    // Volumetric water content (mm/m) at porosity
                    eco.Porosity = (float)pressureheadtable.Porosity(eco.SoilType);

                float f = eco.FieldCap - eco.WiltPnt;
                PlugIn.ModelCore.UI.WriteLine(eco.Name + "\t" + eco.SoilType + "\t" + eco.WiltPnt + "\t" + eco.FieldCap + "\t" + f + "\t" + eco.Porosity );
            }
        }
         
        static double Calculate_PotentialEvapotranspiration(double _Rads, double _Tair, float _dayLength, double Altitude = 0)
        {
        //================================================================================
        //----  Computes the potential evapotranspiration as the value under minimum
        //----  advection according to Priestley and Taylor (1972) as discussed in
        //----  Brutsaert (1982, p. 217).
        //
        //----  Pierluigi Calanca, 23.06.2006 (PROGRASS)
        //================================================================================
            //double _Rads                  // Solar Radiation (MJ/m2/day)
            //double _Tair                   // Air temperature (°C)     
	
	        //double press = 80.0;			 
            double Lv = 2.5e6;				 // Specific heat of vaporisation (J/kg)
	        double Cpd = 1004;				 // Joules/°C/kg (Specific heat at constant pressure)
	        double eps = 0.622;				 // Mol Mass Water (18)/Mol mass air (28.9)
	        double alphaPT = 1.35;			 // Priestley Taylor constant (parameter)

             int sec_per_day = (int) Math.Round(_dayLength);
            const int JoulesPerMJ = 1000000;
            const int days_per_month = 30;
                        
	        // Atmospheric pressure (unit of vapour pressure kPa, depends on altitude)
	        //http://www.fao.org/docrep/x0490e/x0490e07.htm#TopOfPage
	        double press = 101.3 * Math.Pow(((293 -0.0065 * Altitude)/293),5.26);
           
	        // Psychrometric constant [kPa °C-1]
            double gamE  = Cpd*press/(eps*Lv); 

	        // Angle of the curve [-]
            double delta = (6.112 * Math.Exp(17.67 * _Tair / (_Tair + 243.5))) * 17.67 * 243.5 / Math.Pow((_Tair + 243.5), 2);

            // RADs coming in as micromol(PAR)/m2/s
            double Rad_day = _Rads * sec_per_day;   // umol/m2/day
            double Radn = Math.Max(-15 + 0.6 * Rad_day / JoulesPerMJ, 0); // (MJ/m2/day)
            double RadnMJM2 = Radn * sec_per_day / JoulesPerMJ;  // Radn should have unit MJ/m2
            //double RadnMJM2 = _Rads * sec_per_day / 2.0513; //(MJ/m2/day) http://www.pnet.sr.unh.edu/subpages/radconvert.html

            double PET = 0;
	        if (RadnMJM2 > 0)PET = (alphaPT/Lv) * delta/(delta+gamE) * RadnMJM2 * JoulesPerMJ;
	        else PET= 0.0;

            return PET * days_per_month;  //mm/month
        }

        static float Calculate_PotentialEvapotranspiration_umol(double _Rads, double _Tair, float _dayLength, double Altitude = 0)
        {
            // Caculations as presented in Cabrera et al. 2016 for Stewart & Rouse 1976
            const int days_per_month = 30;
            float PET = 0;

            float Rs_W = (float)(_Rads / 1.919); // convert PAR (umol/m2*s) to Solar irradiance (W/m2) [Jacovides et al. 2004]  
            float Rs = Rs_W * 0.0864F; // convert Rs_W (W/m2) to Rs (MJ/m2*d)
            float Gamma = 0.062F; // kPa/C
            float es = 0.6108F * (float)Math.Pow(10, (7.5 * _Tair) / (237.3 + _Tair)); // water vapor saturation pressure (kPa)
            float S = (4098F * es) / (float)(Math.Pow((_Tair + 237.3), 2)); // slope of curve of water pressure and air temp
            float PETmm = (S / (S + Gamma)) * (0.4755F + 0.3773F * Rs); // Stewart & Rouse 1976 (mm/d)
            float PETMJ = (S / (S + Gamma)) * (1.624F + 0.09265F * Rs); // Stewart & Rouse 1976 (MJ/m2 day)
            PET = PETMJ * 0.408F; // convert MJ/m2 day to mm/day

            return PET * days_per_month;  //mm/month http://www.fao.org/3/x0490e/x0490e0i.htm
        }

        public float CalculateEvaporation(SiteCohorts sitecohorts)
        {

            // permafrost
            float frostFreeSoilDepth = sitecohorts.Ecoregion.RootingDepth - FrozenDepth;
            float frostFreeProp = Math.Min(1.0F, frostFreeSoilDepth / sitecohorts.Ecoregion.RootingDepth);

            // mm/month
            //PET = (float)Calculate_PotentialEvapotranspiration(sitecohorts.SubcanopyPAR, sitecohorts.Ecoregion.Variables.Tday, sitecohorts.Ecoregion.Variables.Daylength);
            PET = (float)Calculate_PotentialEvapotranspiration_umol(sitecohorts.SubcanopyPAR, sitecohorts.Ecoregion.Variables.Tday, sitecohorts.Ecoregion.Variables.Daylength);

            float pressurehead = pressureheadtable[sitecohorts.Ecoregion, (int)Math.Round(Water * 100)];

            // Evaporation begins to decline at 75% of field capacity (Robock et al. 1995)
            // Robock, A., Vinnikov, K. Y., Schlosser, C. A., Speranskaya, N. A., & Xue, Y. (1995). Use of midlatitude soil moisture and meteorological observations to validate soil moisture simulations with biosphere and bucket models. Journal of Climate, 8(1), 15-35.
            float evapCritWater = sitecohorts.Ecoregion.FieldCap * 0.75f;
            float evapCritWaterPH = pressureheadtable[sitecohorts.Ecoregion, (int)Math.Round(evapCritWater * 100.0)];

            DeliveryPotential = Cohort.ComputeFWater(-1, -1, evapCritWaterPH, 153, pressurehead);

            // mm/month
            float AET = Math.Min(DeliveryPotential * PET, Water * sitecohorts.Ecoregion.RootingDepth * frostFreeProp);
            sitecohorts.SetAet(AET, sitecohorts.Ecoregion.Variables.Month);

            // Evaporation cannot remove water below wilting point, evaporation cannot be negative
            // Transpiration is assumed to replace evaporation
            Evaporation = (float)Math.Max(0, Math.Min((Water - sitecohorts.Ecoregion.WiltPnt) * sitecohorts.Ecoregion.RootingDepth * frostFreeProp, Math.Max(0, AET - (double)sitecohorts.Transpiration)));

            return Evaporation; //mm/month
        }
   
         

    }
 }