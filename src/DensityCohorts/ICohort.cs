//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// A species cohort with number of tree information.
    /// </summary>
    public interface ICohort
        :Landis.Library.BiomassCohorts.ICohort, Landis.Library.AgeOnlyCohorts.ICohort
    {
        /// <summary>
        /// The number of individual trees in the cohort.
        /// </summary>
        int Treenumber
        {
            get;
        }

        float Diameter
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes how much of the cohort's biomass is non-woody.
        /// </summary>
        /// <param name="site">
        /// The site where the cohort is located.
        /// </param>
        //int ComputeNonWoodyBiomass(ActiveSite site);
        
    }
}
