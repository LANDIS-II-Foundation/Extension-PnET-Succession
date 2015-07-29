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
    public class VarEcoregionDate<T> 
    {
        
        private Landis.Library.Biomass.Ecoregions.AuxParm<VarDate<T>> values;
        //values;  //Landis.Library.Biomass.Ecoregions.AuxParm<VarSpeciesDate<T>> values;


        public T this[IEcoregion eco, DateTime d]
        {
            get
            {
                try
                {
                    return values[eco][d];
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
                    values[eco][d] = value;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }
        }
        public VarEcoregionDate(string label, DateTime[] DateRange)
        {
            values = new Library.Biomass.Ecoregions.AuxParm<VarDate<T>>(PlugIn.modelCore.Ecoregions);
            foreach (IEcoregion eco in PlugIn.ModelCore.Ecoregions)
            {
                values[eco] = new VarDate<T>(label, DateRange);
            }
        }
        /*
        public VarEcoregionDate(string label, int FirstYear, int LastYear)
        {
            values = new VarEcoregion<VarDate<T>>();
            foreach (IEcoregion eco in PlugIn.ModelCore.Ecoregions)
            {
                values[eco] = new VarDate<T>(label, FirstYear, LastYear);
            }
        }
         */
    }
   
}
