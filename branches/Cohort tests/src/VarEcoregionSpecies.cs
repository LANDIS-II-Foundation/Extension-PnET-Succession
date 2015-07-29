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
    public class VarEcoregionSpecies<T> 
    {
        private   Library.Biomass.Ecoregions.AuxParm<Landis.Library.Biomass.Species.AuxParm<T>> values;
            //<Landis.Library.Biomass.Species.AuxParm<T>> values;
        string label;

        public T this[IEcoregion eco, ISpecies s]
        {
            get
            {
                try
                {
                    return values[eco][s];
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
                    values[eco][s] = value;
                }
                catch (System.Exception e)
                {
                    throw e;
                }
            }
        }
        public VarEcoregionSpecies(string label)
        {
            values = new Library.Biomass.Ecoregions.AuxParm<Library.Biomass.Species.AuxParm<T>>(PlugIn.modelCore.Ecoregions);
            //= new VarEcoregion<Library.Biomass.Species.AuxParm<T>>(label);
            this.label = label;
            foreach (IEcoregion eco in PlugIn.ModelCore.Ecoregions)
            {
                values[eco] = new Library.Biomass.Species.AuxParm<T>(PlugIn.modelCore.Species);
            }
             

        }
    }
   
}
