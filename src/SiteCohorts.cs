//  Copyright ...
//  Authors:  Arjan de Bruijn

using Landis.Utilities;
using Landis.Core;
using Landis.Library.InitialCommunities;
using Landis.SpatialModeling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Landis.Library.Climate;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class SiteCohorts : ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
        private byte canopylaimax;
        private float watermax;
        private float snowPack;
        private float CanopyLAI;
        private float subcanopypar;
        private float subcanopyparmax;
        private float propRootAboveFrost;
        private float topFreezeDepth;
        private float soilDiffusivity;
        private float leakageFrac;
        private float runoffCapture;

        private float[] netpsn = null;
        private float netpsnsum;
        private float[] grosspsn = null;
        private float[] folresp = null;
        
        private float[] maintresp = null;
        private float transpiration;
        private double HeterotrophicRespiration;
        public ActiveSite Site;
        public Dictionary<ISpecies, List<Cohort>> cohorts = null;
        IEstablishmentProbability establishmentProbability = null;
        private IHydrology hydrology = null;
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
        private static byte MaxCanopyLayers;
        private static ushort MaxDevLyrAv;
        private static float interception;
        private static float precLoss;
        private static byte Timestep;
        private static int CohortBinSize;
        private static int nlayers;
        private static bool permafrost;
        private static bool invertPest;
        Dictionary<float, float> depthTempDict = new Dictionary<float, float>();  //for permafrost
        //float lastTempBelowSnow = float.MaxValue;
        private static float maxHalfSat;
        private static float minHalfSat;

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

        public float Transpiration
        {
            get
            {
                return transpiration;
            }
        }
        //---------------------------------------------------------------------
        public float SubcanopyPAR
        {
            get
            {
                return subcanopypar;
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
        public float SubCanopyParMAX
        {
            get
            {
                return subcanopyparmax;
            }
        }
        //---------------------------------------------------------------------
        public float WaterMax
        {
            get
            {
                return watermax;
            }
        }
        //---------------------------------------------------------------------
        public float[] NetPsn
        {
            get
            {
                if (netpsn == null)
                {
                    float[] netpsn_array = new float[12];
                    for (int i = 0; i < netpsn_array.Length; i++)
                    {
                        netpsn_array[i] = 0;
                    }
                    return netpsn_array;
                }
                else
                {
                    //return netpsn.Select(psn => (int)psn).ToArray();
                    return netpsn.ToArray();
                }
            }
        }

        public static void Initialize()
        {
            initialSites = new Dictionary<uint, SiteCohorts>();
            Timestep = ((Parameter<byte>)PlugIn.GetParameter(Names.Timestep)).Value;
            MaxDevLyrAv = ((Parameter<ushort>)PlugIn.GetParameter(Names.MaxDevLyrAv, 0, ushort.MaxValue)).Value;
            MaxCanopyLayers = ((Parameter<byte>)PlugIn.GetParameter(Names.MaxCanopyLayers, 0, 20)).Value;
            permafrost = ((Parameter<bool>)PlugIn.GetParameter(Names.Permafrost)).Value;
            invertPest = ((Parameter<bool>)PlugIn.GetParameter(Names.InvertPest)).Value;
            Parameter<string> CohortBinSizeParm = null;
            if (PlugIn.TryGetParameter(Names.CohortBinSize, out CohortBinSizeParm))
            {
                if (!Int32.TryParse(CohortBinSizeParm.Value, out CohortBinSize))
                {             
                    throw new System.Exception("CohortBinSize is not an integer value.");
        }
            }
            else
                CohortBinSize = Timestep;
            maxHalfSat = 0;
            minHalfSat = float.MaxValue;
            foreach(ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
            {
                if (spc.HalfSat > maxHalfSat)
                    maxHalfSat = spc.HalfSat;
                if (spc.HalfSat < minHalfSat)
                    minHalfSat = spc.HalfSat;
        }
        }

        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, bool usingClimateLibrary, string SiteOutputName = null)
        {
            Cohort.SetSiteAccessFunctions(this);
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
                establishmentProbability = new EstablishmentProbability(null, null);
                subcanopypar = initialSites[key].subcanopypar;
                subcanopyparmax = initialSites[key].SubCanopyParMAX;
                watermax = initialSites[key].watermax;

                hydrology = new Hydrology(initialSites[key].hydrology.Water);

                PlugIn.WoodyDebris[Site] = PlugIn.WoodyDebris[initialSites[key].Site].Clone();
                PlugIn.Litter[Site] = PlugIn.Litter[initialSites[key].Site].Clone();
                PlugIn.FineFuels[Site] = PlugIn.Litter[Site].Mass;
                //PlugIn.PressureHead[Site] = hydrology.GetPressureHead(this.Ecoregion);
                this.canopylaimax = initialSites[key].CanopyLAImax;

                foreach (ISpecies spc in initialSites[key].cohorts.Keys)
                {
                    foreach (Cohort cohort in initialSites[key].cohorts[spc])
                    {
                        bool addCohort = AddNewCohort(new Cohort(cohort));
                    }          
                }
                this.netpsn = initialSites[key].NetPsn;
                this.folresp = initialSites[key].FolResp;
                this.grosspsn = initialSites[key].GrossPsn;
                this.maintresp = initialSites[key].MaintResp;
                this.CanopyLAI = initialSites[key].CanopyLAI;
                this.transpiration = initialSites[key].Transpiration;

                // Calculate AdjFolFrac
                AllCohorts.ForEach(x => x.CalcAdjFracFol());

            }
            else
            {
                if (initialSites.ContainsKey(key) == false)
                {
                    initialSites.Add(key, this);
                }
                List<IEcoregionPnETVariables> ecoregionInitializer = EcoregionPnET.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));
                hydrology = new Hydrology(Ecoregion.FieldCap);
                watermax = hydrology.Water;
                subcanopypar = ecoregionInitializer[0].PAR0;
                subcanopyparmax = subcanopypar;

                PlugIn.WoodyDebris[Site] = new Library.Biomass.Pool();
                PlugIn.Litter[Site] = new Library.Biomass.Pool();
                PlugIn.FineFuels[Site] = PlugIn.Litter[Site].Mass;
                //PlugIn.PressureHead[Site] = hydrology.GetPressureHead(Ecoregion);

                if (SiteOutputName != null)
                {
                    this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site));

                    establishmentProbability = new EstablishmentProbability(SiteOutputName, "Establishment.csv");
                }
                else
                {
                    establishmentProbability = new EstablishmentProbability(null, null);
                }

                bool biomassProvided = false;
                foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                {
                    foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                    {
                        if (cohort.Biomass > 0)  // 0 biomass indicates biomass value was not read in
                        {
                            biomassProvided = true;
                            break;
                        }
                    }
                }

                if (biomassProvided)
                {
                    foreach (Landis.Library.BiomassCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
                    {
                        foreach (Landis.Library.BiomassCohorts.ICohort cohort in speciesCohorts)
                        {
                            bool addCohort = AddNewCohort(new Cohort(PlugIn.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, SiteOutputName, (ushort)(StartDate.Year - cohort.Age)));
                        }
                    }

                    AllCohorts.ForEach(x =>
                    {
                        CanopyLAI += x.LAI.Sum();
                    }
                    );
                    this.canopylaimax = (byte)CanopyLAI;

                    //CalculateInitialWater(StartDate);
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

                DateTime date = StartDate.AddYears(-(sortedAgeCohorts[0].Age - 1));

            Landis.Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>> mydata = new Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>>(PlugIn.ModelCore.Ecoregions);

                while (date.CompareTo(StartDate) < 0)
                {
                    //  Add those cohorts that were born at the current year
                    while (sortedAgeCohorts.Count() > 0 && StartDate.Year - date.Year == (sortedAgeCohorts[0].Age - 1))
                    {
                        Cohort cohort = new Cohort(PlugIn.SpeciesPnET[sortedAgeCohorts[0].Species], (ushort)date.Year, SiteOutputName);

                        bool addCohort = AddNewCohort(cohort);

                        sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                    }

                    // Simulation time runs untill the next cohort is added
                    DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - (sortedAgeCohorts[0].Age-1)), 1, 15);

                    var climate_vars = usingClimateLibrary ? EcoregionPnET.GetClimateRegionData(Ecoregion, date, EndDate, Climate.Phase.SpinUp_Climate) : EcoregionPnET.GetData(Ecoregion, date, EndDate);

                    Grow(climate_vars);

                    date = EndDate;

                }
                if (sortedAgeCohorts.Count > 0) throw new System.Exception("Not all cohorts in the initial communities file were initialized.");
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
                        int k = PlugIn.DiscreteUniformRandom(0, copy_range.Count()-1);
                        random_range[b].Add(copy_range[k]);

                        copy_range.RemoveAt(k);
                    }
                }
            return random_range;
        }

        public void SetAet(float value, int Month)
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

        private static float ComputeMaxSnowMelt(float Tave, float DaySpan)
        {
            // Snowmelt rate can range between 1.6 to 6.0 mm/degree day, and default should be 2.74 according to NRCS Part 630 Hydrology National Engineering Handbook (Chapter 11: Snowmelt)
            return 2.74f * Math.Max(0, Tave) * DaySpan;
        }
        private static float CumputeSnowFraction(float Tave)
        {
            return (float)Math.Max(0.0, Math.Min(1.0, (Tave - 2) / -7));
        }

        public bool Grow(List<IEcoregionPnETVariables> data)
        {
            bool success = true;
            float sumPressureHead = 0;
            int countPressureHead = 0;

            establishmentProbability.ResetPerTimeStep();
            Cohort.SetSiteAccessFunctions(this);

            canopylaimax = byte.MinValue;

            SortedDictionary<double, Cohort> SubCanopyCohorts = new SortedDictionary<double, Cohort>();

            int SiteAboveGroundBiomass = AllCohorts.Sum(a => a.Biomass);

            List<int> cohortAges = new List<int>();

            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                if (PlugIn.ModelCore.CurrentTime > 0)
                {
                    AllCohorts[cohort].CalculateDefoliation(Site, SiteAboveGroundBiomass);
                }
                for (int i = 1; i <= PlugIn.IMAX; i++)
                {
                    double CumCohortBiomass = ((float)i / (float)PlugIn.IMAX) * AllCohorts[cohort].TotalBiomass;
                    while (SubCanopyCohorts.ContainsKey(CumCohortBiomass))
                    {
                        // Add a negligable value [-1e-10; + 1e-10] to CumCohortBiomass in order to prevent duplicate keys
                        double k =1e-10 * 2.0 * (PlugIn.ContinuousUniformRandom() - 0.5);
                        CumCohortBiomass += k;
                    }
                    SubCanopyCohorts.Add(CumCohortBiomass, AllCohorts[cohort]);                    
                }
                if(!cohortAges.Contains(AllCohorts[cohort].Age))
                {
                    cohortAges.Add(AllCohorts[cohort].Age);
                }
            }

     
            List<List<int>> rawBins = GetBins(new List<double>(SubCanopyCohorts.Keys));

            // Sort through bins to put cohort sublayers in the same bin based on majority
            List<List<int>> bins = new List<List<int>>();
            if((rawBins != null) && (rawBins.Count > 1))
            {
                Dictionary<string, Dictionary<int,int>> speciesLayerIndex = new Dictionary<string, Dictionary<int,int>>();
                List<int> addedValues = new List<int>();
                foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
                {
                    foreach (int thisAge in cohortAges)
                    {
                        Dictionary<int, double> sumBio = new Dictionary<int, double>();
                        for (int i = 0; i < rawBins.Count(); i++)
                        {
                            double sumLayerBio = 0;
                            List<int> binLayers = rawBins[i];
                            for (int b = 0; b < binLayers.Count(); b++)
                            {
                                int layerKey = binLayers[b];
                                Cohort layerCohort = SubCanopyCohorts.Values.ToArray()[layerKey];
                                if ((layerCohort.SpeciesPNET.Name == spc.Name) && (layerCohort.Age == thisAge))
                                {
                                    sumLayerBio += ((double)layerCohort.TotalBiomass) / ((double)PlugIn.IMAX);
                                }
                            }
                            sumBio.Add(i, sumLayerBio);
                        }                      

                        int layerMaxBio = sumBio.LastOrDefault(x => x.Value == sumBio.Values.Max()).Key;

                        if (sumBio.Values.Max() > 0)
                        {
                            if (speciesLayerIndex.Keys.Contains(spc.Name))
                            {
                                Dictionary<int, int> ageIndex = speciesLayerIndex[spc.Name];
                                ageIndex.Add(thisAge, layerMaxBio);
                                speciesLayerIndex[spc.Name] = ageIndex;
                            }
                            else
                            {
                                Dictionary<int, int> ageIndex = new Dictionary<int, int>();
                                ageIndex.Add(thisAge, layerMaxBio);
                                speciesLayerIndex.Add(spc.Name, ageIndex);
                            }
                            addedValues.Add(layerMaxBio);
                        }
                    }
                }
                //step through subcanopycohorts
                int subLayerKey = 0;

                // There shouldn't be more layers than cohorts
                int numberOfLayers = Math.Min(addedValues.Max()+1, (int)((float)SubCanopyCohorts.Count() / (float)PlugIn.IMAX));

                // Final layer indices should not skip any layers
                List<int> distinctValues = addedValues.Distinct().ToList();
                distinctValues.Sort();
                Dictionary<int, int> indexLookup = new Dictionary<int, int>();
                for (int j = 0; j < distinctValues.Count(); j++ )
                {
                    indexLookup.Add(distinctValues[j], j);
                }

                for (int i = 0; i < numberOfLayers; i++)
                {
                    bins.Add(new List<int>());
                }
                foreach (KeyValuePair<double, Cohort> entry in SubCanopyCohorts)
                {
                    ISpecies spc = entry.Value.SpeciesPNET;
                    int thisAge = entry.Value.Age;
                    int tempIndex = indexLookup[speciesLayerIndex[spc.Name][thisAge]];
                    int layerIndex = Math.Max(tempIndex,entry.Value.Layer);  //Once a cohort reaches a canopy layer it cannot be dropped below that position
                    if(layerIndex > MaxCanopyLayers - 1)
                    {
                        throw new System.Exception("layerIndex  " + layerIndex + " is greater than MaxCanopyLayers - 1: " + (MaxCanopyLayers-1));
                    }
                    if (bins.ElementAtOrDefault(layerIndex) == null)
                    {
                        while (bins.ElementAtOrDefault(layerIndex) == null)
                        {
                            bins.Add(new List<int>());                           
                        }
                    }
                    bins[layerIndex].Add(subLayerKey);
                    
                    subLayerKey += 1;
                }

            }
            else
            {
                bins = rawBins;
            }

            List<List<int>> random_range = GetRandomRange(bins);
             
            folresp = new float[13];
            netpsn = new float[13];
            grosspsn = new float[13];
            maintresp = new float[13];

            Dictionary<ISpeciesPNET, List<float>> annualEstab = new Dictionary<ISpeciesPNET, List<float>>();
            Dictionary<ISpeciesPNET, float> cumulativeEstab = new Dictionary<ISpeciesPNET, float>();
            Dictionary<ISpeciesPNET, List<float>> annualFwater = new Dictionary<ISpeciesPNET, List<float>>();
            Dictionary<ISpeciesPNET, float> cumulativeFwater = new Dictionary<ISpeciesPNET, float>();
            Dictionary<ISpeciesPNET, List<float>> annualFrad = new Dictionary<ISpeciesPNET, List<float>>();
            Dictionary<ISpeciesPNET, float> cumulativeFrad = new Dictionary<ISpeciesPNET, float>();
            Dictionary<ISpeciesPNET, float> monthlyEstab = new Dictionary<ISpeciesPNET, float>();
            Dictionary<ISpeciesPNET, int> monthlyCount = new Dictionary<ISpeciesPNET, int>();
            Dictionary<ISpeciesPNET, int> coldKillMonth = new Dictionary<ISpeciesPNET, int>(); // month in which cold kills each species

            foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
            {
                annualEstab[spc] = new List<float>();
                cumulativeEstab[spc] = 1;
                annualFwater[spc] = new List<float>();
                cumulativeFwater[spc] = 0;
                annualFrad[spc] = new List<float>();
                cumulativeFrad[spc] = 0;
                monthlyCount[spc] = 0;
                coldKillMonth[spc] = int.MaxValue;
            }

            float[] lastOzoneEffect = new float[SubCanopyCohorts.Count()];
            for (int i = 0; i < lastOzoneEffect.Length; i++)
            {
                lastOzoneEffect[i] = 0;
            }


            float lastPropBelowFrost = (hydrology.FrozenDepth/Ecoregion.RootingDepth);
            int daysOfWinter = 0;

            if (PlugIn.ModelCore.CurrentTime > 0) // cold can only kill after spinup
            {
                // Loop through months & species to determine if cold temp would kill any species
                float extremeMinTemp = float.MaxValue;
                int extremeMonth = 0;
                for (int m = 0; m < data.Count(); m++)
                {
                    float minTemp = data[m].Tave - (float)(3.0 * Ecoregion.WinterSTD);
                    if(minTemp < extremeMinTemp)
                    {
                        extremeMinTemp = minTemp;
                        extremeMonth = m;
                    }
                }
                PlugIn.ExtremeMinTemp[Site] = extremeMinTemp;
                foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
                {
                        // Check if low temp kills species
                    if (extremeMinTemp < spc.ColdTol)
                        {
                        coldKillMonth[spc] = extremeMonth;
                        }

                    }
                }
            //Clear pressurehead site values
            sumPressureHead = 0;
            countPressureHead = 0;
            for (int m = 0; m < data.Count(); m++ )
            {
                Ecoregion.Variables = data[m];
                transpiration = 0;
                subcanopypar = Ecoregion.Variables.PAR0;                
                interception = 0;

                // Reset monthly variables that get reported as single year snapshots
                if (Ecoregion.Variables.Month == 1)
                {
                    folresp = new float[13];
                    netpsn = new float[13];
                    grosspsn = new float[13];
                    maintresp = new float[13];
                }

                float ozoneD40 = 0;
                if (m > 0)
                    ozoneD40 = Math.Max(0, Ecoregion.Variables.O3 - data[m - 1].O3);
                else
                    ozoneD40 = Ecoregion.Variables.O3;
                float O3_D40_ppmh = ozoneD40 / 1000; // convert D40 units to ppm h

                propRootAboveFrost = 1;
                leakageFrac = Ecoregion.LeakageFrac;
                float propThawed = 0;

                if (permafrost)
                {
                    // snow calculations - from "Soil thawing worksheet with snow.xlsx"
                    if (Ecoregion.Variables.Tave <= 0)
                    {
                        daysOfWinter += (int)Ecoregion.Variables.DaySpan;
                    }
                    else if (snowPack > 0)
                    {
                        daysOfWinter += (int)Ecoregion.Variables.DaySpan;
                    }
                    else
                    { 
                        daysOfWinter = 0;
                    }
                    float bulkIntercept = 165.0f; //kg/m3
                    float bulkSlope = 1.3f; //kg/m3
                    float Pwater = 1000.0f;  // Density of water (kg/m3)
                    float lambAir = 0.023f; // W/m K (CLM5 documentation, Table 2.7)
                    float lambIce = 2.29f; // W/m K (CLM5 documentation, Table 2.7)
                    float snowHeatCapacity = 2090f; // J/kg K (https://www.engineeringtoolbox.com/specific-heat-capacity-d_391.html)

                    float Psno_kg_m3 = bulkIntercept + (bulkSlope * daysOfWinter); //kg/m3
                    float Psno_g_cm3 = Psno_kg_m3 / 1000; //g/cm3

                    float sno_dep = Pwater * (snowPack / 1000) / Psno_kg_m3 ; //m
                    //if (Ecoregion.Variables.Tave >= 0)  -- snowmelt has already been accounted for
                    //{
                    //    float fracAbove0 = Ecoregion.Variables.Tmax / (Ecoregion.Variables.Tmax - Ecoregion.Variables.Tmin);
                    //    sno_dep = sno_dep * fracAbove0;
                    //}
                    // from CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
                    // Eq. 85 - Jordan (1991)
                    float lambda_Snow = (float) (lambAir+((0.0000775*Psno_kg_m3)+(0.000001105*Math.Pow(Psno_kg_m3,2)))*(lambIce-lambAir))*3.6F*24F; //(kJ/m/d/K) includes unit conversion from W to kJ
                    float vol_heat_capacity_snow = snowHeatCapacity * Psno_kg_m3 / 1000f; // kJ/m3/K
                    //float Ks_snow = Ecoregion.Variables.DaySpan * lambda_Snow / vol_heat_capacity_snow; //thermal diffusivity (m2/month)
                    float Ks_snow = 1000000F / 86400F * (lambda_Snow / vol_heat_capacity_snow); //thermal diffusivity (mm2/s)
                    float damping = (float) Math.Sqrt((2.0F* Ks_snow)/ Constants.omega);
                    float DRz_snow = 1F;
                    if(sno_dep > 0)
                        DRz_snow = (float)Math.Exp(-1.0F * sno_dep * damping); // Damping ratio for snow - adapted from Kang et al. (2000) and Liang et al. (2014)

                    float mossDepth = Ecoregion.MossDepth; //m
                    float cv = 2500; // heat capacity moss - kJ/m3/K (Sazonova and Romanovsky 2003)
                    float lambda_moss = 432; // kJ/m/d/K - converted from 0.2 W/mK (Sazonova and Romanovsky 2003)
                    float moss_diffusivity = lambda_moss / cv;
                    float damping_moss = (float)Math.Sqrt((2.0F * moss_diffusivity) / Constants.omega);
                    float DRz_moss = (float)Math.Exp(-1.0F * mossDepth * damping_moss); // Damping ratio for moss - adapted from Kang et al. (2000) and Liang et al. (2014)


                    //float waterContent = (float)Math.Min(1.0, hydrology.Water / Ecoregion.RootingDepth);  //m3/m3
                    //float waterContent = hydrology.Water/1000;
                    float waterContent = hydrology.Water;// volumetric m/m
                    // Permafrost calculations - from "Soil thawing worksheet.xlsx"
                    // 
                    //if (Ecoregion.Variables.Tave < minMonthlyAvgTemp)
                    //    minMonthlyAvgTemp = Ecoregion.Variables.Tave;
                     //Calculations of diffusivity from soil properties 
                    //float porosity = Ecoregion.Porosity / Ecoregion.RootingDepth;  //m3/m3                    
                    //float porosity = Ecoregion.Porosity/1000;  // m/m   
                    float porosity = Ecoregion.Porosity;  // volumetric m/m 
                    float ga = 0.035F + 0.298F * (waterContent / porosity);
                    float Fa = ((2.0F / 3.0F) / (1.0F + ga * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))) + ((1.0F / 3.0F) / (1.0F + (1.0F - 2.0F * ga) * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))); // ratio of air temp gradient
                    float Fs = PressureHeadSaxton_Rawls.GetFs(Ecoregion.SoilType);
                    float lambda_s = PressureHeadSaxton_Rawls.GetLambda_s(Ecoregion.SoilType);
                    float lambda_theta = (Fs * (1.0F - porosity) * lambda_s + Fa * (porosity - waterContent) * Constants.lambda_a + waterContent * Constants.lambda_w) / (Fs * (1.0F - porosity) + Fa * (porosity - waterContent) + waterContent); //soil thermal conductivity (kJ/m/d/K)
                    float D = lambda_theta / PressureHeadSaxton_Rawls.GetCTheta(Ecoregion.SoilType);  //m2/day
                    float Dmms = D * 1000000 / 86400; //mm2/s
                    soilDiffusivity = Dmms;
                    float Dmonth = D * Ecoregion.Variables.DaySpan; // m2/month
                    float ks = Dmonth * 1000000F / (Ecoregion.Variables.DaySpan * (Constants.SecondsPerHour * 24)); // mm2/s
                    //float d = (float)Math.Pow((Constants.omega / (2.0F * Dmonth)), (0.5));
                    float d = (float)Math.Sqrt(2 * Dmms / Constants.omega);
                    
                    float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
                    topFreezeDepth = maxDepth/1000;
                    float bottomFreezeDepth = maxDepth / 1000;
                    float testDepth = 0;

                    float tSum = 0;
                    float pSum = 0;
                    float tMax = float.MinValue;
                    float tMin = float.MaxValue;
                    int maxMonth = 0;
                    int mCount = 0;
                    if (m < 12)
                    {
                        mCount = Math.Min(12, data.Count());
                        foreach (int z in Enumerable.Range(0, mCount))
                        {
                            tSum += data[z].Tave;
                            pSum += data[z].Prec;
                            if (data[z].Tave > tMax)
                        {
                                tMax = data[z].Tave;
                                maxMonth = data[z].Month;
                        }
                            if (data[z].Tave < tMin)
                                tMin = data[z].Tave;
                        }
                    }
                    else
                    {
                        mCount = 12;
                        foreach (int z in Enumerable.Range(m-11, 12))
                        {
                            tSum += data[z].Tave;
                            pSum += data[z].Prec;
                            if (data[z].Tave > tMax)
                            {
                                tMax = data[z].Tave;
                                maxMonth = data[z].Month;
                        }
                            if (data[z].Tave < tMin)
                                tMin = data[z].Tave;
                        }
                    }
                    float annualTavg = tSum / mCount;
                    float annualPcpAvg = pSum / mCount;
                    float tAmplitude = (tMax - tMin) / 2;

                    // Calculation of diffusivity from climate data (Soil thawing_Campbell_121719.xlsx, Fit_Diffusivity_Climate.R)
                    //if(this.AbovegroundBiomassSum >= permafrostMinVegBiomass) // Vegetated
                    //    soilDiffusivity = (float)Math.Max(0.006, (-0.09674 - 0.005967 * annualTavg + 0.00001552 * annualTavg * annualTavg + 0.002395 * annualPcpAvg)); // Diffusivity (m2/day)
                    //else // Bare
                    //    soilDiffusivity = (float)Math.Max(0.006,(-0.050839 - 0.0055182 * annualTavg - 0.0002212 * annualTavg * annualTavg + 0.0011535 * annualPcpAvg)); // Diffusivity (m2/day)

                    while (testDepth <= (maxDepth / 1000.0))
                        {
                            float DRz = (float)Math.Exp(-1.0F * testDepth * d); // adapted from Kang et al. (2000) and Liang et al. (2014)
                                                                            //float zTemp = annualTavg + (tempBelowSnow - annualTavg) * DRz;

                        float zTemp = (float)(annualTavg + tAmplitude * DRz_snow * DRz_moss * DRz * Math.Sin(Constants.omega * (Ecoregion.Variables.Month + (maxMonth + 1)) - testDepth / d));

                            depthTempDict[testDepth] = zTemp;
                        if ((zTemp <= 0) && (testDepth < topFreezeDepth))
                            topFreezeDepth = testDepth;
                            testDepth += 0.25F;
                        }
                    propRootAboveFrost = Math.Min(1, (topFreezeDepth * 1000)/Ecoregion.RootingDepth);
                    float propRootBelowFrost = 1 - propRootAboveFrost;
                    propThawed = Math.Max(0,propRootAboveFrost - (1-lastPropBelowFrost));
                    float propNewFrozen = Math.Max(0, propRootBelowFrost - lastPropBelowFrost);
                    if (propRootAboveFrost < 1) // If part of the rooting zone is frozen
                    {
                        if (propNewFrozen > 0)  // freezing
                        {
                            float newFrozenSoil = propNewFrozen * Ecoregion.RootingDepth;
                            bool successpct = hydrology.SetFrozenWaterPct (((hydrology.FrozenDepth * hydrology.FrozenWaterPct) + (newFrozenSoil * hydrology.Water)) / (hydrology.FrozenDepth + newFrozenSoil));
                            bool successdepth = hydrology.SetFrozenDepth(Ecoregion.RootingDepth * propRootBelowFrost); // Volume of rooting soil that is frozen
                            // Water is a volumetric value (mm/m) so frozen water does not need to be removed, as the concentration stays the same
                        }
                    }
                    if (propThawed > 0) // thawing
                    {
                        // Thawing soil water added to existing water - redistributes evenly in active soil
                        float existingWater = (1-lastPropBelowFrost) * hydrology.Water;
                        float thawedWater = propThawed * hydrology.FrozenWaterPct;
                        float newWaterContent = (existingWater + thawedWater) / propRootAboveFrost;
                        hydrology.AddWater(newWaterContent - hydrology.Water);
                        bool successdepth = hydrology.SetFrozenDepth(Ecoregion.RootingDepth * propRootBelowFrost);  // Volume of rooting soil that is frozen
                    }
                    float leakageFrostReduction = 1.0F;
                    if ((topFreezeDepth*1000) < (Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth))
                    {
                        if ((topFreezeDepth*1000) < Ecoregion.RootingDepth)
                        {
                            leakageFrostReduction = 0.0F;
                        }
                        else
                        {
                            leakageFrostReduction = (Math.Min((topFreezeDepth*1000), Ecoregion.LeakageFrostDepth) - Ecoregion.RootingDepth) / (Ecoregion.LeakageFrostDepth - Ecoregion.RootingDepth);
                        }
                    }
                    leakageFrac = Ecoregion.LeakageFrac * leakageFrostReduction;
                    lastPropBelowFrost = propRootBelowFrost;
                   
                }
                // permafrost
                //float frostFreeProp = Math.Min(1.0F, frostFreeSoilDepth / Ecoregion.RootingDepth);

                AllCohorts.ForEach(x => x.InitializeSubLayers());

                if (Ecoregion.Variables.Prec < 0) throw new System.Exception("Error, this.Ecoregion.Variables.Prec = " + Ecoregion.Variables.Prec + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                float snowmelt = Math.Min(snowPack, ComputeMaxSnowMelt(Ecoregion.Variables.Tave, Ecoregion.Variables.DaySpan)); // mm
                if (snowmelt < 0) throw new System.Exception("Error, snowmelt = " + snowmelt + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                float newsnow = CumputeSnowFraction(Ecoregion.Variables.Tave) * Ecoregion.Variables.Prec;
                float newsnowpack = newsnow * (1 - Ecoregion.SnowSublimFrac); // (mm) Account for sublimation here
                if (newsnowpack < 0 || newsnowpack > Ecoregion.Variables.Prec)
                {
                    throw new System.Exception("Error, newsnowpack = " + newsnowpack + " availablePrecipitation = " + Ecoregion.Variables.Prec);
                }

                snowPack += newsnowpack - snowmelt;
                if (snowPack < 0) throw new System.Exception("Error, snowPack = " + snowPack + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                float newrain = Ecoregion.Variables.Prec - newsnow;

                // Reduced by interception
                interception = newrain * (float)(1 - Math.Exp(-1 * Ecoregion.PrecIntConst * CanopyLAI));
                float surfaceRain = newrain - interception;

                // Reduced by PrecLossFrac
                precLoss = surfaceRain * Ecoregion.PrecLossFrac;
                float precin = surfaceRain  - precLoss;

                if (precin < 0) throw new System.Exception("Error, precin = " + precin + " newsnow = " + newsnow + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                int numEvents = Ecoregion.PrecipEvents;  // maximum number of precipitation events per month
                float PrecInByEvent = precin / numEvents;  // Divide precip into discreet events within the month
                if (PrecInByEvent < 0) throw new System.Exception("Error, PrecInByEvent = " + PrecInByEvent + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);

                //permafrost - assume melting permafrost water is available in the same way as precip
                // melting permafrost water - assumes soil at field capacity when thawing
                //float thawedWater = thawedDepth * (Ecoregion.FieldCap / Ecoregion.RootingDepth);
                
                // melting permafrost water - uses water % stored from when it froze
                //float thawedWater = thawedDepth * Hydrology.FrozenWaterPct;
                

                   if (propRootAboveFrost >= 1)
                    {                    
                        bool successpct = hydrology.SetFrozenWaterPct(0F);
                        bool successdepth = hydrology.SetFrozenDepth(0F);
                    }
                float MeltInWater = snowmelt;
                
                // Randomly choose which layers will receive the precip events
                // If # of layers < precipEvents, some layers will show up multiple times in number list.  This ensures the same number of precip events regardless of the number of cohorts
                List<int> randomNumbers = new List<int>();
                while (randomNumbers.Count < numEvents)
                {
                    int rand = PlugIn.DiscreteUniformRandom(1, SubCanopyCohorts.Count());
                    randomNumbers.Add(rand);
                }
                var groupList = randomNumbers.GroupBy(i => i);

                // Reset Hydrology values
                Hydrology.RunOff = 0;
                Hydrology.Leakage = 0;
                Hydrology.Evaporation = 0;
                
                

                float O3_ppmh = Ecoregion.Variables.O3 / 1000; // convert AOT40 units to ppm h
                float lastO3 = 0;
                if(m > 0)
                    lastO3 = (data[m-1].O3/1000f);
                float O3_ppmh_month = Math.Max(0,O3_ppmh - lastO3);

                List<ISpeciesPNET> species = PlugIn.SpeciesPnET.AllSpecies.ToList();
                Dictionary<string, float> DelAmax_spp = new Dictionary<string, float>();
                Dictionary<string, float> JCO2_spp = new Dictionary<string, float>();
                Dictionary<string, float> Amax_spp = new Dictionary<string, float>();
                Dictionary<string, float> FTempPSNRefNetPSN_spp = new Dictionary<string, float>();
                Dictionary<string, float> Ca_Ci_spp = new Dictionary<string, float>();

                float subCanopyPrecip = 0;
                float subCanopyMelt = 0;
                int subCanopyIndex = 0;
                if (bins != null)
                {
                    for (int b = bins.Count() - 1; b >= 0; b--)
                    {
                        foreach (int r in random_range[b])
                        {
                            subCanopyIndex++;
                            int precipCount = 0;
                            subCanopyPrecip = 0;
                            subCanopyMelt = MeltInWater / SubCanopyCohorts.Count();
                            bool coldKillBoolean = false;
                            foreach (var g in groupList)
                            {
                                if (g.Key == subCanopyIndex)
                                {
                                    precipCount = g.Count();
                                    subCanopyPrecip = PrecInByEvent;
                                }
                            }
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            ISpeciesPNET spc = c.SpeciesPNET;
                            if (coldKillMonth[spc] == m)
                                coldKillBoolean = true;
                            float O3Effect = lastOzoneEffect[subCanopyIndex - 1];

                            success = c.CalculatePhotosynthesis(subCanopyPrecip, precipCount, leakageFrac, hydrology, ref subcanopypar, O3_ppmh, O3_ppmh_month, subCanopyIndex, SubCanopyCohorts.Count(), ref O3Effect, propRootAboveFrost, subCanopyMelt, coldKillBoolean);
                            lastOzoneEffect[subCanopyIndex - 1] = O3Effect;

                            if (success == false)
                            {
                                throw new System.Exception("Error CalculatePhotosynthesis");
                            }
                            // A cohort cannot be reduced to a lower layer once it reaches a higher layer
                            //if (c.Layer > bins.Count())
                            //    c.Layer = (byte)bins.Count();
                            c.Layer = (byte)Math.Max(b, c.Layer);
                        }
                    }
                }
                else // When no cohorts are present
                {
                    if (MeltInWater > 0)
                    {
                        // Add thawed soil water to soil moisture
                        // Instantaneous runoff (excess of porosity)
                        float waterCapacity = Ecoregion.Porosity * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                        float meltrunoff = Math.Min(MeltInWater, Math.Max(hydrology.Water * Ecoregion.RootingDepth * propRootAboveFrost + MeltInWater - waterCapacity, 0));
                        
                        //if ((hydrology.Water + meltrunoff) > (Ecoregion.Porosity + Ecoregion.RunoffCapture))
                        //    meltrunoff = (hydrology.Water + meltrunoff) - (Ecoregion.Porosity + Ecoregion.RunoffCapture);
                        Hydrology.RunOff += meltrunoff;

                        success = hydrology.AddWater(MeltInWater - meltrunoff, Ecoregion.RootingDepth * propRootAboveFrost);
                        if (success == false) throw new System.Exception("Error adding water, MeltInWaterr = " + MeltInWater + "; water = " + hydrology.Water + "; meltrunoff = " + meltrunoff + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                        float capturedRunoff = 0;
                        if ((Ecoregion.RunoffCapture > 0) & (meltrunoff > 0))
                        {
                            capturedRunoff = Math.Max(0, Math.Min(meltrunoff, (Ecoregion.RunoffCapture - Hydrology.SurfaceWater)));
                            Hydrology.SurfaceWater += capturedRunoff;
                        }
                        Hydrology.RunOff += (meltrunoff - capturedRunoff);
                    }
                    if (precin > 0)
                    {
                    // Instantaneous runoff (excess of porosity)
                        float waterCapacity = Ecoregion.Porosity * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                        float rainrunoff = Math.Min(precin, Math.Max(hydrology.Water * Ecoregion.RootingDepth * propRootAboveFrost + precin - waterCapacity, 0));
                        //if ((hydrology.Water + rainrunoff) > (ecoregion.Porosity + ecoregion.RunoffCapture))
                        //    rainrunoff = (hydrology.Water + rainrunoff) - (ecoregion.Porosity + ecoregion.RunoffCapture);
                        float capturedRunoff = 0;
                        if ((Ecoregion.RunoffCapture > 0) & (rainrunoff > 0))
                        {
                            capturedRunoff = Math.Max(0, Math.Min(rainrunoff, (Ecoregion.RunoffCapture - Hydrology.SurfaceWater)));
                            Hydrology.SurfaceWater += capturedRunoff;
                        }
                        Hydrology.RunOff += (rainrunoff - capturedRunoff);

                        float precipIn = precin - rainrunoff; //mm

                        // Add incoming precipitation to soil moisture
                        success = hydrology.AddWater(precipIn, Ecoregion.RootingDepth * propRootAboveFrost);
                        if (success == false) throw new System.Exception("Error adding water, waterIn = " + precipIn + "; water = " + hydrology.Water + "; rainrunoff = " + rainrunoff + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                    }

                    // Fast Leakage
                    float leakage = Math.Max((float)leakageFrac * (hydrology.Water - Ecoregion.FieldCap), 0) * Ecoregion.RootingDepth * propRootAboveFrost; //mm
                    Hydrology.Leakage += leakage;

                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * leakage, Ecoregion.RootingDepth * propRootAboveFrost);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + Hydrology.Leakage + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                    if (Hydrology.SurfaceWater > 0)
                    {
                        float surfaceInput = Math.Min(Hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.Water));
                        Hydrology.SurfaceWater -= surfaceInput;
                        success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                        if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + Hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                }
                }

                float LAISum = 0;
                AllCohorts.ForEach(x =>
                    {
                        folresp[Ecoregion.Variables.Month - 1] += x.FolResp.Sum();
                        netpsn[Ecoregion.Variables.Month - 1] += x.NetPsn.Sum();
                        grosspsn[Ecoregion.Variables.Month - 1] += x.GrossPsn.Sum();
                        maintresp[Ecoregion.Variables.Month - 1] += x.MaintenanceRespiration.Sum();
                        LAISum += x.LAI.Sum();
                        transpiration += x.Transpiration.Sum();
                    }
                );
                CanopyLAI = LAISum;
                // Surface PAR is effectively 0 when snowpack is present
                if (snowPack > 0)
                    subcanopypar = 0;

                canopylaimax = (byte)Math.Max(canopylaimax, CanopyLAI);
                watermax = Math.Max(hydrology.Water, watermax);                
                subcanopyparmax = Math.Max(subcanopyparmax, subcanopypar);
                if (propRootAboveFrost > 0)
                {
                    Hydrology.Evaporation = hydrology.CalculateEvaporation(this); //mm
                }else
                {
                    Hydrology.Evaporation = 0;
                }
                success = hydrology.AddWater(-1 * Hydrology.Evaporation, Ecoregion.RootingDepth * propRootAboveFrost);
                if (success == false)
                {
                    throw new System.Exception("Error adding water, evaporation = " + Hydrology.Evaporation + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                }
                if ((Hydrology.SurfaceWater > 0) & (hydrology.Water < Ecoregion.Porosity))
                {
                    float surfaceInput = Math.Min(Hydrology.SurfaceWater, ((Ecoregion.Porosity - hydrology.Water)* Ecoregion.RootingDepth * propRootAboveFrost));
                    Hydrology.SurfaceWater -= surfaceInput;
                    success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + Hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
                }

                if (siteoutput != null)
                {
                    AddSiteOutput(data[m]);

                    AllCohorts.ForEach(a => a.UpdateCohortData(data[m]));
                }
                if (data[m].Tave > 0)
                {
                    sumPressureHead += hydrology.GetPressureHead(Ecoregion);
                    countPressureHead += 1;
                }
                if (PlugIn.ModelCore.CurrentTime > 0)
                {
                    monthlyEstab = establishmentProbability.Calculate_Establishment_Month(data[m], Ecoregion, subcanopypar, hydrology, minHalfSat, maxHalfSat, invertPest);

                    foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
                    {
                        if (monthlyEstab.ContainsKey(spc))
                        {
                            //annualEstab[spc] = annualEstab[spc] + monthlyEstab[spc];
                            // Calculate the cumulative probability that no months had successful establishment (later transformed)
                            //annualEstab[spc] = annualEstab[spc] * (1- monthlyEstab[spc]);

                            // Store monthly values for later filtering
                            annualEstab[spc].Add(monthlyEstab[spc]);
                            annualFwater[spc].Add(establishmentProbability.Get_FWater(spc));
                            annualFrad[spc].Add(establishmentProbability.Get_FRad(spc));                            
                        }
                    }

                }
                // Store growing season FRad values                
                AllCohorts.ForEach(x => x.StoreFRad());
                // Reset all cohort values
                AllCohorts.ForEach(x => x.NullSubLayers());

                //  Processes that happen only once per year
                if (data[m].Month == (int)Constants.Months.December)
                {
                    //  Decompose litter
                    HeterotrophicRespiration = (ushort)(PlugIn.Litter[Site].Decompose() + PlugIn.WoodyDebris[Site].Decompose());

                    // Calculate AdjFolFrac
                    AllCohorts.ForEach(x => x.CalcAdjFracFol());

                    // Filter monthly pest values
                    // This assumes up to 3 months of growing season are relevant for establishment
                    // When > 3 months of growing season, exlcude 1st month, assuming trees focus on foliage growth in first month
                    // When > 4 months, ignore the 4th month and beyond as not primarily relevant for establishment
                    // When < 3 months, include all months
                    foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
                    {
                        if (annualEstab[spc].Count > 3)
                        {
                            cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][1]) * (1 - annualEstab[spc][2]) * (1 - annualEstab[spc][3]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][1] + annualFwater[spc][2] + annualFwater[spc][3];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][1] + annualFrad[spc][2] + annualFrad[spc][3];
                            monthlyCount[spc] = monthlyCount[spc] + 3;
                }
                        else if(annualEstab[spc].Count > 2)
                        {
                            cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]) * (1 - annualEstab[spc][1]) * (1 - annualEstab[spc][2]) ;
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0] + annualFwater[spc][1] + annualFwater[spc][2];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0] + annualFrad[spc][1] + annualFrad[spc][2];
                            monthlyCount[spc] = monthlyCount[spc] + 3;
            }
                        else if(annualEstab[spc].Count > 1)
                        {
                            cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]) * (1 - annualEstab[spc][1]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0] + annualFwater[spc][1] ;
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0] + annualFrad[spc][1] ;
                            monthlyCount[spc] = monthlyCount[spc] + 2;
                        }
                        else if(annualEstab[spc].Count == 1)
                        {
                            cumulativeEstab[spc] = cumulativeEstab[spc] * (1 - annualEstab[spc][0]);
                            cumulativeFwater[spc] = cumulativeFwater[spc] + annualFwater[spc][0];
                            cumulativeFrad[spc] = cumulativeFrad[spc] + annualFrad[spc][0];
                            monthlyCount[spc] = monthlyCount[spc] + 1;
                        }

                        //Reset annual lists for next year
                        annualEstab[spc].Clear();
                        annualFwater[spc].Clear();
                        annualFrad[spc].Clear();                        
                    }
                }
            } //for (int m = 0; m < data.Count(); m++ )
            if (PlugIn.ModelCore.CurrentTime > 0)
            {
                foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
                {
                    bool estab = false;
                    float pest = 0;
                    if (monthlyCount[spc] > 0)
                    {
                        //annualEstab[spc] = annualEstab[spc] / monthlyCount[spc];
                        // Transform cumulative probability of no successful establishments to probability of at least one successful establishment
                        cumulativeEstab[spc] = 1 - cumulativeEstab[spc] ;
                        cumulativeFwater[spc] = cumulativeFwater[spc] / monthlyCount[spc];
                        cumulativeFrad[spc] = cumulativeFrad[spc] / monthlyCount[spc];

                        // Modify Pest by maximum value
                        //pest = cumulativeEstab[spc] * spc.MaxPest;

                        // Calculate Pest from average Fwater, Frad and modified by MaxPest
                        pest = cumulativeFwater[spc] * cumulativeFrad[spc] * spc.MaxPest;
                    }
                    
                    if (!spc.PreventEstablishment)
                    {

                        if (pest > (float)PlugIn.ContinuousUniformRandom())
                        {
                            establishmentProbability.EstablishmentTrue(spc);
                            estab = true;

                        }
                    }
                    EstablishmentProbability.RecordPest(PlugIn.ModelCore.CurrentTime, spc, pest, cumulativeFwater[spc], cumulativeFrad[spc], estab, monthlyCount[spc]);

                }
            }

            if (siteoutput != null)
            {
                siteoutput.Write();

                AllCohorts.ForEach(cohort => { cohort.WriteCohortData(); });
            }
            float avgPH = sumPressureHead / countPressureHead;
            PlugIn.PressureHead[Site] = avgPH;
            
            RemoveMarkedCohorts();

            //HeterotrophicRespiration = (ushort)(PlugIn.Litter[Site].Decompose() + PlugIn.WoodyDebris[Site].Decompose());//Moved within m loop to trigger once per year

            return success;
        }

        private void CalculateInitialWater(DateTime StartDate)
        {
            canopylaimax = byte.MinValue;

            SortedDictionary<double, Cohort> SubCanopyCohorts = new SortedDictionary<double, Cohort>();

            List<int> cohortAges = new List<int>();

            for (int cohort = 0; cohort < AllCohorts.Count(); cohort++)
            {
                for (int i = 1; i <= PlugIn.IMAX; i++)
                {
                    double CumCohortBiomass = ((float)i / (float)PlugIn.IMAX) * AllCohorts[cohort].TotalBiomass;
                    while (SubCanopyCohorts.ContainsKey(CumCohortBiomass))
                    {
                        // Add a negligable value [-1e-10; + 1e-10] to CumCohortBiomass in order to prevent duplicate keys
                        double k = 1e-10 * 2.0 * (PlugIn.ContinuousUniformRandom() - 0.5);
                        CumCohortBiomass += k;
                    }
                    SubCanopyCohorts.Add(CumCohortBiomass, AllCohorts[cohort]);
                }
                if (!cohortAges.Contains(AllCohorts[cohort].Age))
                {
                    cohortAges.Add(AllCohorts[cohort].Age);
                }
            }


            List<List<int>> rawBins = GetBins(new List<double>(SubCanopyCohorts.Keys));

            // Sort through bins to put cohort sublayers in the same bin based on majority
            List<List<int>> bins = new List<List<int>>();
            if ((rawBins != null) && (rawBins.Count > 1))
            {
                Dictionary<string, Dictionary<int, int>> speciesLayerIndex = new Dictionary<string, Dictionary<int, int>>();
                List<int> addedValues = new List<int>();
                foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
                {
                    foreach (int thisAge in cohortAges)
                    {
                        Dictionary<int, double> sumBio = new Dictionary<int, double>();
                        for (int i = 0; i < rawBins.Count(); i++)
                        {
                            double sumLayerBio = 0;
                            List<int> binLayers = rawBins[i];
                            for (int b = 0; b < binLayers.Count(); b++)
                            {
                                int layerKey = binLayers[b];
                                Cohort layerCohort = SubCanopyCohorts.Values.ToArray()[layerKey];
                                if ((layerCohort.SpeciesPNET.Name == spc.Name) && (layerCohort.Age == thisAge))
                                {
                                    sumLayerBio += ((double)layerCohort.TotalBiomass) / ((double)PlugIn.IMAX);
                                }
                            }
                            sumBio.Add(i, sumLayerBio);
                        }

                        int layerMaxBio = sumBio.LastOrDefault(x => x.Value == sumBio.Values.Max()).Key;

                        if (sumBio.Values.Max() > 0)
                        {
                            if (speciesLayerIndex.Keys.Contains(spc.Name))
                            {
                                Dictionary<int, int> ageIndex = speciesLayerIndex[spc.Name];
                                ageIndex.Add(thisAge, layerMaxBio);
                                speciesLayerIndex[spc.Name] = ageIndex;
                            }
                            else
                            {
                                Dictionary<int, int> ageIndex = new Dictionary<int, int>();
                                ageIndex.Add(thisAge, layerMaxBio);
                                speciesLayerIndex.Add(spc.Name, ageIndex);
                            }
                            addedValues.Add(layerMaxBio);
                        }
                    }
                }
                //step through subcanopycohorts
                int subLayerKey = 0;

                // There shouldn't be more layers than cohorts
                int numberOfLayers = Math.Min(addedValues.Max() + 1, (int)((float)SubCanopyCohorts.Count() / (float)PlugIn.IMAX));

                // Final layer indices should not skip any layers
                List<int> distinctValues = addedValues.Distinct().ToList();
                distinctValues.Sort();
                Dictionary<int, int> indexLookup = new Dictionary<int, int>();
                for (int j = 0; j < distinctValues.Count(); j++)
                {
                    indexLookup.Add(distinctValues[j], j);
                }

                for (int i = 0; i < numberOfLayers; i++)
                {
                    bins.Add(new List<int>());
                }
                foreach (KeyValuePair<double, Cohort> entry in SubCanopyCohorts)
                {
                    ISpecies spc = entry.Value.SpeciesPNET;
                    int thisAge = entry.Value.Age;
                    int tempIndex = indexLookup[speciesLayerIndex[spc.Name][thisAge]];
                    int layerIndex = Math.Max(tempIndex, entry.Value.Layer);  //Once a cohort reaches a canopy layer it cannot be dropped below that position
                    if (layerIndex > MaxCanopyLayers - 1)
                    {
                        throw new System.Exception("layerIndex  " + layerIndex + " is greater than MaxCanopyLayers - 1: " + (MaxCanopyLayers - 1));
                    }
                    if (bins.ElementAtOrDefault(layerIndex) == null)
                    {
                        while (bins.ElementAtOrDefault(layerIndex) == null)
                        {
                            bins.Add(new List<int>());
                        }
                    }
                    bins[layerIndex].Add(subLayerKey);

                    subLayerKey += 1;
                }

            }
            else
            {
                bins = rawBins;
            }

            List<List<int>> random_range = GetRandomRange(bins);

            List<IEcoregionPnETVariables> climate_vars = EcoregionPnET.GetData(Ecoregion, StartDate, StartDate.AddMonths(1));

            if (climate_vars != null && climate_vars.Count > 0)
            {
                this.Ecoregion.Variables = climate_vars.First();
            }
            else
            {
                return;
            }
            transpiration = 0;
            subcanopypar = this.Ecoregion.Variables.PAR0;
            interception = 0;

            AllCohorts.ForEach(x => x.InitializeSubLayers());

            if (this.Ecoregion.Variables.Prec < 0) throw new System.Exception("Error, this.Ecoregion.Variables.Prec = " + this.Ecoregion.Variables.Prec);

            float snowmelt = Math.Min(snowPack, ComputeMaxSnowMelt(this.Ecoregion.Variables.Tave, this.Ecoregion.Variables.DaySpan)); // mm
            if (snowmelt < 0) throw new System.Exception("Error, snowmelt = " + snowmelt);

            float newsnow = CumputeSnowFraction(this.Ecoregion.Variables.Tave) * this.Ecoregion.Variables.Prec;
            float newsnowpack = newsnow * (1 - this.Ecoregion.SnowSublimFrac); // (mm) Account for sublimation here
            if (newsnowpack < 0 || newsnowpack > this.Ecoregion.Variables.Prec)
            {
                throw new System.Exception("Error, newsnowpack = " + newsnowpack + " availablePrecipitation = " + this.Ecoregion.Variables.Prec);
            }

            snowPack += newsnowpack - snowmelt;
            if (snowPack < 0) throw new System.Exception("Error, snowPack = " + snowPack);

            float newrain = this.Ecoregion.Variables.Prec - newsnow;

            // Reduced by interception
            interception = newrain * (float)(1 - Math.Exp(-1 * this.Ecoregion.PrecIntConst * CanopyLAI));
            float surfaceRain = newrain - interception;

            // Reduced by PrecLossFrac
            precLoss = surfaceRain * this.Ecoregion.PrecLossFrac;
            float availableRain = surfaceRain - precLoss;

            float precin = availableRain + snowmelt;
            if (precin < 0) throw new System.Exception("Error, precin = " + precin + " newsnow = " + newsnow + " snowmelt = " + snowmelt);

            int numEvents = this.Ecoregion.PrecipEvents;  // maximum number of precipitation events per month
            float PrecInByEvent = precin / numEvents;  // Divide precip into discreet events within the month
            if (PrecInByEvent < 0) throw new System.Exception("Error, PrecInByEvent = " + PrecInByEvent);

            // Randomly choose which layers will receive the precip events
            // If # of layers < precipEvents, some layers will show up multiple times in number list.  This ensures the same number of precip events regardless of the number of cohorts
            List<int> randomNumbers = new List<int>();
            while (randomNumbers.Count < numEvents)
            {
                int rand = PlugIn.DiscreteUniformRandom(1, SubCanopyCohorts.Count());
                randomNumbers.Add(rand);
            }
            var groupList = randomNumbers.GroupBy(i => i);

            // Reset Hydrology values
            Hydrology.RunOff = 0;
            Hydrology.Leakage = 0;
            Hydrology.Evaporation = 0;

            float subCanopyPrecip = 0;
            int subCanopyIndex = 0;
            if (bins != null)
            {
                for (int b = bins.Count() - 1; b >= 0; b--)
                {
                    foreach (int r in random_range[b])
                    {
                        subCanopyIndex++;
                        int precipCount = 0;
                        subCanopyPrecip = 0;
                        foreach (var g in groupList)
                        {
                            if (g.Key == subCanopyIndex)
                            {
                                precipCount = g.Count();
                                subCanopyPrecip = PrecInByEvent;
                            }
                        }
                        Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                        ISpeciesPNET spc = c.SpeciesPNET;

        

                        // A cohort cannot be reduced to a lower layer once it reaches a higher layer
                        //if (c.Layer > bins.Count())
                        //    c.Layer = (byte)bins.Count();
                        c.Layer = (byte)Math.Max(b, c.Layer);
                    }
                }
            }
            else // When no cohorts are present
            {
                return;
            }

            // Surface PAR is effectively 0 when snowpack is present
            if (snowPack > 0)
                subcanopypar = 0;

            canopylaimax = (byte)Math.Max(canopylaimax, CanopyLAI);
            watermax = Math.Max(hydrology.Water, watermax);
            subcanopyparmax = Math.Max(subcanopyparmax, subcanopypar);

            if (propRootAboveFrost > 0)
            {
                Hydrology.Evaporation = hydrology.CalculateEvaporation(this); //mm
            }else
            {
                Hydrology.Evaporation = 0;
        }
            bool success = hydrology.AddWater(-1 * Hydrology.Evaporation, Ecoregion.RootingDepth * propRootAboveFrost);
            if (success == false)
            {
                throw new System.Exception("Error adding water, evaporation = " + Hydrology.Evaporation + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
            }
            if (Hydrology.SurfaceWater > 0)
            {
                float surfaceInput = Math.Min(Hydrology.SurfaceWater, (Ecoregion.Porosity - hydrology.Water));
                Hydrology.SurfaceWater -= surfaceInput;
                success = hydrology.AddWater(surfaceInput, Ecoregion.RootingDepth * propRootAboveFrost);
                if (success == false) throw new System.Exception("Error adding water, Hydrology.SurfaceWater = " + Hydrology.SurfaceWater + "; water = " + hydrology.Water + "; ecoregion = " + Ecoregion.Name + "; site = " + Site.Location);
            }
        }

        
        public float[] MaintResp
        {
            get
            {
                if (maintresp == null)
                {
                    float[] maintresp_array = new float[12];
                    for (int i = 0; i < maintresp_array.Length; i++)
                    {
                        maintresp_array[i] = 0;
                    }
                    return maintresp_array;
                }
                else
                {
                    return maintresp.Select(r => (float)r).ToArray();
                }
            }
        }

        public float[] FolResp
        {
            get
            {
                if (folresp == null)
                {
                    float[] folresp_array = new float[12];
                    for (int i = 0; i < folresp_array.Length; i++)
                    {
                        folresp_array[i] = 0;
                    }
                    return folresp_array;
                }
                else
                {
                    return folresp.Select(psn => (float)psn).ToArray();
                }
            }
        }
        public float[] GrossPsn
        {
            get
            {
                if (grosspsn == null)
                {
                    float[] grosspsn_array = new float[12];
                    for (int i = 0; i < grosspsn_array.Length; i++)
                    {
                        grosspsn_array[i] = 0;
                    }
                    return grosspsn_array;
                }
                else
                {
                    return grosspsn.Select(psn => (float)psn).ToArray();
                }
            }
        }

        //public float[] NetPsn
        //{
        //    get
        //    {
        //        if (netpsn == null)
        //        {
        //            float[] netpsn_array = new float[12];
        //            for (int i = 0; i < netpsn_array.Length; i++)
        //            {
        //                netpsn_array[i] = 0;
        //            }
        //            return netpsn_array;
        //        }
        //        else
        //        {
        //            return netpsn.Select(psn => (float)psn).ToArray();
        //        }
        //    }
        //}

        public float NetPsnSum
        {
            get
            {
                if (netpsn == null)
                {
                    float[] netpsn_array = new float[12];
                    for (int i = 0; i < netpsn_array.Length; i++)
                    {
                        netpsn_array[i] = 0;
                    }
                    return netpsn_array.Sum();
                }
                else
                {
                    return netpsn.Select(psn => (float)psn).ToArray().Sum();
                }
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
                Landis.Library.Parameters.Species.AuxParm<int> BiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    BiomassPerSpecies[spc] = cohorts[spc].Sum(o => o.TotalBiomass);
                }
                return BiomassPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies
        {
            get
            {
                Landis.Library.Parameters.Species.AuxParm<int> AbovegroundBiomassPerSpecies = new Library.Parameters.Species.AuxParm<int>(PlugIn.ModelCore.Species);

                foreach (ISpecies spc in cohorts.Keys)
                {
                    AbovegroundBiomassPerSpecies[spc] = cohorts[spc].Sum(o => o.Biomass);
                }
                return AbovegroundBiomassPerSpecies;
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<int> WoodySenescencePerSpecies
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
        }
        public Landis.Library.Parameters.Species.AuxParm<int> FoliageSenescencePerSpecies
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
                return AllCohorts.Sum(o => o.TotalBiomass);
            }

        }
        public float AbovegroundBiomassSum
        {
            get
            {
                return AllCohorts.Sum(o => o.Biomass);
            }

        }
        public float WoodySenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => o.LastWoodySenescence);
            }

        }
        public float FoliageSenescenceSum
        {
            get
            {
                return AllCohorts.Sum(o => o.LastFoliageSenescence);
            }

        }
        public uint BelowGroundBiomass 
        {
            get
            {
                return (uint)cohorts.Values.Sum(o => o.Sum(x => x.Root));
            }
        }

        public float FoliageSum
        {
            get
            {
                return AllCohorts.Sum(o => o.Fol);
            }

        }

        public float NSCSum
        {
            get
            {
                return AllCohorts.Sum(o => o.NSC);
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
                    Landis.Library.BiomassCohorts.ICohort cohort = species_cohort[c];

                    // Disturbances return reduction in aboveground biomass
                    int _reduction = disturbance.ReduceOrKillMarkedCohort(cohort);

                    reduction.Add(_reduction);
                    if (reduction[reduction.Count() - 1] >= species_cohort[c].Biomass)  //Compare to aboveground biomass
                    {
                        ToRemove.Add(species_cohort[c]);
                        // Edited by BRM - 090115
                    }
                    else
                    {
                        double reductionProp = (double)reduction[reduction.Count() - 1] / (double)species_cohort[c].Biomass;  //Proportion of aboveground biomass
                        species_cohort[c].ReduceBiomass(this, reductionProp, disturbance.Type);  // Reduction applies to all biomass
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

            // Does this only occur when a site is disturbed?
            //Allocation.ReduceDeadPools(this, disturbance.Type); 

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
                Landis.Library.BiomassCohorts.Cohort.KilledByAgeOnlyDisturbance(disturbance, cohort, disturbance.CurrentSite, disturbance.Type);
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
            ISpeciesPNET pnetSpecies = PlugIn.SpeciesPnET[species];

            bool speciesPresent = cohorts.ContainsKey(species);

            bool IsMaturePresent = (speciesPresent && (cohorts[species].Max(o => o.Age) >= species.Maturity)) ? true : false;

            return IsMaturePresent;
        }

        public bool AddNewCohort(Cohort newCohort)
        {
            bool addCohort = false;
            if (cohorts.ContainsKey(newCohort.Species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies, List<Cohort>>>(cohorts.Where(o => o.Key == newCohort.Species))[0];

                List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < CohortBinSize));
                //List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < Timestep));

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
                    return addCohort;
                }
                else
                {

                    cohorts[newCohort.Species].Add(newCohort);
                    addCohort = true;
                    return addCohort;
                }

            }
            cohorts.Add(newCohort.Species, new List<Cohort>(new Cohort[] { newCohort }));
            addCohort = true;
            return addCohort;
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
        public void RemoveWoodyDebris(double percentReduction)
        {
            PlugIn.WoodyDebris[Site].ReduceMass(percentReduction);
        }
        public void AddLitter(float AddLitter, ISpeciesPNET spc)
        {
            
            double KNwdLitter = Math.Max(0.3, (-0.5365 + (0.00241 * AET.Sum())) - (((-0.01586 + (0.000056 * AET.Sum())) * spc.FolLignin * 100)));

            PlugIn.Litter[Site].AddMass(AddLitter, KNwdLitter);
        }
        public void RemoveLitter(double percentReduction)
        {
            PlugIn.Litter[Site].ReduceMass(percentReduction);
        }
        string Header(Landis.SpatialModeling.ActiveSite site)
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
                       OutputHeaders.availableWater + "," +
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
                        OutputHeaders.SoilDiffusivity+","+
                        OutputHeaders.FrostDepth+","+
                        OutputHeaders.LeakageFrac;

            return s;
        }

        private void AddSiteOutput(IEcoregionPnETVariables monthdata)
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
                        (hydrology.Water * Ecoregion.RootingDepth * propRootAboveFrost) + "," +  // mm of avialable water
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
                        soilDiffusivity + ","+
                        (topFreezeDepth*1000)+","+
                        leakageFrac;
           
            this.siteoutput.Add(s);
        }
 
        public IEnumerator<Landis.Library.BiomassCohorts.ISpeciesCohorts> GetEnumerator()
        {
            foreach (ISpecies species in cohorts.Keys)
            {
                yield return this[species];
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

             
        }
       

    }


}

