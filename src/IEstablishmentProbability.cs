using System.Collections.Generic;
using Landis.Library.DensityCohorts;

namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEstablishmentProbability
    {
        Landis.Library.Parameters.Species.AuxParm<byte> Probability { get; }

        //void Calculate_Establishment(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology);

        //Dictionary<ISpeciesDensity, float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology);

        //void ResetPerTimeStep();
         
        bool HasEstablished(ISpeciesDensity species);

        void EstablishmentTrue(ISpeciesDensity spc);

        //void RecordPest(int year, ISpeciesDensity spc, float pest,float fwater,float frad, bool estab, int monthCount);

        //float Get_FWater(ISpeciesDensity species);
        //float Get_FRad(ISpeciesDensity species);
    }
}
