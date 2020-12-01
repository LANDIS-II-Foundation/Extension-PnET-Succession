using System.Collections.Generic;
using Landis.Core;


namespace Landis.Extension.Succession.BiomassPnET 
{
    public interface ISiteCohorts : Landis.Library.BiomassCohorts.ISiteCohorts
    {

        float[] NetPsn { get; }

        float[] MaintResp{ get; }

        float[] GrossPsn{ get; }

        float[] FolResp { get; }
        
        
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

        float WaterMax { get; }

        uint BelowGroundBiomass { get; }

        float FoliageSum { get; }

        float NSCSum { get; }

        float AETSum { get; } //mm

        float NetPsnSum { get; }

        List<ISpecies> SpeciesByPlant { get; set; }
        List<ISpecies> SpeciesBySerotiny { get; set; }
        List<ISpecies> SpeciesByResprout { get; set; }
        List<ISpecies> SpeciesBySeed { get; set; }

        List<int> CohortsBySuccession { get; set; }
        List<int> CohortsByHarvest { get; set; }
        List<int> CohortsByFire { get; set; }
        List<int> CohortsByWind { get; set; }
        List<int> CohortsByOther { get; set; }

    }
}
