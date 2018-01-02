
namespace Landis.Extension.Succession.BiomassPnET 
{
    public class Constants 
    {
        public static float MC = 12;                        // Molecular weight of C
        public static float MCO2 = 44;                      // Molecular weight of CO2
        public static int SecondsPerHour = 60 * 60;         // Seconds per hour
        public static int billion = 1000000000;             // Bilion 
        public static  float MCO2_MC = MCO2 / MC;           // Molecular weight of CO2 relative to C 
            
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
