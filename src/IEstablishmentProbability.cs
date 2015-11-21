using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEstablishmentProbability
    {
        Landis.Library.Parameters.Species.AuxParm<byte> Probability { get; }
         
        void Calculate_Establishment(EcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, float water);

        void ResetPerTimeStep();
         
        

        bool HasEstablished(ISpeciesPNET species);
       
        
    }
}
