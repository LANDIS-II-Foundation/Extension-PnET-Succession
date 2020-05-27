using Landis.Library.DensityCohorts;
using System.Collections.Generic;

namespace Landis.Library.DensityCohorts.InitialCommunities
{
    /// <summary>
    /// An initial community.
    /// </summary>
    public interface ICommunity
    {
        /// <summary>
        /// The code that represents the community on maps.
        /// </summary>
        uint MapCode
        {
            get;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site cohorts in the community.
        /// </summary>
        List<ISpeciesCohorts> Cohorts
        {
            get;
        }
    }
}
