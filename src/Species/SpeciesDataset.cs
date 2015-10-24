using System.Collections;
using System.Collections.Generic;

using Landis.Core;


namespace Landis.Extension.Succession.BiomassPnET
{
    public class SpeciesDataset: ISpeciesDataset
    {
        private ISpeciesPNET[] species;

        public ISpeciesPNET Get(Landis.Core.ISpecies spc)
        {
            foreach (ISpeciesPNET s in species)
            {
                if (s.Name == spc.Name) return s;
            }
            throw new System.Exception("No PnET species entry for "+spc.Name);
        }

        IEnumerator<ISpeciesPNET> IEnumerable<ISpeciesPNET>.GetEnumerator()
        {
            foreach (ISpeciesPNET sp in species)
                yield return sp;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<ISpecies>)this).GetEnumerator();
        }

        public SpeciesDataset()
        {
            species = new ISpeciesPNET[PlugIn.ModelCore.Species.Count];
            foreach (ISpecies s in PlugIn.ModelCore.Species)
            {
                ISpeciesParameters parameters = new SpeciesParameters(s);
                species[s.Index] = new SpeciesPnET(s, parameters);
            }
        }
    }

}

 