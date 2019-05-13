using Landis.Core;
using System;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    ///   <para>John McNabb: This is a record of monthly climate data based on the climate library.</para>
    /// </summary>
    public class MonthlyClimateRecord
    {
        public MonthlyClimateRecord(IEcoregion ecoregion, DateTime date)
        {
            var month = date.Month - 1; // climate library month is zero-based, while DateTime.Month is one-based

            O3 = ClimateRegionData.AnnualWeather[ecoregion].MonthlyOzone[month];
            CO2 = ClimateRegionData.AnnualWeather[ecoregion].MonthlyCO2[month];
            PAR0 = ClimateRegionData.AnnualWeather[ecoregion].MonthlyPAR[month];
            Prec = ClimateRegionData.AnnualWeather[ecoregion].MonthlyPrecip[month] * 10.0; // The climate library gives precipitation in cm, but PnET expects precipitation in mm, so multiply by 10.
            Tmax = ClimateRegionData.AnnualWeather[ecoregion].MonthlyMaxTemp[month];
            Tmin = ClimateRegionData.AnnualWeather[ecoregion].MonthlyMinTemp[month];
        }

        public double O3 { get; }
        public double CO2 { get; }
        public double PAR0 { get; }
        public double Prec { get; }
        public double Tmax { get; }
        public double Tmin { get; }
    }
}
