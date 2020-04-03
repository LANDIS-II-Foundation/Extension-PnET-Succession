
namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEcoregionClimateVariables
    {

        float PAR0 { get; }
        float Prec { get; }
        float Tday { get; }
        float VPD { get; }
        float Year { get; }
        float DaySpan { get; }
        float Daylength { get; }
        byte Month { get; }
        float Tave { get; }
        float Tmin { get; }
        float Tmax { get; }
        float CO2 { get; }
        float O3 { get; }
        
        SpeciesPnETVariables this[string species] { get; }
    }
}
