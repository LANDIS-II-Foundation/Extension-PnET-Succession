//  Copyright ...
//  Authors:  Arjan de Bruijn

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
    public class SiteConditions
    {
        private static Random rng = new Random();

        private SiteOutput siteoutput=null;
        private EstablishmentOutput estoutput = null;
        public Hydrology hydrology;
        public ForestFloor forestfloor;
        private SiteCohorts cohorts;
        Canopy canopy = null;
        

        public bool HasSiteOutput
        { 
            get
            {
                if (siteoutput != null) return true;
                return false;
            }
        }
         
        private ActiveSite site;

        public ActiveSite Site { get { return site; } }

       
        private Landis.Library.Biomass.Species.AuxParm<List<int>> deadcohortages = new Landis.Library.Biomass.Species.AuxParm<List<int>>(PlugIn.modelCore.Species);
        Landis.Library.Biomass.Species.AuxParm<int> deadcohorts = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.modelCore.Species);
        Landis.Library.Biomass.Species.AuxParm<int> newcohorts = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.modelCore.Species);
        EstablishmentProbability establishment;

        private float canopylaimax;
        
      
        private static int successionTimestep;

        public float AutotrophicRespiration { get { return canopy.AutotrophicRespiration; } }
        public float TotalRoot { get { return canopy.TotalRoot; } }
        public float TotalBiomass { get { return canopy.TotalBiomass; } }
        public float TotalFoliage { get { return canopy.TotalFoliage; } }
        public float TotalNSC { get { return canopy.TotalNSC; } }
        public int NrOfCohorts { get { return canopy.NrOfCohorts; } }
        
        public float GrossPsn { get { return canopy.GrossPsn; } }
        public float NetPsn { get { return canopy.NetPsn; } }
        public float SubCanopyPARmax { get { return canopy.SubCanopyPARmax; } }
        public float SubCanopyPAR { get { return canopy.SubCanopyPAR; } }
        public Landis.Library.Biomass.Species.AuxParm<List<int>> DeadCohortAges { get { return deadcohortages; } }
        public float CanopyLAImax { get { return canopylaimax; } }
        public SiteCohorts Cohorts {     get     {      return cohorts;     }  }
        public Pool WoodyDebris  {  get     {      return forestfloor.WoodyDebris;     }    }
        public Pool Litter  {   get    {    return forestfloor.Litter;    }   }
        public Landis.Library.Biomass.Species.AuxParm<int> DeadCohorts   {     get     {   return deadcohorts; }   }
        public Landis.Library.Biomass.Species.AuxParm<int> NewCohorts  {  get  {  return newcohorts;  }}
        public EstablishmentProbability Establishment { get { return establishment;  } }

        
        
        public void Reset()
        {
           
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                newcohorts[species] = 0;
                deadcohortages[species].Clear();
                deadcohorts[species] = 0;
                establishment.PotEstablishments[species] = 0;
                establishment.Establishments[species] = 0;
            }
        }
        //---------------------------------------------------------------------
        /// <summary>
        /// A method that computes the initial biomass for a new cohort at a
        /// site based on the existing cohorts.
        /// </summary>
        public delegate int ComputeMethod(ISpecies species,
                                             ISiteCohorts ISiteCohorts,
                                             ActiveSite site);


        private static uint ComputeKey(uint initCommunityMapCode,
                                       ushort ecoregionMapCode)
        {
            return (uint)((initCommunityMapCode << 16) | ecoregionMapCode);
        }


        private static IDictionary<uint, SiteConditions> initialSites;
        //  Initial site biomass for each unique pair of initial
        //  community and ecoregion; Key = 32-bit unsigned integer where
        //  high 16-bits is the map code of the initial community and the
        //  low 16-bits is the ecoregion's map code

        private static IDictionary<uint, List<Landis.Library.AgeOnlyCohorts.ICohort>> sortedCohorts;
        //  Age cohorts for an initial community sorted from oldest to
        //  youngest.  Key = initial community's map code

        public static void Initialize(IInputParameters parameters )
        {
            initialSites = new Dictionary<uint, SiteConditions>();
            sortedCohorts = new Dictionary<uint, List<Landis.Library.AgeOnlyCohorts.ICohort>>();
            successionTimestep = (ushort)parameters.Timestep;
        }
        public SiteConditions GetFromKey(uint key)
        { 
            SiteConditions s=null;
            if (initialSites.TryGetValue(key, out s) && siteoutput == null)
            {
                hydrology = s.hydrology;
                establishment = s.Establishment;

                cohorts = new SiteCohorts();
                foreach (ISpeciesCohorts speciesCohorts in s.cohorts)
                {
                    foreach (Cohort cohort in speciesCohorts)
                    {
                        Cohort newcohort = new Cohort(cohort);
                        cohorts.AddNewCohort(newcohort, PlugIn.TStep);
                    }
                }
                forestfloor = s.forestfloor;
                canopylaimax = s.CanopyLAImax;
            }
            return s;
        }
        public SiteConditions (ActiveSite site, ICommunity initialCommunity)
        {
            cohorts = new SiteCohorts();

            canopy = new Canopy();
            if (PlugIn.HasSiteOutput[site] == true)
            {
                siteoutput = new SiteOutput(site);
                estoutput = new EstablishmentOutput(site);
            }
            
            this.site = site;
            

            foreach (ISpecies spc in PlugIn.modelCore.Species)
            {
                deadcohortages[spc] = new List<int>();
            }

            uint key = ComputeKey(initialCommunity.MapCode, PlugIn.ModelCore.Ecoregion[site].MapCode);

            SiteConditions s = GetFromKey(key);

            if (s != null) return; 

            //  If we don't have a sorted list of age cohorts for the initial
            //  community, make the list
             
            List<Landis.Library.AgeOnlyCohorts.ICohort> sortedAgeCohorts;
            if (!sortedCohorts.TryGetValue(initialCommunity.MapCode, out sortedAgeCohorts))
            {
                sortedAgeCohorts = PlugIn.RankCohortAgesOldToYoung(initialCommunity.Cohorts);
                sortedCohorts[initialCommunity.MapCode] = sortedAgeCohorts;
            }
            hydrology = new Hydrology(PlugIn.modelCore.Ecoregion[site]);
            forestfloor = new ForestFloor();
            cohorts = new SiteCohorts();
            establishment = new EstablishmentProbability(site);
             
             
            if (sortedAgeCohorts.Count == 0) return;

            //PlugIn.ModelCore.UI.WriteLine("Making Biomass Cohorts "+ site);
            BiomassSpinUp(sortedAgeCohorts, site);

            initialSites[key] = this;
            return;
        }
        public void BiomassSpinUp(List<Landis.Library.AgeOnlyCohorts.ICohort> ageCohorts, ActiveSite site)
        {
            if (ageCohorts.Count == 0)
            {
                return;
            }
            System.DateTime SpinUpDate;

            // OLD: SpinUpDate = PlugIn.StartDate.AddYears(-(ageCohorts[0].Age) + ((successionTimestep == 1) ? -1 : 0));

            SpinUpDate = PlugIn.StartDate.AddYears(-(ageCohorts[0].Age));

            while (SpinUpDate.CompareTo(PlugIn.StartDate) < 0)
            {
                //  Add those cohorts that were born at the current year
                foreach (Library.AgeOnlyCohorts.ICohort ageonlycohort in ageCohorts)
                {
                    if (PlugIn.StartDate.Year - SpinUpDate.Year == ageonlycohort.Age)
                    {
                        Cohort cohort = new Cohort(ageonlycohort.Species, 1, ConstantParameters.InitialFol, 0, ConstantParameters.InitialWood, ConstantParameters.InitialFol, ConstantParameters.InitialRoot, SpinUpDate.Year);

                        AddNewCohort(cohort, PlugIn.TStep);

                        if (siteoutput != null)
                        {
                            CohortOutput.WriteHeader(site, cohort);
                        }
                    }
                }
                GrowCohorts(SpinUpDate, SpinUpDate.AddYears(1), false);

                SpinUpDate = SpinUpDate.AddYears(1);
            }
        }
        
       
        
        public void GrowCohorts(DateTime date,
                                DateTime ToTime,
                                bool issuccessionTimestep)
        {
            
            if (Cohorts == null) return;

             
            while (date.CompareTo(ToTime) < 0)
            {
                PlugIn.SetYear(date.Year);

                if (date.Month == (int)Months.January || canopy ==null)
                {
                    canopylaimax = 0;

                    if (cohorts == null)return;
                }
                if (date.Month == (int)Months.June) Defoliate(site);

                hydrology.UpdateSiteHydrology(date);

                canopy.Grow(date, site, hydrology, forestfloor, cohorts, siteoutput);

                canopylaimax = canopy.CanopyLAImax;

                foreach(Cohort cohort in canopy.DeadCohorts)
                {
                   DeadCohortAges[cohort.Species].Add(cohort.Age);
                   deadcohorts[cohort.Species]++;
                   Cohorts[cohort.Species].RemoveCohort(cohort, site, null);

                   canopy.ResetDeadCohorts();
                }
 
                if (issuccessionTimestep)
                {
                    Establishment.ComputeEstablishment(date, this);
 
                    if (HasSiteOutput)
                    {
                        estoutput.UpdateEstData(date, this);
                    }
                }

                if (HasSiteOutput)
                {
                    siteoutput.UpdateSiteData(date, this);
                    siteoutput.WriteSiteData();
                }
                if (date.Month == (int)Months.December)
                {
                    Cohorts.IncrementCohortsAge();

                    forestfloor.Decompose();
                    forestfloor.Decompose();


                }
                date = date.AddMonths(1);
            }
            if (CohortBiomass.HasDeadCohorts)
            {
                CohortBiomass.HasDeadCohorts = false;
            }

        }
        public void UpdateMaturePresent()
        {
            foreach (SpeciesCohorts s in cohorts)
            {
                s.UpdateMaturePresent();
            }
        }
        public void Defoliate(ActiveSite site)
        {

            //if (insectdefoliation == null) return;
            // ---------------------------------------------------------
            // Defoliation ranges from 1.0 (total) to none (0.0).
            // Defoliation is calculated by an external function, typically an extension
            // with a defoliation calculator.  The method CohortDefoliation.Compute is a delegate method
            // and lives within the defoliating extension.

            foreach (ISpeciesCohorts speciesCohorts in Cohorts)
            {
                foreach (ICohort cohort in speciesCohorts)
                {
                    double defoliation = Math.Min(1, Landis.Library.Biomass.CohortDefoliation.Compute(site, cohort.Species, cohort.Biomass, canopy.MaxBiom));

                    if (defoliation > 0)
                    {
                        cohort.Fol *= (1 - (float)defoliation);
                    }

                }
            }

              
        }
        public void AddNewCohort(Cohort cohort, int SuccessionTimeStep)
        {
            cohorts.AddNewCohort(cohort, SuccessionTimeStep);
        }
         
        
        
       
        
    }


}

