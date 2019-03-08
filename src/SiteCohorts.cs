﻿//  Copyright ...
//  Authors:  Arjan de Bruijn

using Landis.Utilities;
using Landis.Core;
using Landis.Library.InitialCommunities;
using Landis.SpatialModeling;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class SiteCohorts : ISiteCohorts, Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
    {
        private byte canopylaimax;
        private ushort watermax;
        private float snowPack;
        private float CanopyLAI;
        private float subcanopypar;
        private float subcanopyparmax;

        private float[] netpsn = null;
        private float[] grosspsn = null;
        private float[] folresp = null;
        
        private float[] maintresp = null;
        private float transpiration;
        private double HeterotrophicRespiration;
        private ActiveSite Site;
        private Dictionary<ISpecies, List<Cohort>> cohorts = null;
        IEstablishmentProbability establishmentProbability = null;
        private IHydrology hydrology = null;

        public IEcoregionPnET Ecoregion;
        public LocalOutput siteoutput;

        static float[] AET = new float[12]; // mm/mo
        private static IDictionary<uint, SiteCohorts> initialSites;
        private static byte MaxCanopyLayers;
        private static ushort MaxDevLyrAv;
        private static float interception;
        private static float precLoss;
        private static byte Timestep;
        private static int nlayers;

        /// <summary>
        /// Occurs when a site is disturbed by an age-only disturbance.
        /// </summary>
        //public static event Landis.Library.BiomassCohorts.DisturbanceEventHandler AgeOnlyDisturbanceEvent;

        //---------------------------------------------------------------------

        public float Transpiration
        {
            get
            {
                return transpiration;
            }
        }
        public float SubcanopyPAR
        {
            get
            {
                return subcanopypar;
            }
        }
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

        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, string SiteOutputName = null)
        {
            Cohort.SetSiteAccessFunctions(this);
            this.Ecoregion = EcoregionPnET.GetPnETEcoregion(PlugIn.ModelCore.Ecoregion[site]);
            this.Site = site;
            cohorts = new Dictionary<ISpecies, List<Cohort>>();
            uint key = ComputeKey((ushort)initialCommunity.MapCode, PlugIn.ModelCore.Ecoregion[site].MapCode);

            if (initialSites.ContainsKey(key) && SiteOutputName == null)
            {
                establishmentProbability = new EstablishmentProbability(null, null);
                subcanopypar = initialSites[key].subcanopypar;
                subcanopyparmax = initialSites[key].SubCanopyParMAX;
                watermax = initialSites[key].watermax;

                hydrology = new Hydrology((ushort)initialSites[key].hydrology.Water);

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
                List<IEcoregionPnETVariables> ecoregionInitializer = EcoregionPnET.GetData(Ecoregion, StartDate.AddMonths(-1), StartDate);
                hydrology = new Hydrology((ushort)Ecoregion.FieldCap);
                watermax = (ushort)hydrology.Water;
                subcanopypar = ecoregionInitializer[0].PAR0;

                PlugIn.WoodyDebris[Site] = new Library.Biomass.Pool();
                PlugIn.Litter[Site] = new Library.Biomass.Pool();

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
                            AddNewCohort(new Cohort(PlugIn.SpeciesPnET[cohort.Species], cohort.Age, cohort.Biomass, SiteOutputName, (ushort)(StartDate.Year - cohort.Age), ref subcanopypar));
                        }
                    }

                    AllCohorts.ForEach(x =>
                    {
                        CanopyLAI += x.LAI.Sum();
                    }
                    );
                    this.canopylaimax = (byte)CanopyLAI;
                    subcanopyparmax = Math.Max(subcanopypar, ecoregionInitializer[0].PAR0);
                }
                else
                {
                    SpinUp(StartDate, site, initialCommunity, SiteOutputName);
                }
            }
        }

        // Spins up sites if no biomass is provided
        private void SpinUp(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, string SiteOutputName = null)
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

            Landis.Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>> mydata = new Library.Parameters.Ecoregions.AuxParm<List<EcoregionPnETVariables>>(PlugIn.ModelCore.Ecoregions);

            while (date.CompareTo(StartDate) < 0)
            {
                //  Add those cohorts that were born at the current year
                while (sortedAgeCohorts.Count() > 0 && StartDate.Year - date.Year == sortedAgeCohorts[0].Age)
                {
                    Cohort cohort = new Cohort(PlugIn.SpeciesPnET[sortedAgeCohorts[0].Species], (ushort)date.Year, SiteOutputName);

                    AddNewCohort(cohort);

                    sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                }

                // Simulation time runs untill the next cohort is added
                DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - sortedAgeCohorts[0].Age), 1, 15);

                List<IEcoregionPnETVariables> climate_vars = EcoregionPnET.GetData(Ecoregion, date, EndDate);

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

            Dictionary<ISpeciesPNET, float> annualEstab = new Dictionary<ISpeciesPNET, float>();
            Dictionary<ISpeciesPNET, float> monthlyEstab = new Dictionary<ISpeciesPNET, float>();
            Dictionary<ISpeciesPNET, int> monthlyCount = new Dictionary<ISpeciesPNET, int>();
            foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
            {
                annualEstab[spc] = 0;
                monthlyCount[spc] = 0;
            }

            float[] lastOzoneEffect = new float[SubCanopyCohorts.Count()];
            for (int i = 0; i < lastOzoneEffect.Length; i++)
            {
                lastOzoneEffect[i] = 0;
            }

            //int monthCount = 0;
            for (int m = 0; m < data.Count(); m++ )
            {
                this.Ecoregion.Variables = data[m];
                transpiration = 0;
                subcanopypar = this.Ecoregion.Variables.PAR0;                
                interception = 0;

                float ozoneD40 = 0;
                if (m > 0)
                    ozoneD40 = Math.Max(0,this.Ecoregion.Variables.O3 - data[m - 1].O3);
                else
                    ozoneD40 = this.Ecoregion.Variables.O3;
                float O3_D40_ppmh = ozoneD40 / 1000; // convert D40 units to ppm h

                AllCohorts.ForEach(x => x.InitializeSubLayers());

                if (this.Ecoregion.Variables.Prec < 0) throw new System.Exception("Error, this.Ecoregion.Variables.Prec = " + this.Ecoregion.Variables.Prec);

                float snowmelt = Math.Min(snowPack, ComputeMaxSnowMelt(this.Ecoregion.Variables.Tave, this.Ecoregion.Variables.DaySpan)); // mm
                if (snowmelt < 0) throw new System.Exception("Error, snowmelt = " + snowmelt );

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
                float availableRain = surfaceRain  - precLoss;

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
                                if(g.Key == subCanopyIndex)
                                {
                                    precipCount = g.Count();
                                    subCanopyPrecip = PrecInByEvent;
                                }
                            }
                            Cohort c = SubCanopyCohorts.Values.ToArray()[r];
                            ISpeciesPNET spc = c.SpeciesPNET;
                            float O3Effect = lastOzoneEffect[subCanopyIndex - 1];

                            success = c.CalculatePhotosynthesis(subCanopyPrecip,precipCount, Ecoregion.LeakageFrac, hydrology, ref subcanopypar, O3_ppmh, O3_ppmh_month, subCanopyIndex, SubCanopyCohorts.Count(), ref O3Effect);
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
                    // Add incoming precipitation to soil moisture
                    success = hydrology.AddWater(precin);
                    if (success == false) throw new System.Exception("Error adding water, waterIn = " + precin + " water = " + hydrology.Water);

                    // Instantaneous runoff (excess of porosity)
                    Hydrology.RunOff = Math.Max(hydrology.Water - Ecoregion.Porosity, 0);
                    success = hydrology.AddWater(-1 * Hydrology.RunOff);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.RunOff = " + Hydrology.RunOff + " water = " + hydrology.Water);

                    // Fast Leakage 
                    Hydrology.Leakage = Math.Max(Ecoregion.LeakageFrac * (hydrology.Water - Ecoregion.FieldCap), 0);

                    // Remove fast leakage
                    success = hydrology.AddWater(-1 * Hydrology.Leakage);
                    if (success == false) throw new System.Exception("Error adding water, Hydrology.Leakage = " + Hydrology.Leakage + " water = " + hydrology.Water);

                }

                CanopyLAI = 0; // Reset to 0
                AllCohorts.ForEach(x =>
                    {
                        folresp[this.Ecoregion.Variables.Month - 1] += x.FolResp.Sum();
                        netpsn[this.Ecoregion.Variables.Month - 1] += x.NetPsn.Sum();
                        grosspsn[this.Ecoregion.Variables.Month - 1] += x.GrossPsn.Sum() * PlugIn.FTimeStep;
                        maintresp[this.Ecoregion.Variables.Month - 1] += x.MaintenanceRespiration.Sum() * PlugIn.FTimeStep;
                        CanopyLAI += x.LAI.Sum();
                        transpiration += x.Transpiration.Sum();
                    }
                );

                // Surface PAR is effectively 0 when snowpack is present
                if (snowPack > 0)
                    subcanopypar = 0;

                canopylaimax = (byte)Math.Max(canopylaimax, CanopyLAI);
                watermax = (ushort)Math.Max(hydrology.Water, watermax);                
                subcanopyparmax = Math.Max(subcanopyparmax, subcanopypar);

                Hydrology.Evaporation = hydrology.CalculateEvaporation(this);
                success = hydrology.AddWater(-1 * Hydrology.Evaporation);
                if (success == false)
                {
                    throw new System.Exception("Error adding water, evaporation = " + Hydrology.Evaporation + " water = " + hydrology.Water);
                }

                if (siteoutput != null)
                {
                    AddSiteOutput(data[m]);

                    AllCohorts.ForEach(a => a.UpdateCohortData(data[m]));
                }

                if (PlugIn.ModelCore.CurrentTime > 0)
                {
                    monthlyEstab = establishmentProbability.Calculate_Establishment_Month(data[m], Ecoregion, subcanopypar, hydrology);

                    foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
                    {
                        if (monthlyEstab.ContainsKey(spc))
                        {
                            annualEstab[spc] = annualEstab[spc] + monthlyEstab[spc];
                            monthlyCount[spc] = monthlyCount[spc] + 1;
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
                }
            }
            if (PlugIn.ModelCore.CurrentTime > 0)
            {
                foreach (ISpeciesPNET spc in PlugIn.SpeciesPnET.AllSpecies)
                {
                    bool estab = false;
                    annualEstab[spc] = annualEstab[spc] / monthlyCount[spc];
                    float pest = annualEstab[spc];
                    if (!spc.PreventEstablishment)
                    {

                        if (pest > (float)PlugIn.ContinuousUniformRandom())
                        {
                            establishmentProbability.EstablishmentTrue(spc);
                            estab = true;

                        }
                    }
                    EstablishmentProbability.RecordPest(PlugIn.ModelCore.CurrentTime, spc, pest, estab);

                }
            }

            if (siteoutput != null)
            {
                siteoutput.Write();

                AllCohorts.ForEach(cohort => { cohort.WriteCohortData(); });
            }

            RemoveMarkedCohorts();

            //HeterotrophicRespiration = (ushort)(PlugIn.Litter[Site].Decompose() + PlugIn.WoodyDebris[Site].Decompose());//Moved within m loop to trigger once per year

            return success;
        }

        
        public int[] MaintResp
        {
            get
            {
                if (maintresp == null)
                {
                    int[] maintresp_array = new int[12];
                    for (int i = 0; i < maintresp_array.Length; i++)
                    {
                        maintresp_array[i] = 0;
                    }
                    return maintresp_array;
                }
                else
                {
                    return maintresp.Select(r => (int)r).ToArray();
                }
            }
        }

        public int[] FolResp
        {
            get
            {
                if (folresp == null)
                {
                    int[] folresp_array = new int[12];
                    for (int i = 0; i < folresp_array.Length; i++)
                    {
                        folresp_array[i] = 0;
                    }
                    return folresp_array;
                }
                else
                {
                    return folresp.Select(psn => (int)psn).ToArray();
                }
            }
        }
        public int[] GrossPsn
        {
            get
            {
                if (grosspsn == null)
                {
                    int[] grosspsn_array = new int[12];
                    for (int i = 0; i < grosspsn_array.Length; i++)
                    {
                        grosspsn_array[i] = 0;
                    }
                    return grosspsn_array;
                }
                else
                {
                    return grosspsn.Select(psn => (int)psn).ToArray();
                }
            }
        }

        public int[] NetPsn
        {
            get
            {
                if (netpsn == null)
                {
                    int[] netpsn_array = new int[12];
                    for (int i = 0; i < netpsn_array.Length; i++)
                    {
                        netpsn_array[i] = 0;
                    }
                    return netpsn_array;
                }
                else
                {
                    return netpsn.Select(psn => (int)psn).ToArray();
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
            Allocation.ReduceDeadPools(this, disturbance.Type); 

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

        private void RemoveMarkedCohorts()  // Cohorts that die due to succession processes
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

            Allocation.Allocate(this, cohort, disturbanceType, 1.0);  //Allocation fraction is 1.0 for complete removals

        }

        public bool IsMaturePresent(ISpecies species)
        {
            ISpeciesPNET pnetSpecies = PlugIn.SpeciesPnET[species];

            bool speciesPresent = cohorts.ContainsKey(species);

            bool IsMaturePresent = (speciesPresent && (cohorts[species].Max(o => o.Age) >= species.Maturity)) ? true : false;

            return IsMaturePresent;
        }

        public void AddNewCohort(Cohort cohort)
        {
            if (cohorts.ContainsKey(cohort.Species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies, List<Cohort>>>(cohorts.Where(o => o.Key == cohort.Species))[0];

                List<Cohort> Cohorts = new List<Cohort>(i.Value.Where(o => o.Age < Timestep));

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
                       OutputHeaders.PAR0  + "," + 
                       OutputHeaders.Tday + "," + 
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
                        OutputHeaders.SubCanopyPAR;

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
                        monthdata.Tday + "," +
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
                        subcanopypar;
           
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

