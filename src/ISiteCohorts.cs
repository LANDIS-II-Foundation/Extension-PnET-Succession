using System.Collections.Generic;


namespace Landis.Extension.Succession.BiomassPnET 
{
    public interface ISiteCohorts : Landis.Library.BiomassCohorts.ISiteCohorts
    {

        int[] NetPsn { get; }

        int[] MaintResp{ get; }
        
        int[] GrossPsn{ get; }

        int[] FolResp { get; }
        
        
        byte CanopyLAImax{get;}

        int AverageAge { get; }

        Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent { get; }

        IEstablishmentProbability EstablishmentProbability { get; }
        
        Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges { get; }

        float BiomassSum { get; }

        float AbovegroundBiomassSum { get; }

        float WoodySenescenceSum { get; }

        float FoliageSenescenceSum { get; }

        int CohortCount { get; }

        float SubCanopyParMAX { get; }

        double Litter{ get; }

        double WoodyDebris { get; }

        int AgeMax { get; }

        ushort WaterMax { get; }

        uint BelowGroundBiomass { get; }

        float FoliageSum { get; }

        float NSCSum { get; }
    }
}
