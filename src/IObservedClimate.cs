using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public interface IObservedClimate: IEnumerable<ObservedClimate>  
    {
        float O3 { get; }// Atmospheric O3 concentration 
        float CO2 { get; } // Atmospheric CO2 concentration (ppm)
        float PAR0 { get; }// Photosynthetically active radiation, average daily during the month (W/m2)
        float Prec { get; }// Precipitation (mm/mo)
        float Tmax { get; }// Maximum daily temperature 
        float Tmin { get; }// Minimum daily temperature
    }
}
