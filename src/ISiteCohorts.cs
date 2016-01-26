using System.Collections.Generic;
using Landis.Core;


namespace Landis.Extension.Succession.BiomassPnET
{
    public interface ISiteCohorts : Landis.Library.BiomassCohorts.ISiteCohorts
    {

        int[] NetPsn { get; }

        int[] MaintResp { get; }

        int[] GrossPsn { get; }

        int[] FolResp { get; }


        byte CanopyLAImax { get; }

        int AverageAge { get; }

        Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent { get; }

        IEstablishmentProbability EstablishmentProbability { get; }

        Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges { get; }

        float BiomassSum { get; }

        int CohortCount { get; }

        float SubCanopyParMAX { get; }

        double Litter { get; }

        double WoodyDebris { get; }

        int AgeMax { get; }

        ushort WaterMax { get; }

        uint BelowGroundBiomass { get; }
    }
}
