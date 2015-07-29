//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller

using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Succession;
using Landis.Library.InitialCommunities;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System.IO;
using System;
using Landis.Library.Biomass;
namespace Landis.Extension.Succession.BiomassPnET
{
    public class PlugIn
        : Landis.Library.Succession.ExtensionBase
    {
        public static readonly string ExtensionName = "PnET Succession";
        public static ICore modelCore;
        private IInputParameters parameters;
        public static DateTime StartDate;
        private static int tstep;
        public static double CurrentYearSiteMortality;
        
        private static Landis.Library.Biomass.BaseCohortsSiteVar baseCohorts;
        private static ISiteVar<ISiteCohorts> cohorts;
        private static BiomassCohortsSiteVar BiomassCohorts;

        static Random r = new Random();


        static int year;
        static int month;
         

        public static int TStep
        {
            get
            {
                return tstep;
            }
        }
         
       private static ISiteVar<Landis.Library.Biomass.Species.AuxParm<int>> newcohorts;
           
        public static ISiteVar<Landis.Library.Biomass.Species.AuxParm<int>> NewCohorts
        {
            get
            {
                return newcohorts;
            }
        }
        
        //---------------------------------------------------------------------

        public PlugIn()
            : base(ExtensionName)
        {
        }

        //---------------------------------------------------------------------

        public override void LoadParameters(string dataFile, ICore mCore)
        {
            modelCore = mCore;
            InputParametersParser parser = new InputParametersParser();
            parameters = modelCore.Load<IInputParameters>(dataFile, parser);

         
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
            cohorts = PlugIn.ModelCore.Landscape.NewSiteVar<ISiteCohorts>();
            BiomassCohorts = new BiomassCohortsSiteVar(cohorts);
            baseCohorts = new Landis.Library.Biomass.BaseCohortsSiteVar(BiomassCohorts);

            // Counts added cohorts per site and per species
            newcohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Species.AuxParm<int>>();
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                newcohorts[site] = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.ModelCore.Species);
            }


            PlugIn.ModelCore.RegisterSiteVar(NewCohorts, "Succession.NewCohorts");
            PlugIn.ModelCore.RegisterSiteVar(cohorts, "Succession.BiomassCohortsPnET");
            PlugIn.ModelCore.RegisterSiteVar(BiomassCohorts, "Succession.BiomassCohorts");

            PlugIn.ModelCore.RegisterSiteVar(baseCohorts, "Succession.AgeCohorts");
            
            Edu.Wisc.Forest.Flel.Util.Directory.EnsureExists("output");
            CohortOutput.Initialize(parameters);
            SiteOutput.Initialize(parameters);
            CohortBiomass.Initialize(parameters);
            EstablishmentProbability.Initialize(parameters);
            CanopyBiomass.Initialize(parameters);
            Static.Initialize(parameters);
            Hydrology.Initialize(parameters);

            Timestep = parameters.Timestep;
            tstep = parameters.Timestep;

            
            
           
            // Initialize Reproduction routines:
           // Reproduction.SufficientResources = SufficientLight;
            Reproduction.Establish = Establish;
            Reproduction.AddNewCohort = AddNewCohort;
            Reproduction.MaturePresent = MaturePresent;
            Reproduction.PlantingEstablish = PlantingEstablish;
            base.Initialize(modelCore, parameters.SeedAlgorithm);

            InitialBiomass.Initialize(parameters);

            Cohort.DeathEvent += CohortDied;

            // TODO ARJAN !! 
            //Landis.Extension.Succession.Biomass.AgeOnlyDisturbances.Module.Initialize(parameters.AgeOnlyDisturbanceParms);

            ForestFloor.Initialize(parameters);

            SiteOutput.Initialize(parameters);

            

            StartDate = new System.DateTime(parameters.StartYear, 1, 15);

            year = StartDate.Year;
            month = StartDate.Month;

            InitializeSites(parameters.InitialCommunities, parameters.InitialCommunitiesMap, modelCore);

            
        }
        //---------------------------------------------------------------------
        protected override void InitializeSite(ActiveSite site,
                                               ICommunity initialCommunity)
        { 
            InitialBiomass initialBiomass = InitialBiomass.ComputeInitialBiomass(site, initialCommunity);

            
            if (initialBiomass == null)
            {
                PlugIn.cohorts[site] = new SiteCohorts();
                return;
            }

            PlugIn.Cohorts[site] = InitialBiomass.Clone(site, initialBiomass.Cohorts);
            ForestFloor.WoodyDebris[site] = initialBiomass.DeadWoodyPool.Clone();
            ForestFloor.Litter[site] = initialBiomass.DeadNonWoodyPool.Clone();
 
        }

        //---------------------------------------------------------------------
        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
            set
            {
                cohorts = value;
            }
        }
        public override void Run()
        {
            base.Run();
            

        }
        


        public override byte ComputeShade(ActiveSite site)
        {
            return 0;
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
        protected override void AgeCohorts(ActiveSite site,
                                           ushort years,
                                           int? successionTimestep)
        {
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
               PlugIn.NewCohorts[site][species] = 0;
               CanopyBiomass.DeadCohortAges[site][species].Clear();
               CanopyBiomass.DeadCohorts[site][species] = 0;
            }
            
            
            if (PlugIn.modelCore.CurrentTime != PlugIn.lastCurrentTime)
            {
                lastCurrentTime = PlugIn.modelCore.CurrentTime;
            }

            DateTime From = new DateTime(year, month, 15);
            DateTime To = From.AddYears(years);
            GrowCohorts(site, From, To,successionTimestep.HasValue);
        }
        
        public static void GrowCohorts(ActiveSite site,
                                       DateTime Time, 
                                       DateTime ToTime,
                                       bool issuccessionTimestep)
        {
            if (PlugIn.Cohorts[site] == null) return;

            while (Time.CompareTo(ToTime) < 0)
            {

                if (Time.Month == 1)
                {
                    if (issuccessionTimestep && tstep > 1)//&& ClimateDependentData.Data[PlugIn.Date[site]] != null
                    {
                        PlugIn.Cohorts[site].CombineCohorts(site, tstep, Time.Year);
                    }

                    CanopyBiomass.SubCanopyPARmax[site] = float.MinValue;
                    Hydrology.AnnualTranspiration[site] = 0;

                    //CanopyBiomass.DefineAgeLayers(site);
                    CanopyBiomass.DefineBiomassLayers(site);

                    CanopyBiomass.CanopyLAI[site] = 0;
                    PlugIn.Cohorts[site].IncrementCohortsAge();
                }

                Hydrology.UpdateSiteHydrology(Time,site);

                CanopyBiomass.SimulateCanopy(Time, site);

                if (issuccessionTimestep) EstablishmentProbability.Compute(Time,site);

                ForestFloor.Decomposition(site);

                SiteOutput.UpdateSiteData(Time,site);
                CanopyBiomass.GrossPsn[site] = 0;
                CanopyBiomass.NetPsn[site] = 0;
                CanopyBiomass.AutotrophicRespiration[site] = 0;
                ForestFloor.HeterotrophicRespiration[site] = 0;
                Hydrology.Transpiration[site] = 0;

                Time = Time.AddMonths(1);
            }
            CanopyBiomass.RemoveDeadCohorts(site);
            
            PlugIn.Cohorts[site].UpdateMaturePresent();

            SiteOutput.WriteSiteData(site);

            DisturbanceDefoliation.Defoliate(site);
        
        }

        static int lastCurrentTime;

        

        //---------------------------------------------------------------------
        /// <summary>
        /// Grows all cohorts at a site for a specified number of years.  The
        /// dead pools at the site also decompose for the given time period.
        /// </summary>
        
        public void AddNewCohort(ISpecies species, ActiveSite site)
        {
            PlugIn.NewCohorts[site][species]++;
            Cohort cohort = new Cohort(species, 1, ConstantParameters.InitialFol, 0,0, 0, 0, year, false);
            
            CohortOutput.WriteHeader(site, cohort);
            

            PlugIn.Cohorts[site].AddNewCohort(cohort);
           
        }
        
        
        //---------------------------------------------------------------------
        public bool Establish(ISpecies species, ActiveSite site)
        {
          if (EstablishmentProbability.Establishments[site][species] == 0) return false;

          return true;
        }

        //---------------------------------------------------------------------

        /// <summary>
        /// Determines if there is a mature cohort at a site.  
        /// This is a Delegate method to base succession.
        /// </summary>
        public bool MaturePresent(ISpecies species, ActiveSite site)
        {
            if (PlugIn.Cohorts[site] == null) return false;
            return PlugIn.Cohorts[site].IsMaturePresent(species);
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


