using Landis.Core;
using System;
using System.Collections.Generic;
using Landis.Library.DensityCohorts;

namespace Landis.Extension.Succession.Density
{
    public class EstablishmentProbability  : IEstablishmentProbability
    {
        private LocalOutput establishment_siteoutput;
        private List<ISpeciesDensity> _hasEstablished;
        private Dictionary<ISpeciesDensity, float> _pest;
        private Dictionary<ISpeciesDensity, float> _fwater;
        private Dictionary<ISpeciesDensity, float> _frad;

        private static int Timestep;

       
        public bool HasEstablished(ISpeciesDensity species)
        {
            return _hasEstablished.Contains(species);
        }
       
        public Landis.Library.Parameters.Species.AuxParm<byte> Probability
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<byte> probability = new Library.Parameters.Species.AuxParm<byte>(PlugIn.ModelCore.Species);
                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    ISpeciesDensity speciespnet = PlugIn.SpeciesDensity[spc];
                    probability[spc] = (byte)(100F * _pest[speciespnet]);
                }
                return probability;
            }
        }
       
        public static void Initialize(int timestep)
        {
            Timestep = timestep;

             
        }
     
        public void EstablishmentTrue(ISpeciesDensity spc)
        {
            _hasEstablished.Add(spc);
        }
        
    }
}
