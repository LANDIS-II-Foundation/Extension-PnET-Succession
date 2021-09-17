//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using Landis.Library.DensityCohorts;
using Landis.Library.Biomass;
using System.Collections.Generic;
using Landis.SpatialModeling;

namespace Landis.Extension.Output.Density
{
    /// <summary>
    /// The pools of dead biomass for the landscape's sites.
    /// </summary>
    public static class SiteVars
    {
        //private static ISiteVar<Pool> woodyDebris;
        //private static ISiteVar<Pool> litter;
        private static ISiteVar<ISiteCohorts> cohorts;


        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {
            cohorts = PlugIn.ModelCore.GetSiteVar<ISiteCohorts>("Succession.DensityCohorts");

            if (cohorts == null)
            {
                string mesg = string.Format("Cohorts are empty.  Please double-check that this extension is compatible with your chosen succession extension.");
                throw new System.ApplicationException(mesg);
            }
        }

        //---------------------------------------------------------------------
        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
        }
        //---------------------------------------------------------------------

/*        /// <summary>
        /// The intact dead woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> WoodyDebris
        {
            get {
                return woodyDebris;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> Litter
        {
            get {
                return litter;
            }
        }*/

    }
}
