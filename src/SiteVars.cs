using System;
using System.Collections.Generic;
using System.Text;

using Landis.Core;
using Landis.Library.DensityCohorts;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.BiomassPnET
{
    public static class SiteVars
    {
        private static ISiteVar<float> siteRD;

        public static void Initialize()
        {
            siteRD = PlugIn.ModelCore.Landscape.NewSiteVar<float>();

            PlugIn.ModelCore.RegisterSiteVar(siteRD, "Succession.SiteRd");
        }

        public static void TotalSiteRD(Landis.Library.DensityCohorts.ISpeciesCohorts speciesCohorts, ActiveSite site)
        {
            float siteRD = 0;

            ISpeciesDensity speciespnet = PlugIn.SpeciesDensity.AllSpecies[speciesCohorts.Species.Index];
            foreach (Landis.Library.DensityCohorts.ICohort cohort in speciesCohorts)
            {
                double tmp_term1 = Math.Pow((cohort.Diameter / 25.4), 1.605);
                float tmp_term2 = 10000 / 1;
                int tmp_term3 = cohort.Treenumber;
                int SDI = speciespnet.MaxSDI;
                double tmp = tmp_term1 * tmp_term2 * tmp_term3 / PlugIn.ModelCore.CellLength;
                siteRD += (float)tmp;
            }

            SiteVars.SiteRD[site] = siteRD;
            //return siteRD;
        }

        public static ISiteVar<float> SiteRD 
        {
            get
            {
                return siteRD;
            }
            
            set
            {
                siteRD = value;
            }
        }



    }
}
