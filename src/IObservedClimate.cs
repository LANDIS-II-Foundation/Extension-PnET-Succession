using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public interface IObservedClimate: IEnumerable<ObservedClimate>  
    {
        float CO2 { get; }
        float PAR0 { get; }
        float Prec { get; }
        float Tmax { get; }
        float Tmin { get; }
    }
}
