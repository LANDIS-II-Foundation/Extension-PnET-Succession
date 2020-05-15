//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Utilities;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// A calculator for computing the change in an individual cohort's biomass
    /// due to annual growth and mortality.
    /// </summary>
    public interface ICalculator
    {

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the change in an individual cohort's biomass due to annual
        /// growth and mortality.
        /// </summary>
        /// <param name="cohort">
        /// The cohort whose biomass the change is to be computed for.
        /// </param>
        /// <param name="site">
        /// The site where the cohort is located.
        /// </param>
        /// <param name="siteBiomass">
        /// The total biomass at the site.
        /// </param>
        /// <param name="prevYearSiteMortality">
        /// The total mortality at the site during the previous year.
        /// </param>

        int ComputeChange(ICohort cohort,
                          ActiveSite site);

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the percentage of a cohort's biomass that is non-woody.
        /// </summary>
        /// <param name="cohort">
        /// The cohort.
        /// </param>
        /// <param name="site">
        /// The site where the cohort is located.
        /// </param>
        Percentage ComputeNonWoodyPercentage(ICohort    cohort,
                                             ActiveSite site);
                                             
    }
}
