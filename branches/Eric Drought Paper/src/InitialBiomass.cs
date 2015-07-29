//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Library.BiomassCohortsPnET;
using Landis.Core;
using System.Collections.Generic;
using Landis.Library.InitialCommunities;
using System.Diagnostics;
using System.Threading;
using Landis.Library.Biomass;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// The initial live and dead biomass at a site.
    /// </summary>
    public class InitialBiomass
    {
        private static ISiteVar<bool> HasSiteOutput;
        private ISiteCohorts cohorts;
        private Landis.Library.Biomass.Pool deadWoodyPool;
        private Pool deadNonWoodyPool;
        
        /// <summary>
        /// The site's initial cohorts.
        /// </summary>
        public ISiteCohorts Cohorts
        {
            get
            {
                return cohorts;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial dead woody pool.
        /// </summary>
        public Pool DeadWoodyPool
        {
            get
            {
                return deadWoodyPool;
            }
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// The site's initial dead non-woody pool.
        /// </summary>
        public Pool DeadNonWoodyPool
        {
            get
            {
                return deadNonWoodyPool;
            }
        }

        
        
        //---------------------------------------------------------------------
        private float water, annualtrans, canopylai, subcanopypar, canopylaimax;
        
        private InitialBiomass(ISiteCohorts cohorts,
                               Pool deadWoodyPool,
                               Pool deadNonWoodyPool,
                               float water,
                               float annualtrans,
                               float canopylai,
                               float canopylaimax,
                               float subcanopypar 
                               )
        {
            
            this.canopylaimax = canopylaimax;
            this.cohorts = cohorts;
            this.deadWoodyPool = deadWoodyPool;
            this.deadNonWoodyPool = deadNonWoodyPool;
            this.water = water;
            this.annualtrans = annualtrans;
            this.canopylai = canopylai;
            this.subcanopypar = subcanopypar;
        }
        //---------------------------------------------------------------------
        public static ISiteCohorts Clone(ActiveSite site, ISiteCohorts site_cohorts)
         {
             ISiteCohorts clone = new SiteCohorts();
             foreach (ISpeciesCohorts speciesCohorts in site_cohorts)
                 foreach (Cohort cohort in speciesCohorts)
                 {
                     clone.AddNewCohort(cohort);   
                 }
             return clone;
         }
      
        //---------------------------------------------------------------------

        private static IDictionary<uint, InitialBiomass> initialSites;
        //  Initial site biomass for each unique pair of initial
        //  community and ecoregion; Key = 32-bit unsigned integer where
        //  high 16-bits is the map code of the initial community and the
        //  low 16-bits is the ecoregion's map code

        private static IDictionary<uint, List<Landis.Library.AgeOnlyCohorts.ICohort>> sortedCohorts;
        //  Age cohorts for an initial community sorted from oldest to
        //  youngest.  Key = initial community's map code

        private static ushort successionTimestep;

        //---------------------------------------------------------------------

        private static uint ComputeKey(uint initCommunityMapCode,
                                       ushort ecoregionMapCode)
        {
            return (uint)((initCommunityMapCode << 16) | ecoregionMapCode);
        }

        //---------------------------------------------------------------------

        static InitialBiomass()
        {
            initialSites = new Dictionary<uint, InitialBiomass>();
            sortedCohorts = new Dictionary<uint, List<Landis.Library.AgeOnlyCohorts.ICohort>>();
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="timestep">
        /// The plug-in's timestep.  It is used for growing biomass cohorts.
        /// </param>
        public static void Initialize(IInputParameters parameters)
        {
            HasSiteOutput = parameters.HasSiteOutput;
            successionTimestep = (ushort)parameters.Timestep;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Computes the initial biomass at a site.
        /// </summary>
        /// <param name="site">
        /// The selected site.
        /// </param>
        /// <param name="initialCommunity">
        /// The initial community of age cohorts at the site.
        /// </param>

        public static InitialBiomass ComputeInitialBiomass(ActiveSite site,
                                             ICommunity initialCommunity)
        {
            
            InitialBiomass initialBiomass;
            
            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            uint key = ComputeKey(initialCommunity.MapCode, ecoregion.MapCode);

            if (initialSites.TryGetValue(key, out initialBiomass) && HasSiteOutput[site]==false)
            {
                CanopyBiomass.CanopyLAImax[site] = initialBiomass.canopylaimax;
                CanopyBiomass.CanopyLAI[site] = initialBiomass.canopylai;
                Hydrology.Water[site] = initialBiomass.water;
                Hydrology.AnnualTranspiration[site] = initialBiomass.annualtrans;
                CanopyBiomass.SubCanopyPAR[site] = initialBiomass.subcanopypar;
                return initialBiomass;
            }
            

            //  If we don't have a sorted list of age cohorts for the initial
            //  community, make the list
             
            List<Landis.Library.AgeOnlyCohorts.ICohort> sortedAgeCohorts;
            if (!sortedCohorts.TryGetValue(initialCommunity.MapCode, out sortedAgeCohorts))
            {
                sortedAgeCohorts = PlugIn.RankCohortAgesOldToYoung(initialCommunity.Cohorts);
                sortedCohorts[initialCommunity.MapCode] = sortedAgeCohorts;
            }

            if (sortedAgeCohorts.Count == 0) return null;
                
            ISiteCohorts cohorts = MakeBiomassCohorts(sortedAgeCohorts, site);



            initialBiomass = new InitialBiomass(cohorts,
                                                ForestFloor.WoodyDebris[site],
                                                ForestFloor.Litter[site],
                                                Hydrology.Water[site],
                                                Hydrology.AnnualTranspiration[site],
                                                CanopyBiomass.CanopyLAI[site],
                                                CanopyBiomass.CanopyLAImax[site],
                                                CanopyBiomass.SubCanopyPAR[site]);

            
            initialSites[key] = initialBiomass;
            return initialBiomass;
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// A method that computes the initial biomass for a new cohort at a
        /// site based on the existing cohorts.
        /// </summary>
        public delegate int ComputeMethod(ISpecies species,
                                             ISiteCohorts ISiteCohorts,
                                             ActiveSite site);

        //---------------------------------------------------------------------

        /// <summary>
        /// Makes the set of biomass cohorts at a site based on the age cohorts
        /// at the site, using a specified method for computing a cohort's
        /// initial biomass.
        /// </summary>
        /// <param name="ageCohorts">
        /// A sorted list of age cohorts, from oldest to youngest.
        /// </param>
        /// <param name="site">
        /// Site where cohorts are located.
        /// </param>
        /// <param name="initialBiomassMethod">
        /// The method for computing the initial biomass for a new cohort.
        /// </param>
        
        public static ISiteCohorts MakeBiomassCohorts(List<Landis.Library.AgeOnlyCohorts.ICohort> ageCohorts,ActiveSite site)
                                                     
        {

            PlugIn.Cohorts[site] = new Library.BiomassCohortsPnET.SiteCohorts();

            Hydrology.Initialize(site);
            
            Hydrology.AnnualTranspiration[site] = 0;
            CanopyBiomass.CanopyLAI[site] = 0;
             
            if (ageCohorts.Count == 0)
            {
                return PlugIn.Cohorts[site];
            }

            System.DateTime SpinUpDate = PlugIn.StartDate.AddYears(-(ageCohorts[0].Age) + ((successionTimestep == 1) ? -1 : 0));

            while (SpinUpDate.CompareTo(PlugIn.StartDate) < 0)
            {
                CanopyBiomass.SubCanopyPAR[site] = Static.PAR0[SpinUpDate];

                //  Add those cohorts that were born at the current year
                foreach (Library.AgeOnlyCohorts.ICohort ageonlycohort in ageCohorts)
                {
                    if (PlugIn.StartDate.Year - SpinUpDate.Year == ageonlycohort.Age)
                    {
                        Cohort cohort = new Cohort(ageonlycohort.Species, 1, ConstantParameters.InitialFol, 0, 0, 0, 0, SpinUpDate.Year, true);
                        PlugIn.Cohorts[site].AddNewCohort(cohort);
                        CohortOutput.WriteHeader(site, cohort);
                        SiteOutput.WriteHeader(site); 
                    }
                }

                PlugIn.GrowCohorts(site, SpinUpDate, SpinUpDate.AddYears(1), false);

                SpinUpDate = SpinUpDate.AddYears(1);
            }
             
             
            return PlugIn.Cohorts[site];
        }
    }
}
