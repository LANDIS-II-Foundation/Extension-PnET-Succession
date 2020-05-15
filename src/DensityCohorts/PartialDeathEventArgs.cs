//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// Information about a cohort's death.
    /// </summary>
    public class PartialDeathEventArgs
    {
        private ICohort cohort;
        private ActiveSite site;
        private ExtensionType disturbanceType;
        private float reduction;

        //---------------------------------------------------------------------

        /// <summary>
        /// The cohort that died.
        /// </summary>
        public ICohort Cohort
        {
            get
            {
                return cohort;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site where the cohort died.
        /// </summary>
        public ActiveSite Site
        {
            get
            {
                return site;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The type of disturbance that killed the cohort.
        /// </summary>
        /// <remarks>
        /// null if the cohort died during the growth phase of succession.
        /// </remarks>
        public ExtensionType DisturbanceType
        {
            get
            {
                return disturbanceType;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The type of disturbance that killed the cohort.
        /// </summary>
        public float Reduction
        {
            get
            {
                return reduction;
            }
        }
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public PartialDeathEventArgs(ICohort cohort,
                              ActiveSite site,
                              ExtensionType disturbanceType, float reduction)
        {
            this.cohort = cohort;
            this.site = site;
            this.disturbanceType = disturbanceType;
            this.reduction = reduction;
        }
    }
}
