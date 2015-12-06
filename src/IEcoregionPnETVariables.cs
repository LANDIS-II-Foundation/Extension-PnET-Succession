using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEcoregionPnETVariables
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
        SpeciesPnETVariables this[string species] { get; }
    }
}
