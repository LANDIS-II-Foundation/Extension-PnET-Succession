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
    public class VarEcoregionSpeciesDate<T> 
    {
        private VarEcoregionSpecies<VarDate<T>> values;   

        public T this[IEcoregion eco, ISpecies s, DateTime d]
        {
            get
            {
                try
                {
                    return values[eco,s][d];
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
                    values[eco, s][d] = value;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }
        }

        public VarEcoregionSpeciesDate(string label, DateTime[] DateRange)
        {
            values = new VarEcoregionSpecies<VarDate<T>>(label);

            foreach (ISpecies s in PlugIn.modelCore.Species)
            {
                foreach (IEcoregion eco in PlugIn.ModelCore.Ecoregions)
                {
                    values[eco, s] = new VarDate<T>(label, DateRange);
                }
            }
        }
    }
   
}
