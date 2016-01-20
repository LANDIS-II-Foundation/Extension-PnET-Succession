using Landis.Core;
using System;
using Landis.SpatialModeling;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{  
    public class Hydrology : IHydrology
    {
        private float water;

        public float Water
        {
            get
            {
                return water;
            }
        }
         
        private static PressureHeadSaxton_Rawls pressureheadtable;

        public float GetPressureHead(IEcoregionPnET ecoregion)
        {
            return pressureheadtable[ecoregion, (int)water];

        }
        
        public static float PET;
        public static float DeliveryPotential;
        public static float Evaporation;
        public static float Leakage;
        public static float RunOff;
        
        public bool AddWater(float addwater)
        {
            water += addwater;

            if(water>= 0)return true;
            else return false;
        }

       
        public Hydrology(ushort water)
        {
            this.water = water;
        
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
                // takes PH (MPa) 
                // Calculates water content (m3H2O/m3 SOIL)

                // Water content at field capacity (calculated as an output variable)
                //  −33 kPa (or −0.33 bar)  
                // mH2O value =  kPa value x 0.101972
                eco.FieldCap = (float)pressureheadtable.CalculateWaterContent((ushort)3.37, eco.SoilType) * eco.RootingDepth;

                eco.WiltPnt = (float)pressureheadtable.CalculateWaterContent((ushort)153, eco.SoilType) * eco.RootingDepth;

                eco.Porosity = (float)pressureheadtable.Porosity(eco.RootingDepth, eco.SoilType);

                float f = eco.FieldCap - eco.WiltPnt;
                PlugIn.ModelCore.UI.WriteLine(eco.Name + "\t" + eco.SoilType + "\t" + eco.WiltPnt + "\t" + eco.FieldCap + "\t" + f + "\t" + eco.Porosity );
            }
        }
         
        static double Calculate_PotentialEvapotranspiration(double _Rads, double _Tair, double Altitude = 0)
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

            const int sec_per_day = 60*60*24;
            const int JoulesPerMJ = 1000000;
            const int days_per_month = 30;
                        
	        // Atmospheric pressure (unit of vapour pressure kPa, depends on altitude)
	        //http://www.fao.org/docrep/x0490e/x0490e07.htm#TopOfPage
	        double press = 101.3 * Math.Pow(((293 -0.0065 * Altitude)/293),5.26);
           
	        // Psychrometric constant [kPa °C-1]
            double gamE  = Cpd*press/(eps*Lv); 

	        // Angle of the curve [-]
            double delta = (6.112 * Math.Exp(17.67 * _Tair / (_Tair + 243.5))) * 17.67 * 243.5 / Math.Pow((_Tair + 243.5), 2);

            // RADs coming in as WAT(PAR)/m2/mo
            double Rad_day = _Rads * sec_per_day;   // J/m2/day
            double Radn = Math.Max(-15 + 0.6 * Rad_day / JoulesPerMJ, 0); // (MJ/m2/day)
            double RadnMJM2 = Radn * sec_per_day / JoulesPerMJ;  // Radn should have unit MJ/m2

            double PET = 0;
	        if (RadnMJM2 > 0)PET = (alphaPT/Lv) * delta/(delta+gamE) * RadnMJM2 * JoulesPerMJ;
	        else PET= 0.0;

            return PET * days_per_month;
        }
      
        public float CalculateEvaporation(SiteCohorts sitecohorts )
        {
            // this.Ecoregion.Variables.Month, Ecoregion, this.subcanopypar, Transpiration, this.Ecoregion.Variables.Tday, ref water,this.SetAet
            PET = (float)Calculate_PotentialEvapotranspiration(sitecohorts.SubcanopyPAR, sitecohorts.Ecoregion.Variables.Tday);

            float pressurehead = pressureheadtable[sitecohorts.Ecoregion, (int)Water];

            DeliveryPotential = Cohort.CumputeFWater(0, 0, 153, pressurehead);

            // Per month
            sitecohorts.SetAet(DeliveryPotential * PET, sitecohorts.Ecoregion.Variables.Month); 

            Evaporation = (float)Math.Min(Water, Math.Max(0, DeliveryPotential * PET - (double)sitecohorts.Transpiration));

            return Evaporation;
        }
   
         

    }
 }