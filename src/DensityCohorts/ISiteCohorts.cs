//  Authors:  Robert M. Scheller, James B. Domingo
using System.Collections.Generic;
using Landis.Core;
//using Landis.Extension.Succession.BiomassPnET;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// All the density cohorts at a site.
    /// </summary>
    public interface ISiteCohorts
        : Landis.Library.Cohorts.ISiteCohorts<DensityCohorts.ISpeciesCohorts>

    {
        int AverageAge { get; }

        Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies { get; }

        Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent { get; }

        //Landis.Extension.Succession.BiomassPnET.IEstablishmentProbability EstablishmentProbability { get; }

        Landis.Library.Parameters.Species.AuxParm<int> SpeciesSeed { get; }

        Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges { get; }

        int CohortCount { get; }

        int AgeMax { get; }

        int ShadeMax { get; }

        List<ISpecies> SpeciesByPlant { get; set; }
        List<ISpecies> SpeciesBySerotiny { get; set; }
        List<ISpecies> SpeciesByResprout { get; set; }
        List<ISpecies> SpeciesBySeed { get; set; }

        List<int> CohortsBySuccession { get; set; }
        List<int> CohortsByHarvest { get; set; }
        List<int> CohortsByFire { get; set; }
        List<int> CohortsByWind { get; set; }
        List<int> CohortsByOther { get; set; }

        /// <summary>
        /// Computes who much a disturbance damages the cohorts by reducing
        /// their biomass.
        /// </summary>
        /// <returns>
        /// The total of all the cohorts' biomass reductions.
        /// </returns>
        int ReduceOrKillBiomassCohorts(IDisturbance disturbance);
    }
}
