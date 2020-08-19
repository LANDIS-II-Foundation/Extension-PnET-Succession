//  Copyright ...
//  Authors:  Arjan de Bruijn

using Landis.Utilities;
using Landis.Core;
using Landis.Library.DensityCohorts.InitialCommunities;
using Landis.SpatialModeling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Landis.Library.Climate;
using Landis.Library.Cohorts;


namespace Landis.Library.DensityCohorts
{
    //public class SiteCohorts : ISiteCohorts, Landis.Library.DensityCohorts.ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    public class SiteCohorts : ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
        public ActiveSite Site;
        private Dictionary<ISpecies, List<Cohort>> cohorts = null;
        //Landis.Extension.Succession.BiomassPnET.IEstablishmentProbability establishmentProbability = null;
        public List<ISpecies> SpeciesEstablishedByPlant = null;
        public List<ISpecies> SpeciesEstablishedBySerotiny = null;
        public List<ISpecies> SpeciesEstablishedByResprout = null;
        public List<ISpecies> SpeciesEstablishedBySeed = null;

        public List<int> CohortsKilledBySuccession = null;
        public List<int> CohortsKilledByHarvest = null;
        public List<int> CohortsKilledByFire = null;
        public List<int> CohortsKilledByWind = null;
        public List<int> CohortsKilledByOther = null;
        public IEcoregion Ecoregion;

        static float[] AET = new float[12]; // mm/mo
        private static IDictionary<uint, SiteCohorts> initialSites;

        private static byte Timestep;
        //private static int CohortBinSize;

        /// <summary>
        /// Occurs when a site is disturbed by an age-only disturbance.
        /// </summary>
        //public static event Landis.Library.BiomassCohorts.DisturbanceEventHandler AgeOnlyDisturbanceEvent;

        //---------------------------------------------------------------------
        public List<ISpecies> SpeciesByPlant
        {
            get
            {
                return SpeciesEstablishedByPlant;
            }
            set
            {
                SpeciesEstablishedByPlant = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISpecies> SpeciesBySerotiny
        {
            get
            {
                return SpeciesEstablishedBySerotiny;
            }
            set
            {
                SpeciesEstablishedBySerotiny = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISpecies> SpeciesByResprout
        {
            get
            {
                return SpeciesEstablishedByResprout;
            }
            set
            {
                SpeciesEstablishedByResprout = value;
            }
        }
        //---------------------------------------------------------------------
        public List<ISpecies> SpeciesBySeed
        {
            get
            {
                return SpeciesEstablishedBySeed;
            }
            set
            {
                SpeciesEstablishedBySeed = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsBySuccession
        {
            get
            {
                return CohortsKilledBySuccession;
            }
            set
            {
                CohortsKilledBySuccession = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByHarvest
        {
            get
            {
                return CohortsKilledByHarvest;
            }
            set
            {
                CohortsKilledByHarvest = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByFire
        {
            get
            {
                return CohortsKilledByFire;
            }
            set
            {
                CohortsKilledByFire = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByWind
        {
            get
            {
                return CohortsKilledByWind;
            }
            set
            {
                CohortsKilledByWind = value;
            }
        }
        //---------------------------------------------------------------------
        public List<int> CohortsByOther
        {
            get
            {
                return CohortsKilledByOther;
            }
            set
            {
                CohortsKilledByOther = value;
            }
        }
        //---------------------------------------------------------------------
/*        public IEstablishmentProbability EstablishmentProbability 
        {
            get
            {
                return establishmentProbability;
            }
        }*/
        //---------------------------------------------------------------------

        public static void Initialize()
        {
            initialSites = new Dictionary<uint, SiteCohorts>();
            Timestep = Convert.ToByte(Cohorts.SuccessionTimeStep);
            /*Parameter<string> CohortBinSizeParm = null;
            if (PlugIn.TryGetParameter(Names.CohortBinSize, out CohortBinSizeParm))
            {
                if (!Int32.TryParse(CohortBinSizeParm.Value, out CohortBinSize))
                {
                    throw new System.Exception("CohortBinSize is not an integer value.");
                }
            }
            else
                CohortBinSize = Timestep;*/
        }

        public SiteCohorts(DateTime StartDate, ActiveSite site, Landis.Library.DensityCohorts.InitialCommunities.ICommunity initialCommunity, bool usingClimateLibrary, string SiteOutputName = null)
        {
            //Cohort.SetSiteAccessFunctions(this);
            this.Ecoregion = EcoregionData.ModelCore.Ecoregion[site];
            this.Site = site;
            
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            SpeciesEstablishedByPlant = new List<ISpecies>();
            SpeciesEstablishedBySerotiny = new List<ISpecies>();
            SpeciesEstablishedByResprout = new List<ISpecies>();
            SpeciesEstablishedBySeed = new List<ISpecies>();
            CohortsKilledBySuccession = new List<int>(new int[EcoregionData.ModelCore.Species.Count()]);
            CohortsKilledByHarvest = new List<int>(new int[EcoregionData.ModelCore.Species.Count()]);
            CohortsKilledByFire = new List<int>(new int[EcoregionData.ModelCore.Species.Count()]);
            CohortsKilledByWind = new List<int>(new int[EcoregionData.ModelCore.Species.Count()]);
            CohortsKilledByOther = new List<int>(new int[EcoregionData.ModelCore.Species.Count()]);

            uint key = ComputeKey((ushort)initialCommunity.MapCode, EcoregionData.ModelCore.Ecoregion[site].MapCode);

            if (initialSites.ContainsKey(key) && SiteOutputName == null)
            {
                foreach (ISpecies spc in initialSites[key].cohorts.Keys)
                {
                    foreach (Cohort cohort in initialSites[key].cohorts[spc])
                    {
                        AddNewCohort(new Cohort(cohort));
                    }
                }
            }
            else
            {
                if (initialSites.ContainsKey(key) == false)
                {
                    initialSites.Add(key, this);
                }


                bool densityProvided = false;
                foreach (Landis.Library.DensityCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                {
                    foreach (Landis.Library.DensityCohorts.ICohort cohort in speciesCohorts)
                    {
                        //FIXME
                        if (cohort.Treenumber > 0)  // 0 Biomass indicates treenumber value was not read in
                        {
                            densityProvided = true;
                            break;
                        }
                    }
                }

                if (densityProvided)
                {
                    foreach (Landis.Library.DensityCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                    {
                        //foreach (Landis.Library.DensityCohorts.ICohort cohort in speciesCohorts)
                        int cohortIndex = 0;
                        foreach (Landis.Library.DensityCohorts.ICohort cohort in speciesCohorts)
                        {
                            AddNewCohort(new Cohort(cohort.Species, cohort.Age, cohort.Treenumber, SiteOutputName, (ushort)(StartDate.Year - cohort.Age), Ecoregion));
                            //AddNewCohort(new Cohort(SpeciesParameters.SpeciesDensity[cohort.Species], cohort.Age, cohort.Treenumber, SiteOutputName, (ushort)(StartDate.Year - cohort.Age), Ecoregion));
                            //ISpeciesDensity speciespnet = PlugIn.SpeciesDensity[cohort.Species];

                            //SpeciesCohorts spCo = (SpeciesCohorts)speciesCohorts;
                            //CohortData coData = spCo.cohortData[0];

                            //coData.Biomass = cohort.Biomass;
                            cohortIndex++;
                        }
                        // BRM - Add function to update biomass values ??
                        SpeciesCohorts spCo = (SpeciesCohorts)speciesCohorts;
                        spCo.UpdateDiameterAndBiomass(Ecoregion);
                    }

                }
                else
                {
                    SpinUp(StartDate, site, initialCommunity, usingClimateLibrary, SiteOutputName);
                }
            }
        }

        // Spins up sites if no biomass is provided
        private void SpinUp(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, bool usingClimateLibrary, string SiteOutputName = null)
        {
                List<Landis.Library.AgeOnlyCohorts.ICohort> sortedAgeCohorts = new List<Landis.Library.AgeOnlyCohorts.ICohort>();
                foreach (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                {
                    foreach (Landis.Library.AgeOnlyCohorts.ICohort cohort in speciesCohorts)
                    {
                        sortedAgeCohorts.Add(cohort);
                    }
                }
                sortedAgeCohorts = new List<Library.AgeOnlyCohorts.ICohort>(sortedAgeCohorts.OrderByDescending(o => o.Age));

                if (sortedAgeCohorts.Count == 0) return;

                DateTime date = StartDate.AddYears(-(sortedAgeCohorts[0].Age));

                //Landis.Library.Parameters.Ecoregions.AuxParm<List<EcoregionClimateVariables>> mydata = new Library.Parameters.Ecoregions.AuxParm<List<EcoregionClimateVariables>>(PlugIn.ModelCore.Ecoregions);

                while (date.CompareTo(StartDate) < 0)
                {
                    //  Add those cohorts that were born at the current year
                    while (sortedAgeCohorts.Count() > 0 && StartDate.Year - date.Year == sortedAgeCohorts[0].Age)
                    {
                        Cohort cohort = new Cohort(SpeciesParameters.SpeciesDensity[sortedAgeCohorts[0].Species], (ushort)date.Year, SiteOutputName);

                        AddNewCohort(cohort);

                        sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                    }

                    // Simulation time runs untill the next cohort is added
                    DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - sortedAgeCohorts[0].Age), 1, 15);

                    //var climate_vars = usingClimateLibrary ? EcoregionPnET.GetClimateRegionData(Ecoregion, date, EndDate, Climate.Phase.SpinUp_Climate) : EcoregionPnET.GetData(Ecoregion, date, EndDate);

                    Grow(site, true);

                    date = EndDate;

                }
                if (sortedAgeCohorts.Count > 0) throw new System.Exception("Not all cohorts in the initial communities file were initialized.");
            }


        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        public bool Grow(ActiveSite site, bool isSuccessionTimestep)
        {
            
            SiteVars.TotalSiteRD(this);
            SiteDynamics.siteSuccession(this);

            if (isSuccessionTimestep && Landis.Library.DensityCohorts.Cohorts.SuccessionTimeStep > 1)
                foreach (SpeciesCohorts speciesCohorts in this)
                    speciesCohorts.CombineYoungCohorts();

            for (int i = 0; i < this.AllCohorts.Count; i++)
            {
                this.AllCohorts[i].IncrementAge();
                if (this.AllCohorts[i].Age >= this.AllCohorts[i].Species.Longevity || this.AllCohorts[i].Treenumber == 0)
                {
                    RemoveCohort(this.AllCohorts[i], null);

                }
            }

            bool success = true;

            return success;
        }

        //---------------------------------------------------------------------
        public Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent = new Library.Parameters.Species.AuxParm<bool>(EcoregionData.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    SpeciesPresent[spc] = true;
                }
                return SpeciesPresent;
            }
        }
        //---------------------------------------------------------------------

        public Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies = new Library.Parameters.Species.AuxParm<int>(EcoregionData.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    CohortCountPerSpecies[spc] = cohorts[spc].Count();
                }
                return CohortCountPerSpecies;
            }
        }
        //---------------------------------------------------------------------

        public Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges = new Library.Parameters.Species.AuxParm<List<ushort>>(EcoregionData.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    CohortAges[spc] = new List<ushort>(cohorts[spc].Select(o => o.Age));
                }
                return CohortAges;
            }
        }

        public int CohortCount
        {
            get
            {
                return cohorts.Values.Sum(o => o.Count());
            }
        }
        

        public int AverageAge 
        {
            get
            {
                return (int) cohorts.Values.Average(o => o.Average(x=>x.Age));
            }
        }

        public float AETSum
        {
            get
            {
                return AET.Sum();
            }
        }
        class SubCanopyComparer : IComparer<int[]>
        {
            // Compare second int (cumulative cohort biomass)
            public int Compare(int[] x, int[] y)
            {
                return (x[0] > y[0])? 1:-1;
            }
        }

        public double siteQMD
        {
            get
            {
                double basal = cohorts.Values.Sum(o => o.Sum(x => x.Diameter * x.Diameter * 0.00007854 / Math.Pow(EcoregionData.ModelCore.CellLength, 2) / 10000));
                double tph = cohorts.Values.Sum(o => o.Sum(x => x.Treenumber / Math.Pow(EcoregionData.ModelCore.CellLength, 2) / 10000));
                return Math.Sqrt((basal / tph) / 0.00007854);
            }
        }


        public static uint ComputeKey(uint a, ushort b)
        {
            uint value = (uint)((a << 16) | b);
            return value;
        }

        public List<Cohort> AllCohorts
        {
            get
            {
                List<Cohort> all = new List<Cohort>();
                foreach (ISpecies spc in cohorts.Keys)
                {
                    all.AddRange(cohorts[spc]);
                }
                return all;
            }
        }

        public int ReduceOrKillBiomassCohorts(IDisturbance disturbance)
        {
            List<int> reduction = new List<int>();

            List<Cohort> ToRemove = new List<Cohort>();
            
            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                //Landis.Library.DensityCohorts.SpeciesCohorts species_cohorts = GetSpeciesCohort(cohorts[species_cohort[0].Species]);
                foreach (Landis.Library.DensityCohorts.ICohort cohort in (IEnumerable<Landis.Library.DensityCohorts.ICohort>)species_cohort)
                //for (int c =0;c< species_cohort.Count(); c++)
                {
                    //FIXME - JSF
                    //Landis.Library.BiomassCohorts.ICohort biocohort = (Library.BiomassCohorts.ICohort) cohort;
                    //int _reduction = disturbance.ReduceOrKillMarkedCohort(biocohort);
                    // Disturbances return reduction in aboveground biomass
                    int _reduction = disturbance.ReduceOrKillMarkedCohort(cohort);
                    double reductionProp = _reduction / cohort.Biomass;
                    int treeRemoval = (int)Math.Round(cohort.Treenumber * reductionProp);
                    reduction.Add(_reduction);
                    if (reduction[reduction.Count() - 1] >= cohort.Treenumber)  //Compare to aboveground biomass
                    {
                        ToRemove.Add((Cohort) cohort);
                        // Edited by BRM - 090115
                    }
                    else
                    {
                        //FIXME compute treenumber reduction from disturbance reduction
                         //Proportion of aboveground biomass
                        cohort.ChangeTreenumber(-treeRemoval);  // Reduction applies to all biomass
                    }
                    //
                }
                
            }

            foreach (Cohort cohort in ToRemove)
            {
                RemoveCohort(cohort, disturbance.Type);
            }

            return reduction.Sum();
        }

        public int ReduceOrKillBiomassCohorts(Landis.Library.BiomassCohorts.IDisturbance disturbance)
        {
            List<int> reduction = new List<int>();

            List<Cohort> ToRemove = new List<Cohort>();

            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                //Landis.Library.DensityCohorts.SpeciesCohorts species_cohorts = GetSpeciesCohort(cohorts[species_cohort[0].Species]);
                foreach (Landis.Library.DensityCohorts.ICohort cohort in (IEnumerable<Landis.Library.DensityCohorts.ICohort>)species_cohort)
                //for (int c =0;c< species_cohort.Count(); c++)
                {
                    //FIXME - JSF
                    //Landis.Library.BiomassCohorts.ICohort biocohort = (Library.BiomassCohorts.ICohort) cohort;
                    //int _reduction = disturbance.ReduceOrKillMarkedCohort(biocohort);
                    // Disturbances return reduction in aboveground biomass
                    int _reduction = disturbance.ReduceOrKillMarkedCohort(cohort);
                    double reductionProp = (double)_reduction / (double)cohort.Biomass;
                    int treeRemoval = (int)Math.Round(cohort.Treenumber * reductionProp);
                    reduction.Add(treeRemoval);
                    if (reduction[reduction.Count() - 1] >= cohort.Treenumber)  //Compare to existing number of trees
                    {
                        ToRemove.Add((Cohort)cohort);
                        // Edited by BRM - 090115
                    }
                    else
                    {
                        //FIXME compute treenumber reduction from disturbance reduction
                        //Proportion of aboveground biomass
                        cohort.ChangeTreenumber(-treeRemoval);  // Reduction applies to all biomass
                    }
                    //
                }

            }

            foreach (Cohort cohort in ToRemove)
            {
                RemoveCohort(cohort, disturbance.Type);
            }

            return reduction.Sum();
        }

        public int AgeMax 
        {
            get
            {
                
                return (cohorts.Values.Count() > 0) ? cohorts.Values.Max(o => o.Max(x => x.Age)) : -1;

            }
        }

        public int ShadeMax
        {
            get
            {
                int maxShade = 0;
                foreach (ISpecies spc in cohorts.Keys)
                {
                    if (spc.ShadeTolerance > maxShade) maxShade = spc.ShadeTolerance;
                }
                return maxShade;
            }
        }

        Landis.Library.AgeOnlyCohorts.ISpeciesCohorts Landis.Library.Cohorts.ISiteCohorts<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.this[ISpecies species]
        {
            get
            {
                ISpeciesDensity speciespnet = SpeciesParameters.SpeciesDensity.AllSpecies[species.Index];
                if (cohorts.ContainsKey(speciespnet))
                {
                    return (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)GetSpeciesCohort(cohorts[speciespnet]);
                }
                return null;
            }
        }
        Landis.Library.BiomassCohorts.ISpeciesCohorts Landis.Library.Cohorts.ISiteCohorts<Landis.Library.BiomassCohorts.ISpeciesCohorts>.this[ISpecies species]
        {
            get
            {
                ISpeciesDensity speciespnet = SpeciesParameters.SpeciesDensity.AllSpecies[species.Index];
                if (cohorts.ContainsKey(speciespnet))
                {
                    return (Landis.Library.BiomassCohorts.ISpeciesCohorts)GetSpeciesCohort(cohorts[speciespnet]);
                }
                return null;
            }
        }

        public Landis.Library.DensityCohorts.ISpeciesCohorts this[ISpecies species]
        {
            get
            {
                ISpeciesDensity speciesdensity = SpeciesParameters.SpeciesDensity.AllSpecies[species.Index];
                if (cohorts.ContainsKey(speciesdensity))
                {
                    
                    return GetSpeciesCohort(cohorts[speciesdensity]);
                }
                return null;
                
            }
        }

        void Landis.Library.AgeOnlyCohorts.ISiteCohorts.RemoveMarkedCohorts(Landis.Library.AgeOnlyCohorts.ICohortDisturbance disturbance)
        {
            /*
            if (AgeOnlyDisturbanceEvent != null)
            {
                AgeOnlyDisturbanceEvent(this, new Landis.Library.BiomassCohorts.DisturbanceEventArgs(disturbance.CurrentSite, disturbance.Type));
            }
             */

            //FIXME - JSF
            //ReduceOrKillBiomassCohorts(new Landis.Library.BiomassCohorts.WrappedDisturbance(disturbance));
            ReduceOrKillBiomassCohorts(new Landis.Library.DensityCohorts.WrappedDisturbance(disturbance));
        }

        void Landis.Library.AgeOnlyCohorts.ISiteCohorts.RemoveMarkedCohorts(Landis.Library.AgeOnlyCohorts.ISpeciesCohortsDisturbance disturbance)
        {
            /*
            if (AgeOnlyDisturbanceEvent != null)
            {
                AgeOnlyDisturbanceEvent(this, new Landis.Library.BiomassCohorts.DisturbanceEventArgs(disturbance.CurrentSite, disturbance.Type));
            }
            */

            // Does this only occur when a site is disturbed?
            //Allocation.ReduceDeadPools(this, disturbance.Type); 

            //  Go through list of species cohorts from back to front so that
            //  a removal does not mess up the loop.
            int totalReduction = 0;

            List<Cohort> ToRemove = new List<Cohort>();

            Landis.Library.AgeOnlyCohorts.SpeciesCohortBoolArray isSpeciesCohortDamaged = new Landis.Library.AgeOnlyCohorts.SpeciesCohortBoolArray();

            foreach (ISpecies spc in cohorts.Keys)
            {
                Landis.Library.DensityCohorts.SpeciesCohorts speciescohort = GetSpeciesCohort(cohorts[spc]);

                isSpeciesCohortDamaged.SetAllFalse(speciescohort.Count);

                disturbance.MarkCohortsForDeath(speciescohort, isSpeciesCohortDamaged);

                for (int c = 0; c < isSpeciesCohortDamaged.Count; c++)
                {
                    if (isSpeciesCohortDamaged[c])
                    {
                        totalReduction += speciescohort[c].Treenumber;

                        ToRemove.Add(cohorts[spc][c]);
//                        ToRemove.AddRange(cohorts[spc].Where(o => o.Age == speciescohort[c].Age));
                    }
                }

            }
            foreach (Cohort cohort in ToRemove)
            {
                Landis.Library.DensityCohorts.Cohort.KilledByAgeOnlyDisturbance(disturbance, cohort, disturbance.CurrentSite, disturbance.Type);
                RemoveCohort(cohort, disturbance.Type);
            }
        }


        public void RemoveCohort(Cohort cohort, ExtensionType disturbanceType)
        {
            cohorts[cohort.Species].Remove(cohort);

            if (cohorts[cohort.Species].Count == 0)
            {
                cohorts.Remove(cohort.Species);
            }
            Cohort.Died(this, cohort, Site, disturbanceType);

        }

        public bool IsMaturePresent(ISpecies species)
        {
            ISpeciesDensity densitySpecies = SpeciesParameters.SpeciesDensity[species];

            int temp = densitySpecies.MaxSDI;

            bool speciesPresent = cohorts.ContainsKey(densitySpecies);

            bool IsMaturePresent = (speciesPresent && (cohorts[densitySpecies].Max(o => o.Age) >= species.Maturity)) ? true : false;

            return IsMaturePresent;
        }

        public Landis.Library.Parameters.Species.AuxParm<int> SpeciesSeed
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> SpeciesSeed = new Library.Parameters.Species.AuxParm<int>(EcoregionData.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    SpeciesSeed[spc] = cohorts[spc].Sum(o => o.Treenumber);
                }
                return SpeciesSeed;
            }
        }

        public void AddNewCohort(Cohort newCohort)
        {
            if (cohorts.ContainsKey(newCohort.Species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies, List<Cohort>>>(cohorts.Where(o => o.Key == newCohort.Species))[0];

                //List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < CohortBinSize));
                List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < Timestep));

                if(Cohorts.Count > 1)
                {
                    foreach(Cohort Cohort in Cohorts.Skip(1))
                    {
                        newCohort.Accumulate(Cohort);
                    }
                }                

                if (Cohorts.Count() > 0)
                {
                    Cohorts[0].Accumulate(newCohort);
                    return;
                }

                cohorts[newCohort.Species].Add(newCohort);

                return;
            }
            cohorts.Add(newCohort.Species, new List<Cohort>(new Cohort[] { newCohort }));
        }

        Landis.Library.DensityCohorts.SpeciesCohorts GetSpeciesCohort(List<Cohort> cohorts)
        {
            Landis.Library.DensityCohorts.SpeciesCohorts spc = new Library.DensityCohorts.SpeciesCohorts(cohorts[0].Species, cohorts[0].Age, cohorts[0].Treenumber, this.Ecoregion);

            for (int c = 1; c < cohorts.Count; c++)
            {
                spc.AddNewCohort(cohorts[c].Age, cohorts[c].Treenumber, this.Ecoregion);
            }

            spc.UpdateDiameterAndBiomass(this.Ecoregion);
            return spc;
        }
        
 
        public IEnumerator<Landis.Library.DensityCohorts.ISpeciesCohorts> GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return (Library.DensityCohorts.ISpeciesCohorts) this[species];
            }
        }
       
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.BiomassCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                Landis.Library.DensityCohorts.SpeciesCohorts speciescohort = GetSpeciesCohort(cohorts[species]);
                Landis.Library.BiomassCohorts.ISpeciesCohorts isp = (Landis.Library.BiomassCohorts.ISpeciesCohorts)speciescohort;
                yield return isp;
            }
             
        }

        IEnumerator<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                Landis.Library.DensityCohorts.SpeciesCohorts speciescohort = GetSpeciesCohort(cohorts[species]);
                Landis.Library.AgeOnlyCohorts.ISpeciesCohorts isp = (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)speciescohort;
                yield return isp;
            }

             
        }
       

    }


}

