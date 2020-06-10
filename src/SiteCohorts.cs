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
using Landis.Library.DensityCohorts;
using Landis.Library.Cohorts;
//using Landis.Library.BiomassCohorts;

namespace Landis.Extension.Succession.BiomassPnET
{
    //public class SiteCohorts : ISiteCohorts, Landis.Library.DensityCohorts.ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    public class SiteCohorts : ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
        public ActiveSite Site;
        private Dictionary<ISpecies, List<Cohort>> cohorts = null;
        IEstablishmentProbability establishmentProbability = null;
        public List<ISpecies> SpeciesEstablishedByPlant = null;
        public List<ISpecies> SpeciesEstablishedBySerotiny = null;
        public List<ISpecies> SpeciesEstablishedByResprout = null;
        public List<ISpecies> SpeciesEstablishedBySeed = null;

        public List<int> CohortsKilledBySuccession = null;
        public List<int> CohortsKilledByHarvest = null;
        public List<int> CohortsKilledByFire = null;
        public List<int> CohortsKilledByWind = null;
        public List<int> CohortsKilledByOther = null;

        public IEcoregionPnET Ecoregion;
        public LocalOutput siteoutput;

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
        public IEstablishmentProbability EstablishmentProbability 
        {
            get
            {
                return establishmentProbability;
            }
        }
        //---------------------------------------------------------------------

        public static void Initialize()
        {
            initialSites = new Dictionary<uint, SiteCohorts>();
            Timestep = ((Parameter<byte>)PlugIn.GetParameter(Names.Timestep)).Value;
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
            this.Ecoregion = EcoregionPnET.GetPnETEcoregion(PlugIn.ModelCore.Ecoregion[site]);
            this.Site = site;
            
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            SpeciesEstablishedByPlant = new List<ISpecies>();
            SpeciesEstablishedBySerotiny = new List<ISpecies>();
            SpeciesEstablishedByResprout = new List<ISpecies>();
            SpeciesEstablishedBySeed = new List<ISpecies>();
            CohortsKilledBySuccession = new List<int>(new int[PlugIn.ModelCore.Species.Count()]);
            CohortsKilledByHarvest = new List<int>(new int[PlugIn.ModelCore.Species.Count()]);
            CohortsKilledByFire = new List<int>(new int[PlugIn.ModelCore.Species.Count()]);
            CohortsKilledByWind = new List<int>(new int[PlugIn.ModelCore.Species.Count()]);
            CohortsKilledByOther = new List<int>(new int[PlugIn.ModelCore.Species.Count()]);

            uint key = ComputeKey((ushort)initialCommunity.MapCode, PlugIn.ModelCore.Ecoregion[site].MapCode);

            if (initialSites.ContainsKey(key) && SiteOutputName == null)
            {
                //PlugIn.WoodyDebris[Site] = PlugIn.WoodyDebris[initialSites[key].Site].Clone();
                //PlugIn.Litter[Site] = PlugIn.Litter[initialSites[key].Site].Clone();
                //PlugIn.FineFuels[Site] = PlugIn.Litter[Site].Mass;

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
                //List<IEcoregionClimateVariables> ecoregionInitializer = EcoregionPnET.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));

                //PlugIn.WoodyDebris[Site] = new Library.Biomass.Pool();
                //PlugIn.Litter[Site] = new Library.Biomass.Pool();
                //PlugIn.FineFuels[Site] = PlugIn.Litter[Site].Mass;

                /*if (SiteOutputName != null)
                {
                    this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site));
                }
               */



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
                            // FIXME - feeds in age, treenumber, diameter - placeholder 1 for diameter
                            // The biomass value actually represents treenumber
                            AddNewCohort(new Cohort(PlugIn.SpeciesDensity[cohort.Species], cohort.Age, cohort.Treenumber, SiteOutputName, (ushort)(StartDate.Year - cohort.Age), Ecoregion));
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
                        Cohort cohort = new Cohort(PlugIn.SpeciesDensity[sortedAgeCohorts[0].Species], (ushort)date.Year, SiteOutputName);

                        AddNewCohort(cohort);

                        sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                    }

                    // Simulation time runs untill the next cohort is added
                    DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - sortedAgeCohorts[0].Age), 1, 15);

                    //var climate_vars = usingClimateLibrary ? EcoregionPnET.GetClimateRegionData(Ecoregion, date, EndDate, Climate.Phase.SpinUp_Climate) : EcoregionPnET.GetData(Ecoregion, date, EndDate);

                    Grow();

                    date = EndDate;

                }
                if (sortedAgeCohorts.Count > 0) throw new System.Exception("Not all cohorts in the initial communities file were initialized.");
            }

       /* List<List<int>> GetRandomRange(List<List<int>> bins)
        {
            List<List<int>> random_range = new List<List<int>>();
            if (bins != null) for (int b = 0; b < bins.Count(); b++)
                {
                    random_range.Add(new List<int>());

                    List<int> copy_range = new List<int>(bins[b]);

                    while (copy_range.Count() > 0)
                    {
                        int k = PlugIn.DiscreteUniformRandom(0, copy_range.Count()-1);
                        random_range[b].Add(copy_range[k]);

                        copy_range.RemoveAt(k);
                    }
                }
            return random_range;
        }*/

        /*public void SetAet(float value, int Month)
        {
            AET[Month-1] = value;
        }*/

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        public bool Grow()
        {

            //FIXME - grow and age the cohorts

            // Testing access to cohort attributes
            Cohort.SetSiteAccessFunctions(this);
            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                float checkDiameter = AllCohorts[cohort].Diameter;
                int checkAge = AllCohorts[cohort].Age;
                float checkDensity = AllCohorts[cohort].Treenumber;
                ISpeciesDensity speciespnet = PlugIn.SpeciesDensity.AllSpecies[AllCohorts[cohort].Species.Index];
                int SDI = speciespnet.MaxSDI;
                int checkBiomass = AllCohorts[cohort].Biomass;
            }
            foreach (Landis.Library.DensityCohorts.SpeciesCohorts speciesCohorts in this)
            {
                SiteVars.TotalSiteRD(speciesCohorts, this.Site);
            }

            //SiteDynamics(this);
            int shadeMax = ShadeMax;
            double GSO1 = EcoregionPnET.GSO1[this.Ecoregion];
            bool success = true;
            
            return success;
        }      

        /*public double WoodyDebris 
        {
            get
            {
                return PlugIn.WoodyDebris[Site].Mass;
            }
        }*/

        /*public double Litter 
        {
            get
            {
                return PlugIn.Litter[Site].Mass;
            }
        }*/
       
        public  Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<bool> SpeciesPresent = new Library.Parameters.Species.AuxParm<bool>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    SpeciesPresent[spc] = true;
                }
                return SpeciesPresent;
            }
        }

        /*public Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    BiomassPerSpecies[spc] = cohorts[spc].Sum(o => o.TotalBiomass);
                }
                return BiomassPerSpecies;
            }
        }*/
        //public Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies
        //{
        //    get
        //    {
        //        Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

        //        foreach (ISpecies spc in cohorts.Keys)
        //        {
        //            AbovegroundBiomassPerSpecies[spc] = cohorts[spc].Sum(o => o.Biomass);
        //        }
        //        return AbovegroundBiomassPerSpecies;
        //    }
        //}
        /*public Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    WoodySenescencePerSpecies[spc] = cohorts[spc].Sum(o => o.LastWoodySenescence);
                }
                return WoodySenescencePerSpecies;
            }
        }*/
        /*public Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    FoliageSenescencePerSpecies[spc] = cohorts[spc].Sum(o => o.LastFoliageSenescence);
                }
                return FoliageSenescencePerSpecies;
            }
        }*/
        public Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> CohortCountPerSpecies = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    CohortCountPerSpecies[spc] = cohorts[spc].Count();
                }
                return CohortCountPerSpecies;
            }
        }

        public Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<List<ushort>> CohortAges = new Library.Parameters.Species.AuxParm<List<ushort>>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    CohortAges[spc] = new List<ushort>(cohorts[spc].Select(o => o.Age));
                }
                return CohortAges;
            }
        }

        /*public float BiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => o.TotalBiomass);
            }

        }*/

        //public float AbovegroundBiomassSum
        //{
        //    get
        //    {
        //        return AllCohorts.Sum(o => o.Biomass);
        //    }

        //}
        /*public float WoodySenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => o.LastWoodySenescence);
            }

        }*/
       /* public float FoliageSenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => o.LastFoliageSenescence);
            }

        }*/
        /*public uint BelowGroundBiomass 
        {
            get
            {
                return (uint)cohorts.Values.Sum(o => o.Sum(x => x.Root));
            }
        }*/

        /*public float FoliageSum
        {
            get
            {
                return AllCohorts.Sum(o => o.Fol);
            }

        }*/

        /*public float NSCSum
        {
            get
            {
                return AllCohorts.Sum(o => o.NSC);
            }

        }*/
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

        /*private SortedDictionary<int[], Cohort> GetSubcanopyLayers()
        {
            SortedDictionary<int[], Cohort> subcanopylayers = new SortedDictionary<int[], Cohort>(new SubCanopyComparer());

            foreach (Cohort cohort in AllCohorts)
            {
                for (int i = 0; i < PlugIn.IMAX; i++)
                {
                    int[] subcanopplayer = new int[] { (ushort)((i + 1) / (float)PlugIn.IMAX * cohort.BiomassMax) };
                    subcanopylayers.Add(subcanopplayer, cohort);
                }
            }
            return subcanopylayers;
        }*/

        /*private static int[] GetNextBinPositions(int[] index_in, int numcohorts)
        {
            
            for (int index = index_in.Length - 1; index >= 0; index--)
            {
                int maxvalue = numcohorts - index_in.Length + index - 1;
                if (index_in[index] < maxvalue)
                {
                    {
                        index_in[index]++;

                        for (int i = index+1; i < index_in.Length; i++)
                        {
                            index_in[i] = index_in[i - 1] + 1;
                        }
                        return index_in;
                    }

                }
                /*
                else 
                {
                    if (index == 0) return null;

                    index_in[index - 1]++;
 
                    for (int i = index; i < index_in.Length; i++)
                    {
                        index_in[i] = index_in[index - 1] + i;
                    }
                     
                }
                 *//*
            }
            return null;
        }*/
      
        /*private int[] GetFirstBinPositions(int nlayers, int ncohorts)
        {
            int[] Bin = new int[nlayers - 1];

            for (int ly = 0; ly < Bin.Length; ly++)
            {
                Bin[ly] = ly+1;
            }
            return Bin;
        }*/

        /*public static List<T> Shuffle<T>(List<T> array)
        {
            int n = array.Count;
            while (n > 1)
            {
                int k = PlugIn.DiscreteUniformRandom(0, n);
                n--;

                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
            return array;
        }*/

        /*uint CalculateLayerstdev(List<double> f)
        {
            return (uint)Math.Max(Math.Abs(f.Max() - f.Average()), Math.Abs(f.Min() - f.Average()));

        }*/

        /*int[] MinMaxCohortNr(int[] Bin, int i, int Count)
        {
            int min = (i > 0) ? Bin[i - 1] : 0;
            int max = (i < Bin.Count()) ? Bin[i] : Count - 1;

            return new int[] { min, max };
        }*/

        //static List<uint> layerstdev = new List<uint>();

        /*private List<List<int>> GetBins(List<double> CumCohortBiomass)
        {
            nlayers = 0;
            layerstdev.Clear();
            if (CumCohortBiomass.Count() == 0)
            {                
                return null;
            }

            // Bin and BestBin are lists of indexes that determine what cohort belongs to what canopy layer, 
            // i.e. when Bin[1] contains 45 then SubCanopyCohorts[45] is in layer 1
            int[] BestBin = null;
            int[] Bin = null;
                       

            float LayerStDev = float.MaxValue;

            //=====================OPTIMIZATION LOOP====================================
            do
            {
                nlayers++;
                

                Bin = GetFirstBinPositions(nlayers, CumCohortBiomass.Count());

                while (Bin != null)
                {
                    layerstdev.Clear();

                    if (Bin.Count() == 0)
                    {
                        layerstdev.Add(CalculateLayerstdev(CumCohortBiomass));
                    }
                    else for (int i = 0; i <= Bin.Count(); i++)
                    {
                        int[] MinMax = MinMaxCohortNr(Bin, i, CumCohortBiomass.Count());

                        // Get the within-layer variance in biomass
                        layerstdev.Add(CalculateLayerstdev(CumCohortBiomass.GetRange(MinMax[0], MinMax[1] - MinMax[0])));
                    }

                    // Keep the optimal (min within-layer variance) layer setting
                    if (layerstdev.Max() < LayerStDev)
                    {
                        BestBin = new List<int>(Bin).ToArray();
                        LayerStDev = layerstdev.Max();
                    }
                    Bin = GetNextBinPositions(Bin, CumCohortBiomass.Count());

                }
            }
            while (layerstdev.Max() >= MaxDevLyrAv && nlayers < MaxCanopyLayers && nlayers < (CumCohortBiomass.Count()/PlugIn.IMAX));
            //=====================OPTIMIZATION LOOP====================================


            // Actual layer configuration
            List<List<int>> Bins = new List<List<int>>();
            if (BestBin.Count() == 0)
            {
                // One canopy layer
                Bins.Add(new List<int>());
                for (int i = 0; i < CumCohortBiomass.Count(); i++)
                {
                    Bins[0].Add(i);
                }
            }
            else for (int i = 0; i <= BestBin.Count(); i++)
            {
                // Multiple canopy layers
                Bins.Add(new List<int>());

                int[] minmax = MinMaxCohortNr(BestBin, i, CumCohortBiomass.Count());

                // Add index numbers to the Bins array
                for (int a = minmax[0]; a < ((i == BestBin.Count()) ? minmax[1]+1 : minmax[1]); a++)
                {
                    Bins[i].Add(a);
                }
            }
            return Bins;
        }*/

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

        public int ReduceOrKillBiomassCohorts(Landis.Library.BiomassCohorts.IDisturbance disturbance)
        {
            List<int> reduction = new List<int>();

            List<Cohort> ToRemove = new List<Cohort>();
            
            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                Landis.Library.DensityCohorts.SpeciesCohorts species_cohorts = GetSpeciesCohort(cohorts[species_cohort[0].Species]);
                
                for (int c =0;c< species_cohort.Count(); c++)
                {
                    Landis.Library.BiomassCohorts.ICohort cohort = (Library.BiomassCohorts.ICohort) species_cohort[c];

                    // Disturbances return reduction in aboveground biomass
                    int _reduction = disturbance.ReduceOrKillMarkedCohort(cohort);

                    reduction.Add(_reduction);
                    if (reduction[reduction.Count() - 1] >= species_cohort[c].Treenumber)  //Compare to aboveground biomass
                    {
                        ToRemove.Add(species_cohort[c]);
                        // Edited by BRM - 090115
                    }
                    else
                    {
                        double reductionProp = (double)reduction[reduction.Count() - 1] / (double)species_cohort[c].Treenumber;  //Proportion of aboveground biomass
                        //species_cohort[c].ReduceBiomass(this, reductionProp, disturbance.Type);  // Reduction applies to all biomass
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
                ISpeciesDensity speciespnet = PlugIn.SpeciesDensity.AllSpecies[species.Index];
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
                ISpeciesDensity speciespnet = PlugIn.SpeciesDensity.AllSpecies[species.Index];
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
                ISpeciesDensity speciespnet = PlugIn.SpeciesDensity.AllSpecies[species.Index];
                if (cohorts.ContainsKey(speciespnet))
                {
                    
                    return GetSpeciesCohort(cohorts[speciespnet]);
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
            ReduceOrKillBiomassCohorts(new Landis.Library.BiomassCohorts.WrappedDisturbance(disturbance));

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

        // FIXME - I don't think we need this function
        /*private void RemoveMarkedCohorts()
        {
            for (int c = cohorts.Values.Count - 1; c >= 0; c--)
            {
                List<Cohort> species_cohort = cohorts.Values.ElementAt(c);

                for (int cc = species_cohort.Count - 1; cc >= 0; cc--)
        {
                    if (species_cohort[cc].IsAlive == false)
                    {
                        RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));
                    }
                }
            }
        }*/

        // with cold temp killing cohorts - now moved to within CalculatePhotosynthesis function
        /*private void RemoveMarkedCohorts(float minMonthlyAvgTemp, float winterSTD)
        {

            for (int c = cohorts.Values.Count - 1; c >= 0; c--)
            {
                List<Cohort> species_cohort = cohorts.Values.ElementAt(c);

                for (int cc = species_cohort.Count - 1; cc >= 0; cc--)
                {
                    if (species_cohort[cc].IsAlive == false)
                    {
                      
                        RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));

                    }
                    else
                    {
                        if(permafrost)
                        {
                            // Check if low temp kills cohorts
                            if((minMonthlyAvgTemp - (3.0 * winterSTD)) < species_cohort[cc].SpeciesPNET.ColdTol)
                            {
                                RemoveCohort(species_cohort[cc], new ExtensionType(Names.ExtensionName));
                            }
                        }
                    }
                
                }
            }

        }*/

        public void RemoveCohort(Cohort cohort, ExtensionType disturbanceType)
        {

            if(disturbanceType.Name == Names.ExtensionName)
            {
                CohortsKilledBySuccession[cohort.Species.Index] += 1;
            }
            else if(disturbanceType.Name == "disturbance:harvest")
            {
                CohortsKilledByHarvest[cohort.Species.Index] += 1;
            }
            else if(disturbanceType.Name == "disturbance:fire")
            {
                CohortsKilledByFire[cohort.Species.Index] += 1;
            }
            else if (disturbanceType.Name == "disturbance:wind")
            {
                CohortsKilledByWind[cohort.Species.Index] += 1;
            }
            else
            {
                CohortsKilledByOther[cohort.Species.Index] += 1;
            }

            if (disturbanceType.Name != Names.ExtensionName)
            {
                Cohort.RaiseDeathEvent(this, cohort, Site, disturbanceType);
            }

            cohorts[cohort.Species].Remove(cohort);

            if (cohorts[cohort.Species].Count == 0)
            {
                cohorts.Remove(cohort.Species);
            }

            Allocation.Allocate(this, cohort, disturbanceType, 1.0);  //Allocation fraction is 1.0 for complete removals

        }

        public bool IsMaturePresent(ISpecies species)
        {
            ISpeciesDensity pnetSpecies = PlugIn.SpeciesDensity[species];

            int temp = pnetSpecies.MaxSDI;

            bool speciesPresent = cohorts.ContainsKey(species);

            bool IsMaturePresent = (speciesPresent && (cohorts[species].Max(o => o.Age) >= species.Maturity)) ? true : false;

            return IsMaturePresent;
        }

        public Landis.Library.Parameters.Species.AuxParm<int> SpeciesSeed
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> SpeciesSeed = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

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
        
        /*
        public void AddWoodyDebris(float Litter, float KWdLit)
        {
            PlugIn.WoodyDebris[Site].AddMass(Litter, KWdLit);
        }
        public void RemoveWoodyDebris(double percentReduction)
        {
            PlugIn.WoodyDebris[Site].ReduceMass(percentReduction);
        }
        public void AddLitter(float AddLitter, ISpeciesDensity spc)
        {
            
            double KNwdLitter = Math.Max(0.3, (-0.5365 + (0.00241 * AET.Sum())) - (((-0.01586 + (0.000056 * AET.Sum())) * spc.FolLignin * 100)));

            PlugIn.Litter[Site].AddMass(AddLitter, KNwdLitter);
        }
        public void RemoveLitter(double percentReduction)
        {
            PlugIn.Litter[Site].ReduceMass(percentReduction);
        }
        */
        /*string Header(Landis.SpatialModeling.ActiveSite site)
        {
            
            string s = OutputHeaders.Time +  "," +
                       OutputHeaders.Ecoregion + "," + 
                       OutputHeaders.SoilType +"," +
                       OutputHeaders.NrOfCohorts + "," +
                       OutputHeaders.MaxLayerStdev + "," +
                       OutputHeaders.Layers + "," +
                       OutputHeaders.PAR0 + "," +
                       OutputHeaders.Tmin + "," +
                       OutputHeaders.Tave + "," +
                       OutputHeaders.Tday + "," +
                       OutputHeaders.Tmax + "," +
                       OutputHeaders.Precip + "," +
                        OutputHeaders.CO2 + "," +
                         OutputHeaders.O3 + "," +
                       OutputHeaders.RunOff + "," + 
                       OutputHeaders.Leakage + "," + 
                       OutputHeaders.PET + "," +
                       OutputHeaders.Evaporation + "," +
                       OutputHeaders.Transpiration + "," + 
                       OutputHeaders.Interception + "," +
                       OutputHeaders.SurfaceRunOff + "," +
                       OutputHeaders.water + "," +
                       OutputHeaders.PressureHead + "," + 
                       OutputHeaders.SnowPack + "," +
                        OutputHeaders.LAI + "," + 
                        OutputHeaders.VPD + "," + 
                        OutputHeaders.GrossPsn + "," + 
                        OutputHeaders.NetPsn + "," +
                        OutputHeaders.MaintResp + "," +
                        OutputHeaders.Wood + "," + 
                        OutputHeaders.Root + "," + 
                        OutputHeaders.Fol + "," + 
                        OutputHeaders.NSC + "," + 
                        OutputHeaders.HeteroResp + "," +
                        OutputHeaders.Litter + "," + 
                        OutputHeaders.CWD + "," +
                        OutputHeaders.WoodySenescence + "," + 
                        OutputHeaders.FoliageSenescence + ","+
                        OutputHeaders.SubCanopyPAR+","+
                        OutputHeaders.FrostDepth+","+
                        OutputHeaders.LeakageFrac;

            return s;
        }*/

        /*private void AddSiteOutput(IEcoregionPnETVariables monthdata)
        {
            uint maxLayerStdev = 0;
            if (layerstdev.Count() > 0)
                maxLayerStdev = layerstdev.Max();

            string s = monthdata.Year + "," +
                        Ecoregion.Name + "," +
                        Ecoregion.SoilType + "," +
                        cohorts.Values.Sum(o => o.Count) + "," +
                        maxLayerStdev + "," +
                        nlayers + "," +
                        monthdata.PAR0 + "," + 
                        monthdata.Tmin + "," +
                        monthdata.Tave + "," +
                        monthdata.Tday + "," +
                        monthdata.Tmax + "," +
                        monthdata.Prec + "," +
                        monthdata.CO2 + "," +
                         monthdata.O3 + "," +
                        Hydrology.RunOff + "," +
                        Hydrology.Leakage + "," +
                        Hydrology.PET + "," +
                        Hydrology.Evaporation + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Transpiration.Sum())) + "," +
                        interception + "," +
                        precLoss +"," +
                        hydrology.Water + "," +
                         hydrology.GetPressureHead(Ecoregion) + "," +
                        snowPack + "," +
                        this.CanopyLAI + "," +
                        monthdata.VPD + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.GrossPsn.Sum())) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.NetPsn.Sum())) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.MaintenanceRespiration.Sum())) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Wood)) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Root)) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Fol)) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.NSC)) + "," +
                        HeterotrophicRespiration + "," +
                        PlugIn.Litter[Site].Mass + "," +
                        PlugIn.WoodyDebris[Site].Mass + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.LastWoodySenescence)) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.LastFoliageSenescence))+ "," +
                        subcanopypar +","+
                        frostFreeSoilDepth+","+
                        leakageFrac;
           
            this.siteoutput.Add(s);
        }*/
 
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
                Landis.Library.BiomassCohorts.ISpeciesCohorts isp = (Landis.Library.BiomassCohorts.ISpeciesCohorts) this[species];
                yield return isp;
            }
             
        }

        IEnumerator<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)this[species];
            }

             
        }
       

    }


}

