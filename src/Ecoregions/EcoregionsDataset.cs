using System.Collections;
using System.Collections.Generic;

using Landis.Core;


namespace Landis.Extension.Succession.BiomassPnET
{
    public class EcoregionsDataset : IEcoregionDataset
    {
        private IEcoregionPNET[] ecoregions;

      
        public IEcoregion this[int value]
        {
            get
            {
                return ecoregions[value];
            }
        }
        public int Count
        {
            get
            {
                return ecoregions.Length;
            }
        }
        public IEcoregion this[string name]
        {
            get
            {
                foreach (IEcoregionPNET ecoregionPnET in ecoregions)
                {
                    if (ecoregionPnET.Name == name)
                    {
                        return ecoregionPnET;
                    }
                }
                throw new System.Exception("Cannot find ecoreigon "+ name);

            }
        }
        public IEcoregion Find(ushort u) 
        {

            return ecoregions[u];
        }

        public IEcoregionPNET Get(Landis.Core.IEcoregion eco)
        {
            foreach (IEcoregionPNET eco2 in ecoregions)
            {
                if (eco2.Name == eco.Name) return eco2;
            }
            throw new System.Exception("No PnET ecoregion entry for " + eco.Name);
        }

        IEnumerator<Landis.Core.IEcoregion> System.Collections.Generic.IEnumerable<Landis.Core.IEcoregion>.GetEnumerator()
        {
            foreach (IEcoregionPNET eco in ecoregions)
                yield return eco;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<IEcoregion>)this).GetEnumerator();
        }

        public EcoregionsDataset()
        {
            ecoregions = new IEcoregionPNET[PlugIn.ModelCore.Ecoregions.Count];
            foreach (IEcoregion eco in PlugIn.ModelCore.Ecoregions)
            {
                IEcoregionParameters  parameters = new EcoregionParameters(eco);
                ecoregions[eco.Index] = new EcoregionPnET(eco, parameters);
            }
        }
    }

}
