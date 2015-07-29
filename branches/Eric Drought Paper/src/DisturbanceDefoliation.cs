using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
 
using Edu.Wisc.Forest.Flel.Util;
 

namespace Landis.Extension.Succession.BiomassPnET
{
    class DisturbanceDefoliation
    {
        private static int TotalBiomass(ActiveSite site)
        {
            int TotalBiomass = 0;
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                ISpeciesCohorts s = PlugIn.Cohorts[site][species];
                if (s != null) foreach (ICohort cohort in s)
                    {
                        TotalBiomass += cohort.Biomass;
                    }
            }
            return TotalBiomass;
        }
        
        public static void Defoliate(ActiveSite site)
        {
            int totalB = TotalBiomass(site);

            // ---------------------------------------------------------
            // Defoliation ranges from 1.0 (total) to none (0.0).
            // Defoliation is calculated by an external function, typically an extension
            // with a defoliation calculator.  The method CohortDefoliation.Compute is a delegate method
            // and lives within the defoliating extension.

            foreach (ISpeciesCohorts speciesCohorts in PlugIn.Cohorts[site])
            {
                foreach (ICohort cohort in speciesCohorts)
                {
                    double defoliation = Landis.Library.BiomassCohorts.CohortDefoliation.Compute(cohort, site, totalB);
                    if (defoliation > 0)
                    {
                        cohort.Fol *= (1 - (float)defoliation);
                    }
                }
            }

            
        }
    }
}
