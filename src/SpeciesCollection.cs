using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.InitialCommunities;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Linq;


namespace Landis.Extension.Succession.BiomassPnET
{
    public class SpeciesCollection : IEnumerable<Landis.Library.BiomassCohortsPnET.PnETSpecies> 
    {
          
        Dictionary<ISpecies, Landis.Library.BiomassCohortsPnET.PnETSpecies> AllSpecies = new Dictionary<ISpecies, PnETSpecies>();
 
        public Landis.Library.BiomassCohortsPnET.PnETSpecies this[ISpecies species]
        {
            get
            {
                return AllSpecies[species];
            }
        }

        
        // Return species parameter
        string GetSpeciesParameter<T>(string label, T min, T max, ISpecies species) where T : IComparable 
        {
            Parameter2<T> p = (Parameter2<T>)PlugIn.GetParameter2<T>(label, min, max);
            return p[species.Name].ToString(); 
        }
         
        public SpeciesCollection()
        {
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                AllSpecies[species] = new Landis.Library.BiomassCohortsPnET.PnETSpecies(species, GetSpeciesParameter);
            }
        }
        public IEnumerator<Landis.Library.BiomassCohortsPnET.PnETSpecies> GetEnumerator()
        {
            return AllSpecies.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
         

    }
}
