
namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEcoregionPnETVariables
    {

        float PAR0 { get; } // Photosynthetically active radiation, average daily during the month (umol/m2*s)
        float Prec { get; } // Precipitation (mm/mo)
        float Tday { get; } // Monthly average daytime air temp: (Tmax + Tave)/2
        float VPD { get; } // Vapor pressure deficit
        float Time { get; } // Decimal year and month
        int Year { get; }
        float DaySpan { get; } // Number of days in the month
        float Daylength { get; } // Length of daylight in seconds
        byte Month { get; }  // Numeric month
        float Tave { get; } // Monthly average air temp: (Tmin + Tmax)/2
        float Tmin { get; } // Monthly min air temp
        float Tmax { get; } // Monthly max air temp
        float CO2 { get; } // Atmospheric CO2 concentration (ppm)
        float O3 { get; } // Atmospheric O3 concentration, acumulated during growing season (AOT40) (ppb h)

        SpeciesPnETVariables this[string species] { get; }
    }
}
