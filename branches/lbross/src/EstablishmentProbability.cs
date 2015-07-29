using Landis.SpatialModeling;
using Landis.Core;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{
    public static class EstablishmentProbability  
    {
        public static string Header
        {
            get
            {
                return "Year" + ","  + "Species" + ","  + "Pest" + "," + "Est";
            }
        }
        public static double Calculate_Establishment(ISpecies Species, float PAR, float PressureHead)
        {
           

            double frad = Math.Pow(SubCanopyLayer.CumputeFrad(PAR, Species.HalfSat()), Species.EstRad());
            double fwater = Math.Pow(SubCanopyLayer.GetFWater(Species, PressureHead), Species.EstMoist());

            return frad * fwater;
        }
        public static bool ComputeEstablishment(DateTime date, ISpecies Species, double pest, LocalOutput establishment_siteoutput)
        {
            bool est = pest > (float)PlugIn.ContinuousUniformRandom();

            if (establishment_siteoutput != null)
            {
                establishment_siteoutput.Add(date.Year.ToString() + "," + Species.Name + "," +  pest + "," + est);

                // TODO: win time by reducing calls to write
                establishment_siteoutput.Write();
            }
            if (Species.PreventEstablishment()) return false;
            return est;
        }


        
    }
}
