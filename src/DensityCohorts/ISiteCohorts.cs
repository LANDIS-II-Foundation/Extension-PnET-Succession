//  Authors:  Robert M. Scheller, James B. Domingo

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// All the density cohorts at a site.
    /// </summary>
    public interface ISiteCohorts
        : Landis.Library.Cohorts.ISiteCohorts<DensityCohorts.ISpeciesCohorts>

    {
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
