using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEstablishmentProbability
    {
        Landis.Library.Parameters.Species.AuxParm<byte> Probability { get; }

        //void Calculate_Establishment(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology);

        Dictionary<ISpeciesPNET,float> Calculate_Establishment_Month(IEcoregionPnETVariables pnetvars, IEcoregionPnET ecoregion, float PAR, IHydrology hydrology, float minHalfSat, float maxHalfSat, bool invertPest);

        void ResetPerTimeStep();
         
        bool HasEstablished(ISpeciesPNET species);

        void EstablishmentTrue(ISpeciesPNET spc);

        void RecordPest(int year, ISpeciesPNET spc, float pest,float fwater,float frad, bool estab, int monthCount);

        float Get_FWater(ISpeciesPNET species);
        float Get_FRad(ISpeciesPNET species);
    }
}
