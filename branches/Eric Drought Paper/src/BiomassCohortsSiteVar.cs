// Copyright 2010 Green Code LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using BiomassCohortsPnET = Landis.Library.BiomassCohortsPnET;
using BiomassCohorts = Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Wraps a biomass-cohorts site variable and provides access to it as a
    /// site variable of base cohorts.
    /// </summary>
    public class BiomassCohortsSiteVar
        : ISiteVar<BiomassCohorts.ISiteCohorts>
    {
        private ISiteVar<BiomassCohortsPnET.ISiteCohorts> PnETCohortSiteVar;

        public BiomassCohortsSiteVar(ISiteVar<BiomassCohortsPnET.ISiteCohorts> siteVar)
        {
            PnETCohortSiteVar = siteVar;
        }

        #region ISiteVariable members
        System.Type ISiteVariable.DataType
        {
            get
            {
                return typeof(BiomassCohorts.ISiteCohorts);
            }
        }

        InactiveSiteMode ISiteVariable.Mode
        {
            get
            {
                return PnETCohortSiteVar.Mode;
            }
        }

        ILandscape ISiteVariable.Landscape
        {
            get
            {
                return PnETCohortSiteVar.Landscape;
            }
        }
        #endregion

        #region ISiteVar<BaseCohorts.ISiteCohorts> members
        // Extensions other than succession have no need to assign the whole
        // site-cohorts object at any site.

        BiomassCohorts.ISiteCohorts ISiteVar<BiomassCohorts.ISiteCohorts>.this[Site site]
        {
            get
            {
                return (BiomassCohorts.ISiteCohorts)PnETCohortSiteVar[site];
                //return biomassCohortSiteVar[site]; 
            }
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        BiomassCohorts.ISiteCohorts ISiteVar<BiomassCohorts.ISiteCohorts>.ActiveSiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        BiomassCohorts.ISiteCohorts ISiteVar<BiomassCohorts.ISiteCohorts>.InactiveSiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }

        BiomassCohorts.ISiteCohorts ISiteVar<BiomassCohorts.ISiteCohorts>.SiteValues
        {
            set
            {
                throw new System.InvalidOperationException("Operation restricted to succession extension");
            }
        }
        #endregion
    }
}
