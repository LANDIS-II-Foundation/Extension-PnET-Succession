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
    public class SiteCohorts : Landis.Library.BiomassCohorts.ISiteCohorts, Landis.Library.AgeOnlyCohorts.ISiteCohorts
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
        public static IDictionary<uint, SiteCohorts> initialSites { get; private set; }

        public Dictionary<ISpecies, List<Cohort>> cohorts = null;

        public IEcoregion Ecoregion { get; private set; }
        public ActiveSite Site { get; private set; }
        private static byte Timestep;
        public LocalOutput establishment_siteoutput;
        public LocalOutput siteoutput;
        public readonly Landis.Library.Biomass.Pool WoodyDebris;
        public readonly Landis.Library.Biomass.Pool Litter;
        public float[] AET = new float[12];
        ushort HeterotrophicRespiration;
        ushort Water;
        public Landis.Library.Parameters.Species.AuxParm<float> Pest { get; private set; }
        ushort SnowPack;

        public ushort WaterMAX;
        public ushort SubCanopyParMAX;
        public byte CanopyLAImax { get; private set; }

        public static uint ComputeKey(uint initCommunityMapCode, ushort ecoregionMapCode)
        {
            uint value = (uint)((initCommunityMapCode << 16) | ecoregionMapCode);
            return value;
        }
        public static void Initialize()
        {
            initialSites = new Dictionary<uint, SiteCohorts>();
            Timestep = ((Parameter<byte>)PlugIn.GetParameter(Names.Timestep)).Value;
             
        }

        // Copy existing SiteCohorts
        public SiteCohorts(SiteCohorts s, ActiveSite site)
        {
            this.Ecoregion = PlugIn.ModelCore.Ecoregion[site];
            this.Site = site;

            //this.canopy = new Canopy(site, this);
            this.WoodyDebris = s.WoodyDebris.Clone();
            this.Pest = s.Pest;
            this.Litter = s.Litter.Clone();
            this.Water = s.Water;
            this.SnowPack = s.SnowPack;
            this.CanopyLAImax = s.CanopyLAImax;

            this.SubCanopyParMAX = s.SubCanopyParMAX;
            this.WaterMAX = s.WaterMAX;

            foreach (List<Cohort> species_cohort in s.cohorts.Values)
            {
                foreach (Cohort cohort in species_cohort)
                {
                    AddNewCohort(new Cohort(cohort, PlugIn.IMAX), Timestep);
                }
            }

        }
        
        // Create SiteCohorts in SpinUp
        public SiteCohorts(DateTime StartDate, ActiveSite site, ICommunity initialCommunity, string SiteOutputName = null)
        {
            this.Ecoregion = PlugIn.ModelCore.Ecoregion[site];
            this.Site = site;

            WaterMAX = 0;
            SubCanopyParMAX = 0;
            Pest = new Library.Parameters.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            

            this.Water = (ushort)Hydrology.FieldCap[Ecoregion];
            this.SnowPack = 0;

            this.WoodyDebris = new Library.Biomass.Pool();
            this.Litter = new Library.Biomass.Pool();

            if (SiteOutputName != null)
            {
                this.siteoutput = new LocalOutput(SiteOutputName, "Site.csv", Header(site), PlugIn.ModelCore.UI.WriteLine);


                establishment_siteoutput = new LocalOutput(SiteOutputName, "Establishment.csv", EstablishmentProbability.Header, PlugIn.ModelCore.UI.WriteLine);

            }
            List<Landis.Library.AgeOnlyCohorts.ICohort> cohorts = new List<Landis.Library.AgeOnlyCohorts.ICohort>();
            foreach (Landis.Library.AgeOnlyCohorts.ISpeciesCohorts speciesCohorts in initialCommunity.Cohorts)
            {
                foreach (Landis.Library.AgeOnlyCohorts.ICohort cohort in speciesCohorts)
                    cohorts.Add(cohort);
            }
            List<Landis.Library.AgeOnlyCohorts.ICohort> sortedAgeCohorts = new List<Library.AgeOnlyCohorts.ICohort>(cohorts.OrderByDescending(o => o.Age));
             

            uint key = ComputeKey(initialCommunity.MapCode, Ecoregion.MapCode);
            initialSites[key] = this;

            if (sortedAgeCohorts.Count == 0) return;

            DateTime date = StartDate.AddYears(-(sortedAgeCohorts[0].Age));

            while (date.CompareTo(StartDate) < 0)
            {
                //  Add those cohorts that were born at the current year
                while (sortedAgeCohorts.Count() > 0 && StartDate.Year - date.Year == sortedAgeCohorts[0].Age)
                {

                    Cohort cohort = new Cohort(sortedAgeCohorts[0].Species, (ushort)date.Year, PlugIn.IMAX, sortedAgeCohorts[0].Species.InitialNSC(), sortedAgeCohorts[0].Species.DNSC());

                    AddNewCohort(cohort, Timestep);

                    if (siteoutput != null)
                    {
                        cohort.InitializeOutput(siteoutput.SiteName, (ushort)date.Year, PlugIn.ModelCore.UI.WriteLine);
                    }

                    sortedAgeCohorts.Remove(sortedAgeCohorts[0]);
                }
                //DateTime EndDate = date.AddYears(1);
                DateTime EndDate = (sortedAgeCohorts.Count == 0) ? StartDate : new DateTime((int)(StartDate.Year - sortedAgeCohorts[0].Age), 1, 15);

                List<EcoregionDateData> data = EcoregionDateData.Get(Ecoregion, date, EndDate);
                Grow(data);

                date = EndDate;

            }
            if (sortedAgeCohorts.Count > 0) throw new System.Exception("Not all cohorts in the initial communities file were initialized.");
        }

        
        public int ReduceOrKillBiomassCohorts(Landis.Library.BiomassCohorts.IDisturbance disturbance)
        {
            List<int> reduction = new List<int>();

            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                Landis.Library.BiomassCohorts.SpeciesCohorts species_cohorts = GetSpeciesCohort(cohorts[species_cohort[0].species]);
                
                List<Cohort> ToRemove = new List<Cohort>();
                for (int c =0;c< species_cohort.Count(); c++)
                {
                    reduction.Add(disturbance.ReduceOrKillMarkedCohort(species_cohort[c]));
                    if (reduction[reduction.Count()-1] >= species_cohort[c].Biomass) ToRemove.Add(species_cohort[c]);
                }
                foreach (Cohort cohort in ToRemove)
                {
                    RemoveCohort(this, cohort, disturbance.CurrentSite, disturbance.Type);
                }
            }
            return reduction.Sum();
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

                        ToRemove.AddRange(cohorts[spc].Where(o => o.Age == speciescohort[c].Age));
                    }
                }

            }
            foreach (Cohort cohort in ToRemove)
            {
                RemoveCohort(this, cohort, disturbance.CurrentSite, disturbance.Type);
            }
        }
        public void RemoveCohort(object sender, Cohort cohort, ActiveSite site, ExtensionType disturbanceType)
        {
            Cohort.Died(this, cohort, site, disturbanceType);

            cohorts[cohort.species].Remove(cohort);

            if (cohorts[cohort.species].Count == 0)
            {
                cohorts.Remove(cohort.species);
            }
        }
        public bool IsMaturePresent(ISpecies species)
        {
            bool myreturn = (cohorts.ContainsKey(species) && (cohorts[species].Min(o => o.Age) > species.Maturity)) ? true : false;

            return myreturn;
        }
        public bool AddNewCohort(Cohort cohort, int SuccessionTimeStep)
        {
            if(cohorts ==null)
            {
                cohorts = new Dictionary<ISpecies, List<Cohort>>();
            }
            if (cohorts.ContainsKey(cohort.species))
            {
                // This should deliver only one KeyValuePair
                KeyValuePair<ISpecies, List<Cohort>> i = new List<KeyValuePair<ISpecies,List<Cohort>>>(cohorts.Where(o => o.Key == cohort.species))[0];

                List<Cohort> _cohorts = new List<Cohort>(i.Value.Where(o => o.Age <= SuccessionTimeStep));

                if (_cohorts.Count() > 0)
                {
                    cohort.Wood += cohort.Wood;
                    cohort.Fol += cohort.Fol;
                    cohort.Root += cohort.Root;
                    return false;
                }
                
                cohorts[cohort.species].Add(cohort);
                return true;
            }
            cohorts.Add(cohort.species, new List<Cohort>(new Cohort[]{cohort}));

            
            return true;
        }

        
        Landis.Library.BiomassCohorts.SpeciesCohorts GetSpeciesCohort(List<Cohort> cohorts)
        {
            Landis.Library.BiomassCohorts.SpeciesCohorts spc = new Library.BiomassCohorts.SpeciesCohorts(cohorts[0].species, cohorts[0].Age, cohorts[0].Biomass);

            for (int c = 1; c < cohorts.Count; c++)
            {
                spc.AddNewCohort(cohorts[c].Age, cohorts[c].Biomass);
            }


            return spc;
        }

        public void AddWoodyDebris(float Litter, float KWdLit)
        {
            WoodyDebris.AddMass(Litter, KWdLit);
        }
        public void AddLitter(float AddLitter, ISpecies Species)
        {
            double KNwdLitter = (-0.5365 + (0.00241 * AET.Sum())) - (((-0.01586 + (0.000056 * AET.Sum())) * Species.FolLignin() * 100));
            Litter.AddMass(AddLitter, KNwdLitter);
        }

        
       
        private void RemoveDeadCohorts(ActiveSite site, LocalOutput siteoutput) 
        {
            try
            {
                for (int c = cohorts.Values.Count - 1; c >= 0; c--)
                {
                    List<Cohort> species_cohort = cohorts.Values.ElementAt(c);

                    for (int cc = species_cohort.Count - 1; cc >= 0; cc--)
                    {
                        if (species_cohort[cc].IsAlive == false)
                        {
                            RemoveCohort(this, species_cohort[cc], this.Site, new ExtensionType(Names.ExtensionName));

                        }
                    }
                }
            }
            catch(System.Exception e)
            {
                throw e;
            }
            
        }
        public void Grow(List<EcoregionDateData> data)
        {
            // Reset all summary values
            CanopyLAImax = 0;
            WaterMAX = 0;
            SubCanopyParMAX = 0;

            foreach (ISpecies s in PlugIn.ModelCore.Species)
            {
                Pest[s] = 1;
            }

            Canopy canopy = new Canopy(Site, this);

            Hydrology hydrology = new Hydrology(Site);

            foreach (EcoregionDateData monthdata in data)
            {
                if (monthdata.Month == (int)Constants.Months.June) Defoliate(Site);

                float canopyLAI = cohorts.Values.Sum(o => o.Sum(x => x.LAI));

                AET[monthdata.Month-1] = hydrology.SubtractInterception(monthdata, canopyLAI, ref Water);  

                hydrology.UpdateSiteHydrology(monthdata, ref Water, ref SnowPack);

                if (monthdata.AnyLeafChange)
                {
                    if (cohorts != null) foreach (List<Cohort> species_cohort in cohorts.Values)
                    {
                        foreach (Cohort cohort in species_cohort)
                        {
                            if (monthdata.Leaf_Change[cohort.Species] == true)
                            {
                                float Litter = cohort.FoliageSenescence(monthdata);

                                if (Litter > 0)
                                {
                                    AddLitter(Litter, cohort.species);
                                }
                            }
                        }
                    }
                }

                float SubCanopyPar = monthdata.PAR0;
                if (cohorts != null && cohorts.Count > 0)
                {
                     
                    canopy.CalculatePhotosynthesis(monthdata, hydrology, ref Water, ref SubCanopyPar);
                }

                float Transpiration = cohorts.Values.Sum(o => o.Sum(x => x.Transpiration)); 
                AET[monthdata.Month - 1] += Transpiration;

                AET[monthdata.Month - 1] += hydrology.SubtractEvaporation((ushort)SubCanopyPar, Transpiration, monthdata.Tday, ref Water);

                if (monthdata.AnyLeaf_On)
                {
                    foreach (ISpecies s in PlugIn.ModelCore.Species)
                    {
                       

                        float _pest = (float)EstablishmentProbability.Calculate_Establishment(s, SubCanopyPar, Hydrology.PressureHead(this.Ecoregion, Water));

                        Pest[s] = Math.Min(_pest, Pest[s]);
                    }


                    CanopyLAImax = (byte)Math.Round(Math.Max(canopyLAI, CanopyLAImax), 0);
                    SubCanopyParMAX = (ushort)Math.Max(SubCanopyParMAX, SubCanopyPar);
                    WaterMAX = (ushort)Math.Max(WaterMAX, Water);

                }
                
                if (siteoutput != null)
                {
                    AddSiteOutput(monthdata, hydrology, canopy);

                    foreach (List<Cohort> species_cohort in cohorts.Values)
                    {
                        foreach (Cohort cohort in species_cohort)
                        {
                            cohort.UpdateCohortData(monthdata.Year, Site, monthdata.FTempPSN[cohort.species], monthdata.FTempResp[cohort.species], monthdata.Leaf_On[cohort.species]);
                        }
                    }
                }
                 
                // Stuff that is done once in a year
                if (monthdata.Month == (int)Constants.Months.December)
                {
                    foreach (List<Cohort> species_cohort in cohorts.Values)
                    {
                        foreach (Cohort cohort in species_cohort)
                        {
                            cohort.MaxBiomass = Math.Max(cohort.MaxBiomass, cohort.Biomass);
                            cohort.Age++;

                            //cohort.FActiveBiom = (float)Math.Exp(-cohort.species.FrActWd() * cohort.MaxBiomass);

                            AddWoodyDebris(cohort.Senescence(), cohort.species.KWdLit());
                        }
                    }
                    HeterotrophicRespiration = (ushort)(Litter.Decompose() + WoodyDebris.Decompose());
                     
                }
            }
            RemoveDeadCohorts(Site, siteoutput);
            

            if (siteoutput != null)
            {
                siteoutput.Write();

                foreach (List<Cohort> species_cohort in cohorts.Values)
                {
                    foreach (Cohort cohort in species_cohort)
                    {
                        cohort.WriteCohortData();

                        cohort.ClearSubLayers();
                    }
                }
            }
            
        }
        
        public void Defoliate(ActiveSite site)
        {
            int sumbiom = cohorts.Values.Sum(o => o.Sum(x => x.Biomass)); 

            foreach (List<Cohort> species_cohort in cohorts.Values)
            {
                foreach (Cohort cohort in species_cohort)
                {
                    double defoliation = Math.Min(1, Landis.Library.Biomass.CohortDefoliation.Compute(site, cohort.Species, cohort.Biomass, sumbiom));

                    if (defoliation > 0)
                    {
                        cohort.Fol = (ushort)(cohort.Fol * (1F - (float)defoliation));
                    }
                }
            }
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
                       OutputHeaders.DeliveryPotential + "," +
                       OutputHeaders.Interception + "," +
                       OutputHeaders.water + "," +
                       OutputHeaders.MaxWater + "," + 
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
         
        private void AddSiteOutput(EcoregionDateData monthdata, Hydrology hydrology, Canopy canopy)
        {

            string s = monthdata.Year + "," + 
                        cohorts.Values.Sum(o=> o.Count) + "," +
                        Canopy.MaxLayerStDev + "," + 
                        canopy.Nlayers + "," + 
                        monthdata.PAR0 + "," + 
                        monthdata.Tday + "," +
                        monthdata.Prec + "," + 
                        Hydrology.RunOff + "," + 
                        Hydrology.Leakage + "," + 
                        Hydrology.PET + "," +
                        Hydrology.Evaporation + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Transpiration)) +"," +
                        Hydrology.DeliveryPotential + "," +
                        Hydrology.Interception + "," + 
                        Water + "," +
                        WaterMAX + "," + 
                        Hydrology.PressureHead(PlugIn.ModelCore.Ecoregion[Site], Water) + "," +
                        SnowPack + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.LAI)) +"," +
                        monthdata.VPD + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Grosspsn)) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Netpsn)) + "," + 
                        cohorts.Values.Sum(o => o.Sum(x => x.MaintenanceRespiration))  + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Wood)) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Root)) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.Fol)) + "," +
                        cohorts.Values.Sum(o => o.Sum(x => x.NSC)) + "," +
                        HeterotrophicRespiration + "," + 
                        Litter.Mass + "," + 
                        WoodyDebris.Mass;

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

