using Landis.Library.DensityCohorts;
using System.Collections.Generic;

namespace Landis.Library.DensityCohorts.InitialCommunities
{
    public class Community
        : Landis.Library.DensityCohorts.InitialCommunities.ICommunity
    {
        private uint mapCode;
        private List<ISpeciesCohorts> cohorts;

        //---------------------------------------------------------------------

        public uint MapCode
        {
            get {
                return mapCode;
            }
        }

        //---------------------------------------------------------------------

        public List<ISpeciesCohorts> Cohorts
        {
            get {
                return cohorts;
            }
        }

        //---------------------------------------------------------------------

        public Community(uint mapCode, List<ISpeciesCohorts> cohorts)
        {
            this.mapCode = mapCode;
            this.cohorts = cohorts;
        }
    }
}
