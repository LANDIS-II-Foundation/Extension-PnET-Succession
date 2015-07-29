//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Succession;
using Landis.Library.InitialCommunities;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using Landis.Library.Biomass;
//

namespace Landis.Extension.Succession.BiomassPnET
{
    public class PlugIn
        : Landis.Library.Succession.ExtensionBase
    {
        static int lastCurrentTime;
        public static readonly string ExtensionName = "PnET-Succession";
        public static ICore modelCore;
        private IInputParameters parameters;
        public static double CurrentYearSiteMortality;
        public ISiteVar<SiteConditions> siteconditions;
        static ISiteVar<bool> hassiteoutput;
        private static Landis.Library.Biomass.BaseCohortsSiteVar baseCohorts;
        private static BiomassCohortsSiteVar BiomassCohorts;
        static Random r = new Random();
        static int year;
        static int month;
        public static DateTime StartDate;
        private static int tstep;

        public static void SetYear(int Year)
        {
            year = Year;
        }

        public static ISiteVar<bool> HasSiteOutput
        {
            get
            {
                return hassiteoutput;
            }
        }
         
        
        public static int TStep  {   get    {      return tstep;     }   }
       
        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName)
        {
           
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            
            modelCore = mCore;
            InputParametersParser parser = new InputParametersParser(dataFile);
            parameters = Landis.Data.Load<IInputParameters>(dataFile, parser);
        }

        //---------------------------------------------------------------------
        
        public static ICore ModelCore
        {
            get
            {
                return modelCore;
            }
        }
        //---------------------------------------------------------------------
        public override void Initialize()
        {
            PlugIn.ModelCore.UI.WriteLine("Initializing " + ExtensionName + " version "+ typeof(PlugIn).Assembly.GetName().Version);
            
            /*Testing
             * Landis.Library.BiomassCohorts.Cohort C;
            Cohort D;

            C = (Landis.Library.BiomassCohorts.Cohort)D;
            */
           

            hassiteoutput = parameters.HasSiteOutput;
            siteconditions = PlugIn.ModelCore.Landscape.NewSiteVar<SiteConditions>();
            Edu.Wisc.Forest.Flel.Util.Directory.EnsureExists("output");

            //CohortOutput.Initialize(parameters);
            Hydrology.Initialize(parameters);
            StaticVariables.InitializeStatic(parameters);
            ForestFloor.Initialize(parameters);
            SiteConditions.Initialize(parameters);
            CohortBiomass.Initialize(parameters);

            Timestep = parameters.Timestep;
            tstep = parameters.Timestep;

            // Initialize Reproduction routines:
            Reproduction.SufficientResources = SufficientResources;
            Reproduction.Establish = Establish;
            Reproduction.AddNewCohort = AddNewCohort;
            Reproduction.MaturePresent = MaturePresent;
            Reproduction.PlantingEstablish = PlantingEstablish;
            base.Initialize(modelCore, parameters.SeedAlgorithm);
            Cohort.DeathEvent += CohortDied;
 
            StartDate = new System.DateTime(parameters.StartYear, 1, 15);

            year = StartDate.Year;
            month = StartDate.Month;

            InitializeSites(parameters.InitialCommunities, parameters.InitialCommunitiesMap, modelCore);

            EstablishmentProbability.Initialize(parameters);

            ISiteVar<ISiteCohorts> cohorts = PlugIn.ModelCore.Landscape.NewSiteVar<ISiteCohorts>();

            foreach (ActiveSite site in PlugIn.modelCore.Landscape)
            {              
                cohorts[site] = siteconditions[site].Cohorts;
            }

            BiomassCohorts = new BiomassCohortsSiteVar(cohorts);
            baseCohorts = new Landis.Library.Biomass.BaseCohortsSiteVar(BiomassCohorts);

            PlugIn.ModelCore.RegisterSiteVar(BiomassCohorts, "Succession.BiomassCohorts");

            PlugIn.ModelCore.RegisterSiteVar(siteconditions, "Succession.SiteConditionsPnET");
            PlugIn.ModelCore.RegisterSiteVar(baseCohorts, "Succession.AgeCohorts");


            ISiteVar<Pool> WoodyDebris = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
            foreach (ActiveSite site in PlugIn.modelCore.Landscape)
            {
                WoodyDebris[site] = siteconditions[site].WoodyDebris;
            }
            PlugIn.ModelCore.RegisterSiteVar(WoodyDebris, "Succession.WoodyDebris");

            ISiteVar<Pool> Litter = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
            foreach (ActiveSite site in PlugIn.modelCore.Landscape)
            {
                Litter[site] = siteconditions[site].WoodyDebris;
            }
            PlugIn.ModelCore.RegisterSiteVar(Litter, "Succession.Litter");
            


        }
        //---------------------------------------------------------------------
        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        {

            siteconditions[site] = new SiteConditions(site, initialCommunity);
        }

        //---------------------------------------------------------------------
        
        public override void Run()
        {
            base.Run();
        }
        
        public override byte ComputeShade(ActiveSite site)
        {
            return 0;
        }
        protected override void AgeCohorts(ActiveSite site,
                                            ushort years,
                                            int? successionTimestep)
        {
            siteconditions[site].Reset();
             
            if (PlugIn.modelCore.CurrentTime != PlugIn.lastCurrentTime)
            {
                lastCurrentTime = PlugIn.modelCore.CurrentTime;
            }

            DateTime From = new DateTime(PlugIn.StartDate.Year + modelCore.CurrentTime - tstep, month, 15);
            DateTime To = new DateTime(PlugIn.StartDate.Year + modelCore.CurrentTime, month, 15);

            // Writes est file
            siteconditions[site].GrowCohorts(From, To, successionTimestep.HasValue);

            siteconditions[site].UpdateMaturePresent();

        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Makes a list of age cohorts in an initial community sorted from
        /// oldest to youngest.
        /// </summary>
         
        public static List<Landis.Library.AgeOnlyCohorts.ICohort> RankCohortAgesOldToYoung(List<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts> sppCohorts)
        {
            List<Landis.Library.AgeOnlyCohorts.ICohort> cohorts = new List<Landis.Library.AgeOnlyCohorts.ICohort>();
            foreach (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts speciesCohorts in sppCohorts)
            {
                foreach (Landis.Library.AgeOnlyCohorts.ICohort cohort in speciesCohorts)
                    cohorts.Add(cohort);
            }
            cohorts.Sort(Landis.Library.AgeOnlyCohorts.Util.WhichIsOlderCohort);
            return cohorts;
        }
        
        public void CohortDied(object sender,
                               Landis.Library.BiomassCohorts.DeathEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            if (disturbanceType != null)
            {
                ActiveSite site = eventArgs.Site;
                Disturbed[site] = true;
                if (disturbanceType.IsMemberOf("disturbance:fire"))
                    Reproduction.CheckForPostFireRegen(eventArgs.Cohort, site);
                else
                    Reproduction.CheckForResprouting(eventArgs.Cohort, site);
            }
        }
        
        public void AddNewCohort(ISpecies species, ActiveSite site)
        {
            //System.Console.WriteLine("AddNewCohort " + site + " " + species.Name);
            siteconditions[site].NewCohorts[species]++;

            Cohort cohort = new Cohort(species, 1, ConstantParameters.InitialFol, 0, ConstantParameters.InitialWood, 0, ConstantParameters.InitialRoot, year);

            siteconditions[site].Establishment.Establishments[species]++;
            

            if (HasSiteOutput[site] == true)
            {
                CohortOutput.WriteHeader(site, cohort);
            }

            siteconditions[site].AddNewCohort(cohort, PlugIn.TStep);
        }
        
        
        //---------------------------------------------------------------------
        public bool SufficientResources(ISpecies species, ActiveSite site)
        {
            return true;
        }
        public bool Establish(ISpecies species, ActiveSite site)
        {
          if (siteconditions[site].Establishment.PotEstablishments[species] == 0) return false;

          return true;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if there is a mature cohort at a site.  
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool MaturePresent(ISpecies species, ActiveSite site)
        {
            if (siteconditions[site].Cohorts == null) return false;

            bool ispresent = siteconditions[site].Cohorts.IsMaturePresent(species);

            return ispresent;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if a species can establish on a site.
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool PlantingEstablish(ISpecies species, ActiveSite site)
        {
            return true;
            //            IEcoregion ecoregion = modelCore.Ecoregion[site];
            //            int FirstYear = 0;
            //int LastYear = 0;
            //return SpeciesEstablishmentProbability2.ComputeEstablishmentProbability(ref FirstYear, ref LastYear, site, species, modelCore.Ecoregion[site]) > 0.0;
        }

    }
}


