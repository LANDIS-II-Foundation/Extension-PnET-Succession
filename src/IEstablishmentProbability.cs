using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEstablishmentProbability
    {
        Landis.Library.Parameters.Species.AuxParm<double> Probability { get; }

        //void Calculate_Establishment(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology);

        Dictionary<ISpeciesPNET,float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology);

        void ResetPerTimeStep();
         
        bool HasEstablished(ISpeciesPNET species);

        void EstablishmentTrue(ISpeciesPNET spc);

        void RecordPest(int year, ISpeciesPNET spc, float pest, bool estab);
    }
}
