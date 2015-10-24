using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public class Constants 
    {
        public static float MC = 12;    // Molecular weight of C
        public static float MCO2 = 44; // Molecular weight of CO2
        public static int SecondsPerHour = 60 * 60;
        public static int billion = 1000000000;
        public static float million  = 1000000;
        public static  float MCO2_MC = MCO2 / MC;
        public static float log2 = (float)Math.Log(2);
        public static int sec_per_day = 24* SecondsPerHour;
        public static float JoulesPerMJ = 1000000;
        public static int sec_per_month = (int)(30.5F * (float)sec_per_day);
        public static int mm_per_m = 1000;
        public static int days_per_month = 30;
        public static int NrOfMonths = Enum.GetValues(typeof(Months)).GetLength(0);
        public static System.Array AllMonths = Enum.GetValues(typeof(Months));
        public static float MillionOverTwelve = million / 12F;
     
        public enum Months
        {
            January = 1,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }

        
         
    }
}
