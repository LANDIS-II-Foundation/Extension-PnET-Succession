//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.SpatialModeling;

using System;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// An iterator for a species' cohorts from oldest to youngest.
    /// </summary>
    public class OldToYoungIterator
    {
        private SpeciesCohorts cohorts;
 
        //  Index of the current cohort among the set of cohorts.
        private int? index;

        //  Age of the current cohort
        private int currentCohortAge;

        //  Index of the next cohort among the set of cohorts.
        private int nextIndex;

        //  Did the current cohort die during its annual growth?
        private bool currentCohortDied;

        //---------------------------------------------------------------------

        /// <summary>
        /// The age of the current cohort.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// There is no current cohort because 1) the iterator has gone through
        /// all the cohorts (i.e., the last call to the MoveNext method
        /// returned false); or 2) the last call to the GrowCurrentCohort
        /// method resulted in the current cohort's death due to senescence.
        /// </exception>
        public int Age
        {
            get {
                if (index.HasValue)
                    return currentCohortAge;
                throw NoCurrentCohortException();
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The set of cohorts the iterator is iterating through.
        /// </summary>
        public SpeciesCohorts SpeciesCohorts
        {
            get {
                return cohorts;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance for a set of species cohorts.
        /// </summary>
        public OldToYoungIterator(SpeciesCohorts cohorts)
        {
            this.cohorts = cohorts;
            this.nextIndex = 0;
            this.currentCohortDied = false;
            MoveNext();
        }

        //---------------------------------------------------------------------

        private InvalidOperationException NoCurrentCohortException()
        {
            string mesg = "Old-to-young iterator has no current cohort";
            if (currentCohortDied)
                mesg = mesg + "; the previous current cohort died";
            return new InvalidOperationException(mesg);
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Grows the current cohort for one year.
        /// </summary>
        /// <param name="site">
        /// The site where the cohort is located.
        /// </param>
        /// <param name="siteBiomass">
        /// The total biomass at the site.  This parameter is changed by the
        /// same amount as the current cohort's biomass.
        /// </param>
        /// <param name="prevYearMortality">
        /// The total mortality at the site during the previous year.
        /// </param>
        /// <returns>
        /// The total mortality (excluding annual leaf litter) for the current
        /// cohort.
        /// </returns>
        public void GrowCurrentCohort(ActiveSite site)
                                     //ref int    siteBiomass, 
                                     //int        prevYearMortality)
        {
            if (! index.HasValue)
                throw NoCurrentCohortException();

            //int cohortMortality;
            nextIndex = cohorts.GrowCohort(index.Value, site); //, ref siteBiomass, prevYearMortality, out cohortMortality);

            currentCohortDied = (nextIndex == index.Value);
            return; // cohortMortality;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Advances the iterator to the next cohort.
        /// </summary>
        /// <returns>
        /// True if there is another cohort to process.  False if there are no
        /// more cohorts.
        /// </returns>
        public bool MoveNext()
        {
            index = nextIndex;
            if (0 <= index && index < cohorts.Count) {
                currentCohortAge = cohorts.GetAge(index.Value);
                return true;
            }
            else {
                //  No more cohorts
                index = null;
                return false;
            }
        }
    }
}
