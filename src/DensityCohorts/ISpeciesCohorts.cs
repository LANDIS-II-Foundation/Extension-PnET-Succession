//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Core;
using System.Collections.Generic;

namespace Landis.Library.DensityCohorts
{
        /// The biomass cohorts for a particular species at a site.
        /// </summary>
        public interface ISpeciesCohorts
             : Landis.Library.Cohorts.ISpeciesCohorts<Landis.Library.DensityCohorts.ICohort>
        {
        }
    
}
