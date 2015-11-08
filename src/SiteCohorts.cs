//  Copyright ...
//  Authors:  Arjan de Bruijn
using Landis.Core;
using System.Collections;
using System.Collections.Generic;
using Landis.SpatialModeling;
using System.Linq;
using Landis.Library.InitialCommunities;
using Edu.Wisc.Forest.Flel.Util;
using System;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class SiteCohorts : ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
        /// <summary>
        /// Occurs when a site is disturbed by an age-only disturbance.
        /// </summary>
        //public static event Landis.Library.BiomassCohorts.DisturbanceEventHandler AgeOnlyDisturbanceEvent;

        //List<Cohort> cohorts = new List<Cohort>();
        //  Initial site biomass for each unique pair of initial
        //  community and ecoregion; Key = 32-bit unsigned integer where
        //  high 16-bits is the map code of the initial community and the
        //  low 16-bits is the ecoregion's map code
        private static IDictionary<uint, SiteCohorts> initialSites;
        private static byte MaxCanopyLayers;
        private static ushort MaxDevLyrAv;

        public Dictionary<ISpecies, List<Cohort>> cohorts = null;
        IEstablishmentProbability establishmentProbability = null;

        public IEcoregionPnET Ecoregion;
        private ActiveSite Site;
       
        public LocalOutput siteoutput;
        
        private float water;
         

        private float CanopyLAI;
        private byte canopylaimax;
        private float[] netpsn =null;
        private float[] grosspsn = null;
        private float[] maintresp = null;
         
        static float[] AET = new float[12]; 
        private float Transpiration;
        private double HeterotrophicRespiration;
         
        private static float interception;
        private static byte Timestep;
        private static int nlayers;

        float subcanopypar;
        ushort watermax;
        private float subcanopyparmax;

        public IEstablishmentProbability EstablishmentProbability 
        {
            get
            {
                return establishmentProbability;
            }
        }

        public float SubCanopyParMAX
        {
            get
            {
                return subcanopyparmax;
            }
        }

        public ushort WaterMax
        {
            get
            {
                return watermax;
            }
        }

        public static void Initialize()
        {
            initialSites = new Dictionary<uint, SiteCohorts>();
            Timestep = ((Parameter<byte>)PlugIn.GetParameter(Names.Timestep)).Value;
            MaxDevLyrAv = ((Parameter<ushort>)PlugIn.GetParameter(Names.MaxDevLyrAv, 0, ushort.MaxValue)).Value;
            MaxCanopyLayers = ((Parameter<byte>)PlugIn.GetParameter(Names.MaxCanopyLayers, 0, 20)).Value;

            
            
        }

        // Create SiteCohorts in SpinUp
        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, string SiteOutputName = null)
        {
            
            Cohort.SetSiteAccessFunctions(this);

            this.Ecoregion = EcoregionPnET.AllEcoregions[PlugIn.ModelCore.Ecoregion[site]];//new EcoregionPnET();
            this.Site = site;
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            uint key = ComputeKey((ushort)initialCommunity.MapCode, PlugIn.ModelCore.Ecoregion[site].MapCode);

            if (initialSites.ContainsKey(key) && SiteOutputName == null)
            {
                establishmentProbability = new EstablishmentProbability(null, null);
                subcanopypar = initialSites[key].subcanopypar;
                subcanopyparmax = initialSites[key].SubCanopyParMAX;
                watermax = initialSites[key].watermax;

                water = initialSites[key].water;
              
                PlugIn.WoodyDebris[Site] = PlugIn.WoodyDebris[initialSites[key].Site].Clone();
                PlugIn.Litter[Site] = PlugIn.Litter[initialSites[key].Site].Clone();
                this.canopylaimax = initialSites[key].CanopyLAImax;

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
                water = (ushort)Ecoregion.FieldCap;

               
                PlugIn.WoodyDebris[Site] = new Library.Biomass.Pool();
                PlugIn.Litter[Site] = new Library.Biomass.Pool();
                
                if (SiteOutputName != null)
                {
                    this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site));
                    
                    establishmentProbability = new EstablishmentProbability(SiteOutputName, "Establishment.csv");
                }
                establishmentProbability = new EstablishmentProbability(null, null);

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

                Landis.Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>> mydata = new Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>>(PlugIn.ModelCore.Ecoregions);
 
                while (date.CompareTo(StartDate) < 0)
                {
                    //  Add those cohorts that were born at the current year
                    while (sortedAgeCohorts.Count() > 0 && StartDate.Year - date.Year == sortedAgeCohorts[0].Age)
                    {
                        Cohort cohort = new Cohort(SpeciesPnET.AllSpecies[sortedAgeCohorts[0].Species], (ushort)date.Year, SiteOutputName);

                        AddNewCohort(cohort);

                        sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                    }
                    
                    // Simulation time runs untill the next cohort is added
                    DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - sortedAgeCohorts[0].Age), 1, 15);

                    List<EcoregionPnETVariables> climate_vars = EcoregionPnET.GetData(Ecoregion, date, EndDate);

                    Grow(climate_vars);

                    date = EndDate;

                }
                if (sortedAgeCohorts.Count > 0) throw new System.Exception("Not all cohorts in the initial communities file were initialized.");
            }
        }
        
        public void SubtractTranspiration(float transpiration, ISpeciesPNET species)
        {
            float WaterMin = Hydrology.CalculateWaterContent(this.Ecoregion, species.H4);  // What is this? It doesn't get used

            Hydrology.SubtractTranspiration(this.Ecoregion, WaterMin, (ushort)transpiration, ref water);
        }

        List<List<int>> GetRandomRange(List<List<int>> bins)
        {
            List<List<int>> random_range = new List<List<int>>();
            if (bins != null) for (int b = 0; b < bins.Count(); b++)
                {
                    random_range.Add(new List<int>());

                    List<int> copy_range = new List<int>(bins[b]);

                    while (copy_range.Count() > 0)
                    {
                        int k = PlugIn.DiscreteUniformRandom(0, copy_range.Count());
                        random_range[b].Add(copy_range[k]);

                        copy_range.RemoveAt(k);
                    }
                }
            return random_range;
        }

        void SetAet(float value, int Month)
        {
            AET[Month-1] = value;
        }

        class DescendingComparer<T> : IComparer<T> where T : IComparable<T>
        {
            public int Compare(T x, T y)
            {
                return y.CompareTo(x);
            }
        }

        public void Grow(List<EcoregionPnETVariables> data)
        {
            establishmentProbability.ResetPerTimeStep();
            Cohort.SetSiteAccessFunctions(this);

            canopylaimax = byte.MinValue;

            //pest =  EstablishmentProbability.InitialPest;

            SortedDictionary<double, Cohort> SubCanopyCohorts = new SortedDictionary<double, Cohort>();

            int SiteBiomass = AllCohorts.Sum(a => a.Biomass);

            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                double defoliation = Landis.Library.Biomass.CohortDefoliation.Compute(Site, AllCohorts[cohort].Species, AllCohorts[cohort].Biomass, SiteBiomass);
                
                if (defoliation > 0)
                {
                    AllCohorts[cohort].ReduceBiomass(defoliation);
                }
                for (int i = 0; i < PlugIn.IMAX; i++)
                {
                    double CumCohortBiomass = ((float)i / (float)PlugIn.IMAX) * AllCohorts[cohort].Biomass;
                    while (SubCanopyCohorts.ContainsKey(CumCohortBiomass))
                    {
                        // Add a negligable value [-1e-10; + 1e-10] to CumCohortBiomass in order to prevent duplicate keys
                        double k =1e-10 * 2.0 * (PlugIn.ContinuousUniformRandom() - 0.5);
                        CumCohortBiomass += k;
                    }
                    SubCanopyCohorts.Add(CumCohortBiomass, AllCohorts[cohort]);
                    
                }
            }

            List<List<int>> bins = GetBins(new List<double>(SubCanopyCohorts.Keys));

            List<List<int>> random_range = GetRandomRange(bins);

            Cohort[] Cohorts = SubCanopyCohorts.Values.ToArray();

            float CohortFraction = 1.0F / SubCanopyCohorts.Count();
            float LeakageFractionPerCohort = CohortFraction * Ecoregion.LeakageFrac;

            netpsn = new float[13];
            grosspsn = new float[13];
            maintresp = new float[13];

            for (int m = 0; m < data.Count(); m++ )
            {
                Transpiration = 0;
                subcanopypar = data[m].PAR0;
                CanopyLAI = 0;
                interception = 0;

                AllCohorts.ForEach(x => x.InitializeSubLayers());


                if (bins != null)
                {
                    for (int b = bins.Count() - 1; b >= 0; b--)
                    {
                        foreach (int r in random_range[b])
                        {
                            using (Cohort c = Cohorts[r])
                            {
                                c.CalculatePhotosynthesis(data[m], CohortFraction, LeakageFractionPerCohort, ref water,  ref subcanopypar);
                                interception += c.Interception.Sum();
                                c.Layer = (byte)Math.Max(b, c.Layer);
                            }
                        }
                    }
                }


               
               
                AllCohorts.ForEach(x =>
                    {
                        netpsn[data[m].Month - 1] += x.NetPsn.Sum();
                        grosspsn[data[m].Month - 1] += x.GrossPsn.Sum() * PlugIn.FTimeStep;
                        maintresp[data[m].Month - 1] += x.MaintenanceRespiration.Sum() * PlugIn.FTimeStep;
                        CanopyLAI += x.LAI.Sum();
                    }
                );
                
              
                canopylaimax = (byte)Math.Max(canopylaimax, CanopyLAI);
                watermax = (byte)Math.Max(water, watermax);
                subcanopyparmax = Math.Max(subcanopyparmax, subcanopypar);

                Hydrology.SubtractEvaporation(data[m].Month, Ecoregion, (ushort)subcanopypar, Transpiration, data[m].Tday, ref water,  SetAet);

                if (siteoutput != null)
                {
                    AddSiteOutput(data[m]);

                    AllCohorts.ForEach(a => a.UpdateCohortData(data[m]));
                }

                if (PlugIn.ModelCore.CurrentTime > 0)
                {
                    establishmentProbability.Calculate_Establishment(data[m], Ecoregion, subcanopypar, water);
                }

                AllCohorts.ForEach(x => x.NullSubLayers());
            }
            if (siteoutput != null)
            {
                siteoutput.Write();

                AllCohorts.ForEach(cohort => { cohort.WriteCohortData(); });
            }

            RemoveMarkedCohorts();

            HeterotrophicRespiration = (ushort)(PlugIn.Litter[Site].Decompose() + PlugIn.WoodyDebris[Site].Decompose());//6s
                
        }

        private int[] GetIntArray(float[] float_array)
        {
            if (float_array == null) return new int[13];

            int[] int_array = new int[float_array.Length];
            for (int f = 0; f < float_array.Length; f++)
            {
                int_array[f] = (int)float_array[f];
            }
            return int_array;
        }
        
        public int[] MaintResp
        {
            get
            {
                return GetIntArray(maintresp);
            }
        }

        public int[] GrossPsn
        {
            get
            {
                return GetIntArray(grosspsn);
            }
        }

        public int[] NetPsn
        {
            get
            {
                return GetIntArray(netpsn);
            }
        }

        public byte CanopyLAImax
        {
            get
            {
                return (byte)canopylaimax;
            }
        }

        public double WoodyDebris 
        {
            get
            {
                return PlugIn.WoodyDebris[Site].Mass;
            }
        }

        public double Litter 
        {
            get
            {
                return PlugIn.Litter[Site].Mass;
            }
        }
       
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

        public Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies 
        { 
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> SpeciesPresent = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    SpeciesPresent[spc] = cohorts[spc].Sum(o=>o.Biomass);
                }
                return SpeciesPresent;
            }
        }

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

        public float BiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => o.Biomass);
            }

        }

        public uint BelowGroundBiomass 
        {
            get
            {
                return (uint)cohorts.Values.Sum(o => o.Sum(x => x.Root));
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

        class SubCanopyComparer : IComparer<int[]>
        {
            // Compare second int (cumulative cohort biomass)
            public int Compare(int[] x, int[] y)
            {
                return (x[0] > y[0])? 1:-1;
            }
        }

        private SortedDictionary<int[], Cohort> GetSubcanopyLayers()
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
        }

        private static int[] GetNextBinPositions(int[] index_in, int numcohorts)
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
                 */
            }
            return null;
        }
      
        private int[] GetFirstBinPositions(int nlayers, int ncohorts)
        {
            int[] Bin = new int[nlayers - 1];

            for (int ly = 0; ly < Bin.Length; ly++)
            {
                Bin[ly] = ly+1;
            }
            return Bin;
        }

        public static List<T> Shuffle<T>(List<T> array)
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
        }

        uint CalculateLayerstdev(List<double> f)
        {
            return (uint)Math.Max(Math.Abs(f.Max() - f.Average()), Math.Abs(f.Min() - f.Average()));

        }

        int[] MinMaxCohortNr(int[] Bin, int i, int Count)
        {
            int min = (i > 0) ? Bin[i - 1] : 0;
            int max = (i < Bin.Count()) ? Bin[i] : Count - 1;

            return new int[] { min, max };
        }

        static List<uint> layerstdev = new List<uint>();

        private List<List<int>> GetBins(List<double> CumCohortBiomass)
        {
            if (CumCohortBiomass.Count() == 0) return null;

            // Bin and BestBin are lists of indexes that determine what cohort belongs to what canopy layer, 
            // i.e. when Bin[1] contains 45 then SubCanopyCohorts[45] is in layer 1
            int[] BestBin = null;
            int[] Bin = null;

            nlayers = 0;

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
            while (layerstdev.Max() >= MaxDevLyrAv && nlayers < MaxCanopyLayers);
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

        public int ReduceOrKillBiomassCohorts(Landis.Library.BiomassCohorts.IDisturbance disturbance)
        {
            List<int> reduction = new List<int>();

            List<Cohort> ToRemove = new List<Cohort>();

            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                Landis.Library.BiomassCohorts.SpeciesCohorts species_cohorts = GetSpeciesCohort(cohorts[species_cohort[0].Species]);
                
                for (int c =0;c< species_cohort.Count(); c++)
                {
                    reduction.Add(disturbance.ReduceOrKillMarkedCohort(species_cohort[c]));
                    if (reduction[reduction.Count()-1] >= species_cohort[c].Biomass)
                        ToRemove.Add(species_cohort[c]);
                    // Edited by BRM - 090115
                    else
                    {
                        double reductionProp = (double)reduction[reduction.Count() - 1] / (double)species_cohort[c].Biomass;
                        species_cohort[c].ReduceBiomass(reductionProp);
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
       
        Landis.Library.AgeOnlyCohorts.ISpeciesCohorts Landis.Library.Cohorts.ISiteCohorts<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.this[ISpecies species]
        {
            get
            {
                if (cohorts.ContainsKey(species))
                {
                    return (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)GetSpeciesCohort(cohorts[species]);
                }
                return null;
            }
        }

        public Landis.Library.BiomassCohorts.ISpeciesCohorts this[ISpecies species]
        {
            get
            {
                if (cohorts.ContainsKey(species))
                {
                    return GetSpeciesCohort(cohorts[species]);
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

            //  Go through list of species cohorts from back to front so that
            //  a removal does not mess up the loop.
            int totalReduction = 0;

            List<Cohort> ToRemove = new List<Cohort>();

            Landis.Library.AgeOnlyCohorts.SpeciesCohortBoolArray isSpeciesCohortDamaged = new Landis.Library.AgeOnlyCohorts.SpeciesCohortBoolArray();

            foreach (ISpecies spc in cohorts.Keys)
            {
                Landis.Library.BiomassCohorts.SpeciesCohorts speciescohort = GetSpeciesCohort(cohorts[spc]);

                isSpeciesCohortDamaged.SetAllFalse(speciescohort.Count);

                disturbance.MarkCohortsForDeath(speciescohort, isSpeciesCohortDamaged);

                for (int c = 0; c < isSpeciesCohortDamaged.Count; c++)
                {
                    if (isSpeciesCohortDamaged[c])
                    {
                        totalReduction += speciescohort[c].Biomass;

                        ToRemove.Add(cohorts[spc][c]);
//                        ToRemove.AddRange(cohorts[spc].Where(o => o.Age == speciescohort[c].Age));
                    }
                }

            }
            foreach (Cohort cohort in ToRemove)
            {
                RemoveCohort(cohort, disturbance.Type);
            }
        }

        private void RemoveMarkedCohorts()
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

        }

        public void RemoveCohort(Cohort cohort, ExtensionType disturbanceType)
        {
            if (disturbanceType.Name != Names.ExtensionName)
            {
                Cohort.RaiseDeathEvent(this, cohort, Site, disturbanceType);
            }

            cohorts[cohort.Species].Remove(cohort);

            if (cohorts[cohort.Species].Count == 0)
            {
                cohorts.Remove(cohort.Species);
            }

            Allocation.Allocate(this, cohort, disturbanceType);

        }

        public bool IsMaturePresent(ISpecies species)
        {
            bool myreturn = (cohorts.ContainsKey(species) && (cohorts[species].Min(o => o.Age) > species.Maturity)) ? true : false;

            return myreturn;
        }

        public void AddNewCohort(Cohort cohort)
        {

            if (cohorts.ContainsKey(cohort.Species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies, List<Cohort>>>(cohorts.Where(o => o.Key == cohort.Species))[0];

                List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age <= Timestep));

                Cohorts.ForEach(a => cohort.Accumulate(a)); ;

                if (Cohorts.Count() > 0)
                {
                    Cohorts[0].Accumulate(cohort);
                    return;
                }

                cohorts[cohort.Species].Add(cohort);

                return;
            }
            cohorts.Add(cohort.Species, new List<Cohort>(new Cohort[] { cohort }));
        }

        Landis.Library.BiomassCohorts.SpeciesCohorts GetSpeciesCohort(List<Cohort> cohorts)
        {
            Landis.Library.BiomassCohorts.SpeciesCohorts spc = new Library.BiomassCohorts.SpeciesCohorts(cohorts[0].Species, cohorts[0].Age, cohorts[0].Biomass);

            for (int c = 1; c < cohorts.Count; c++)
            {
                spc.AddNewCohort(cohorts[c].Age, cohorts[c].Biomass);
            }


            return spc;
        }
        
        public void AddWoodyDebris(float Litter, float KWdLit)
        {
            PlugIn.WoodyDebris[Site].AddMass(Litter, KWdLit);
        }

        public void AddLitter(float AddLitter, ISpeciesPNET spc)
        {
            
            double KNwdLitter = Math.Max(0.3, (-0.5365 + (0.00241 * AET.Sum())) - (((-0.01586 + (0.000056 * AET.Sum())) * spc.FolLignin * 100)));

            PlugIn.Litter[Site].AddMass(AddLitter, KNwdLitter);
        }

        string Header(Landis.SpatialModeling.ActiveSite site)
        {
            
            string s = OutputHeaders.Time +  "," + 
                       OutputHeaders.NrOfCohorts + "," +
                       OutputHeaders.MaxLayerStdev + "," + 
                       OutputHeaders.layers + "," + 
                       OutputHeaders.PAR0  + "," + 
                       OutputHeaders.Tday + "," + 
                       OutputHeaders.Precip + "," +
                       OutputHeaders.RunOff + "," + 
                       OutputHeaders.Leakage + "," + 
                       OutputHeaders.PET + "," +
                       OutputHeaders.Evaporation + "," +
                       OutputHeaders.Transpiration + "," + 
                       OutputHeaders.Interception + "," +
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
                        OutputHeaders.CWD;

            return s;
        }

        private void AddSiteOutput(EcoregionPnETVariables monthdata)
        {

            string s = monthdata.Year + "," +
                        cohorts.Values.Sum(o => o.Count) + "," +
                        layerstdev.Max() + "," +
                        nlayers + "," +
                        monthdata.PAR0 + "," +
                        monthdata.Tday + "," +
                        monthdata.Prec + "," +
                        Hydrology.RunOff + "," +
                        Hydrology.Leakage + "," +
                        Hydrology.PET + "," +
                        Hydrology.Evaporation + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Transpiration.Sum())) + "," +
                        interception + "," +
                        water + "," +
                        Hydrology.Pressureheadtable[Ecoregion, (int)water] + "," +
                        monthdata.SnowPack + "," +
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
                         PlugIn.WoodyDebris[Site].Mass;
           
            this.siteoutput.Add(s);
        }
 
        public IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return this[species];
            }
        }
        /*
        public IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> GetEnumerator()
        {
            foreach (Landis.Library.BiomassCohorts.SpeciesCohorts speciesCohorts in Speciescohorts)
            {
                yield return speciesCohorts;
            }
        }
         */
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.BiomassCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                Landis.Library.BiomassCohorts.ISpeciesCohorts isp = this[species];
                yield return isp;
            }
             
        }

        IEnumerator<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts> IEnumerable<Landis.Library.AgeOnlyCohorts.ISpeciesCohorts>.GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts)this[species];
            }

            //foreach (Landis.Library.BiomassCohorts.SpeciesCohorts speciesCohort in Speciescohorts)
            //{
            //    yield return speciesCohort;
            //}
        }
       

    }


}

