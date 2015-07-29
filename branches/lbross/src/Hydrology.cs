using Landis.Core;
using System;
using Landis.SpatialModeling;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{  
    public class Hydrology  
    {
        private static float snowmelt;
        private static float WaterIn;
        
        
        static IPressureHeadTable Pressureheadtable;
        public static Landis.Library.Parameters.Ecoregions.AuxParm<float> FieldCap { get; private set; }
        public static Landis.Library.Parameters.Ecoregions.AuxParm<float> WiltPnt;
        public static Landis.Library.Parameters.Ecoregions.AuxParm<float> Porosity;
        public static Landis.Library.Parameters.Ecoregions.AuxParm<string> SoilType;

        public static float RunOff { get; private set; }
        public static float Leakage { get; private set; }
        public static float PET { get; private set; }
        public static float DeliveryPotential { get; private set; }
        public static float Interception { get; private set; }
        
        public static float Evaporation { get; private set; }
        
        static string soiltype;

        private static ActiveSite site;
        private static IEcoregion ecoregion;

        public static void Initialize()
        {

            Parameter<string> PressureHeadCalculationMethod = null;
            if (PlugIn.TryGetParameter(Names.PressureHeadCalculationMethod, out PressureHeadCalculationMethod))
            {
                Parameter<string> p = PlugIn.GetParameter(Names.PressureHeadCalculationMethod);

                if (p.Value == Names.VanGenuchten) Pressureheadtable = new PressureHeadVanGenuchten();
                else if (PlugIn.GetParameter(Names.PressureHeadCalculationMethod).Value == Names.SaxtonAndRawls) Pressureheadtable = new PressureHeadSaxton_Rawls();
                else
                {
                    string msg = "Unknown presciption for calculating pressurehead, value for " + Names.PressureHeadCalculationMethod + " can be "+ Names.VanGenuchten+" or " + Names.SaxtonAndRawls;
                    throw new System.Exception(msg);
                }
            }
            else
            {
                string msg = "Missing presciption for calculating pressurehead, expected keyword " + Names.PressureHeadCalculationMethod + " in " + PlugIn.GetParameter(Names.PnETGenericParameters).Value + " or in " + PlugIn.GetParameter(Names.ExtensionName).Value; 
                throw new System.Exception(msg);
            }
           
            WiltPnt = new Library.Parameters.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            FieldCap = new Library.Parameters.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);
            Porosity = new Library.Parameters.Ecoregions.AuxParm<float>(PlugIn.ModelCore.Ecoregions);

            SoilType = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)PlugIn.GetParameter(Names.SoilType);

            PlugIn.ModelCore.UI.WriteLine("Eco\tSoilt\tWiltPnt\tFieldCap(mm)\tFC-WP\tPorosity");
            foreach (IEcoregion eco in PlugIn.ModelCore.Ecoregions) if (eco.Active)
            {
                // takes PH (MPa) 
                // Calculates water content (m3H2O/m3 SOIL)

                // Water content at field capacity (calculated as an output variable)
                //  −33 kPa (or −0.33 bar)  
                // mH2O value =  kPa value x 0.101972
                FieldCap[eco] = (float)Hydrology.Pressureheadtable.CalculateWaterContent((ushort)3.36, SoilType[eco]) * eco.RootingDepth();

                WiltPnt[eco] = (float)Hydrology.Pressureheadtable.CalculateWaterContent((ushort)150, SoilType[eco]) * eco.RootingDepth();

                Porosity[eco] = (float)Hydrology.Pressureheadtable.Porosity(eco.RootingDepth(), SoilType[eco]);

                float f = FieldCap[eco] - WiltPnt[eco];
                PlugIn.ModelCore.UI.WriteLine(eco.Name + "\t" + SoilType[eco] + "\t" + WiltPnt[eco] + "\t" + FieldCap[eco] + "\t" + f + "\t" + Porosity[eco] );
            }
        }
        public Hydrology(ActiveSite Site)
        {
            site = Site;
            ecoregion = PlugIn.ModelCore.Ecoregion[site];
            soiltype = SoilType[ecoregion];
          
        }

       
        double Calculate_PotentialEvapotranspiration(double _Rads, double _Tair, double Altitude = 0)
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


        public static int PressureHead(IEcoregion ecoregion, ushort Water)
        {
            return Pressureheadtable[ecoregion, Water];
        }
        int PressureHead(ushort Water)
        {
            return Pressureheadtable[ecoregion, Water];
        }
        public static void SubtractTranspiration(ushort Cohorttranspiration, ref ushort Water)
        {
            if (Cohorttranspiration > Water)
            {
                
                Water = 0;
            }
            else
            {
                Water -= Cohorttranspiration;
                
            }
        }
        public float SubtractEvaporation(ushort SubCanopyRadiation, float Transpiration, float Temp, ref ushort Water)
        {
            PET = (float)Calculate_PotentialEvapotranspiration(SubCanopyRadiation, Temp);

            DeliveryPotential = SubCanopyLayer.GetFWater(0, 0, 150, PressureHead(Water));

            Evaporation = Math.Min(Water, Math.Max(0, DeliveryPotential * PET - Transpiration));

            Water -= (ushort)Evaporation;

            return Evaporation;
        }
        public float SubtractInterception(EcoregionDateData data, float LAIsum, ref ushort Water)
        {
            Interception = data.Precin * Calculate_Interception_Fraction(LAIsum);

            Water -= (ushort)Interception;

            return Interception;
            
        }
        /// <summary>
        /// Fraction of incoming precip lost
        /// </summary>
        /// <param name="LAI"></param>
        /// <returns></returns>
        float Calculate_Interception_Fraction(float LAIsum)
        {
            return (float) (1- Math.Exp(-1* ecoregion.PrecIntConst() * LAIsum));
        }
        public void UpdateSiteHydrology(EcoregionDateData data, ref ushort Water, ref ushort SnowPack)
        {
            Water -= (ushort)Math.Max(Water - FieldCap[ecoregion], 0);

            snowmelt = Math.Min(SnowPack, data.Maxmonthlysnowmelt);

            SnowPack += (ushort)(data.Newsnow - snowmelt);

            WaterIn = data.Precin + snowmelt;//mm  

            float PrecLoss = WaterIn * ecoregion.PrecLossFrac();

            float infiltration = WaterIn - PrecLoss;

            Water += (ushort)infiltration;
           
            // Leakage 
            Leakage = Math.Max(ecoregion.LeakageFrac() * (Water - FieldCap[ecoregion]), 0);
            Water -= (ushort)Leakage;

            // Instantaneous runoff (excess of porosity)
            RunOff = Math.Max(Water - Porosity[ecoregion] * ecoregion.RootingDepth(), 0);
            Water -= (ushort)RunOff;

            
        }
         
        
    }
 }