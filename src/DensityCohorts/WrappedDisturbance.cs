//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using Landis.Core;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// A wrapped age-cohort disturbance so it works with biomass cohorts.
    /// </summary>
    public class WrappedDisturbance
        : IDisturbance
    {
        private AgeOnlyCohorts.ICohortDisturbance ageCohortDisturbance;

        //---------------------------------------------------------------------

        public WrappedDisturbance(AgeOnlyCohorts.ICohortDisturbance ageCohortDisturbance)
        {
            this.ageCohortDisturbance = ageCohortDisturbance;
        }

        //---------------------------------------------------------------------

        public ExtensionType Type
        {
            get {
                return ageCohortDisturbance.Type;
            }
        }

        //---------------------------------------------------------------------

        public ActiveSite CurrentSite
        {
            get {
                return ageCohortDisturbance.CurrentSite;
            }
        }

        //---------------------------------------------------------------------

        public int ReduceOrKillMarkedCohort(ICohort cohort)
        {
            if (ageCohortDisturbance.MarkCohortForDeath(cohort)) {
                Cohort.KilledByAgeOnlyDisturbance(this, cohort,
                                                  ageCohortDisturbance.CurrentSite,
                                                  ageCohortDisturbance.Type);
                return cohort.Treenumber;
            }
            else
                return 0;
        }
    }
}
