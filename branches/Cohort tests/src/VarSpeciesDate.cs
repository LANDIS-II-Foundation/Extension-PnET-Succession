using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class VarSpeciesDate<T> 
    {
        private Landis.Library.Biomass.Species.AuxParm<VarDate<T>> values;

        public T this[DateTime d, ISpecies s]
        {
            get
            {
                try
                {
                    return values[s][d];
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }
            set
            {
                try
                {
                    values[s][d] = value;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }
        }
        public VarSpeciesDate(string label, DateTime[] DateRange)
        {
            values = new Library.Biomass.Species.AuxParm<VarDate<T>>(PlugIn.ModelCore.Species);

            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                values[spc] = new VarDate<T>(label, DateRange);
            }
        }
        /*
        public VarSpeciesDate(string label, int FirstYear, int LastYear)
        {
            values = new Library.Biomass.Species.AuxParm<VarDate<T>>(PlugIn.ModelCore.Species);

            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                values[spc] = new VarDate<T>(label, FirstYear, LastYear);
            }
        }
         */
    }
   
}
