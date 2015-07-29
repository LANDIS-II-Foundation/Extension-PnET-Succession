 using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using Landis.Library.Biomass;
namespace Landis.Extension.Succession.BiomassPnET
{
    class Canopy
    {
        // Implements a layered canopy.
        private float subcanopyparmax;
        private float subcanopypar;
        private static float IMAX = 5; // sub canopy layers
        private int NumberOfLayersMax = int.MinValue;
        private float autotrophicrespiration=0;
        private float netpsn =0;
        private float grosspsn =0;
        private float totalbiomass =0;
        private float totalfoliage = 0;
        private float totalnsc = 0;
        private float totalroot = 0;
        private float transpiration = 0;
        private float canopylaimax = 0;
        private int nrofcohorts;
        int maxage = 0;
        int maxbiom = 0;
        static int[] CanopyNumbers;
        public int MaxAge
        {
            get
            {
                return maxage;
            }
        }
        public int MaxBiom
        {
            get
            {
                return maxbiom;
            }
        }

        
        private static Landis.Library.Biomass.Species.AuxParm<float> KWdLit;
        

        List<CohortBiomass>[] canopy;

        public float CanopyLAImax { get { return canopylaimax;  }  }
        public float SubCanopyPARmax { get { return subcanopyparmax; } }
        public float SubCanopyPAR { get { return subcanopypar; } }
        public float GrossPsn { get { return grosspsn; } }
        public float NetPsn { get { return netpsn; } }
        public float AutotrophicRespiration { get { return autotrophicrespiration; } }
        public float TotalRoot { get { return totalroot; } }
        public float TotalBiomass { get { return totalbiomass; } }
        public float TotalFoliage { get { return totalfoliage; } }
        public float TotalNSC { get { return totalnsc; } }
        
        public int NrOfCohorts { get { return nrofcohorts; } }


        List<Cohort> deadcohorts = new List<Cohort>();

        public List<Cohort> DeadCohorts { get { return deadcohorts; } }

        public static void Initialize(IInputParameters parameters)
        {
            KWdLit = parameters.KWdLit;

            if (parameters.CanopyLayerAges.Count() == 0)
            {
                CanopyNumbers = new int[1] { 0 };
                return;
            }
            else
            {
                CanopyNumbers = new int[parameters.CanopyLayerAges[parameters.CanopyLayerAges.Count() - 1] + 1];

                for (int age = 0; age < parameters.CanopyLayerAges[parameters.CanopyLayerAges.Count - 1] + 1; age++)
                {
                    CanopyNumbers[age] = 1;
                    for (int l = 0; l < parameters.CanopyLayerAges.Count; l++)
                    {
                        if (age >= parameters.CanopyLayerAges[l])
                        {
                            CanopyNumbers[age] = parameters.CanopyLayerNumbers[l];
                        }
                    }
                }
            }
        }
        public static int GetNumberOfLayers(int MaxAge)
        {
           
            if(MaxAge < CanopyNumbers.Length) return CanopyNumbers[MaxAge];
            return CanopyNumbers[CanopyNumbers.Length - 1];
            
        }
        private int GetMaxAge(SiteCohorts Cohorts)
        {
            int maxage = int.MinValue;
            foreach (ISpeciesCohorts spc in Cohorts)
            {
                foreach (ICohort cohort in spc)
                {
                    if (cohort.Age > maxage) maxage = cohort.Age;
                }
            }
            return maxage;
        }
        public void Grow(DateTime date, ActiveSite site, Hydrology hydrology, ForestFloor forestfloor, SiteCohorts Cohorts, SiteOutput siteoutput)
        {
            SetCanopyLayers(Cohorts, MaxAge, MaxBiom);
            CalculateRadiationProfile(site, date);
            CalculatePhotosynthesis(date, site, hydrology, forestfloor, siteoutput);
        }
        public void ResetDeadCohorts()
        {
            deadcohorts = new List<Cohort>();
        }
        public void SetCanopyLayers(SiteCohorts Cohorts, int MaxAge, int MaxBiom)
        {
            int NewNrOfLayersMax = Canopy.GetNumberOfLayers(MaxAge);

            canopy = new List<CohortBiomass>[NewNrOfLayersMax+1];
            for (int canopylayer = 0; canopylayer < NewNrOfLayersMax+1; canopylayer++)
            {
                canopy[canopylayer] = new List<CohortBiomass>();
            }
            if (Cohorts == null) return;

            foreach (ISpeciesCohorts spc in Cohorts)
            {
                foreach (Cohort cohort in spc)
                {
                    // For initialization (all cohorts end up in one layer the first call regardless of biomass). 
                    // This is not pretty but saves computation time.
                    if (MaxBiom < cohort.Biomass) MaxBiom = cohort.Biomass;

                    // The max is not to allow a lower canopy layer than previously.
                    float ratio = (float)(cohort.Biomass) / MaxBiom;

                    int actuallayer = (int)(ratio * NewNrOfLayersMax) - 1;

                    if (MaxBiom > 0)
                    {
                        // only reset CanopyLayer if NumberOfLayersMax changed
                        if (NewNrOfLayersMax != NumberOfLayersMax)
                        {
                            cohort.CanopyLayer = Math.Min(NewNrOfLayersMax, Math.Max(cohort.CanopyLayer, actuallayer));
                        }
                    }

                    if (cohort.CanopyLayer > canopy.Length)
                    {
                        throw new System.Exception("cannot implement canopylayer = " + cohort.CanopyLayer + " number of canopy layers is " + canopy.Length + " MaxAge = " + MaxAge);
                    }

                    canopy[cohort.CanopyLayer].Add(new CohortBiomass(cohort));
                }
            }
            NumberOfLayersMax = NewNrOfLayersMax;
        }
        public void CalculatePhotosynthesis(DateTime date, ActiveSite site, Hydrology hydrology, ForestFloor forestfloor, SiteOutput siteoutput)
        {
            autotrophicrespiration = 0;
            grosspsn = 0;
            netpsn = 0;
            totalnsc = 0;
            totalroot = 0;
            totalfoliage=0;
            totalbiomass = 0;
            maxbiom = 0;
            maxage  = 0;
            for (int canopylayer = canopy.Length - 1; canopylayer >= 0; canopylayer--)  
            {
                for (int ix = 0; ix < IMAX; ix++)
                {
                    if (canopy[canopylayer] == null) continue;

                    for (int coh = canopy[canopylayer].Count - 1; coh >= 0; coh--)
                    {
                        CohortBiomass CB = canopy[canopylayer][coh];

                        if (ix == 0)
                        {
                            forestfloor.WoodyDebris.AddMass(CB.WoodSenescence(), KWdLit[CB.Cohort.Species]);
                            forestfloor.Litter.AddMass(CB.FoliageSenescence(date, PlugIn.modelCore.Ecoregion[site], CB.Cohort.Species), ForestFloor.KNwdLitter[PlugIn.modelCore.Ecoregion[site], CB.Cohort.Species]);
                        }

                        float transpiration_lyr = 0;

                        

                        bool cohortisalive = CB.ComputePhotosynthesis(date, PlugIn.modelCore.Ecoregion[site], ix, (int)IMAX, hydrology, ref transpiration_lyr);

                        if (CB.Cohort.Age > MaxAge) maxage  = CB.Cohort.Age;
                        if (MaxBiom < CB.Cohort.Biomass) maxbiom = CB.Cohort.Biomass;

                        autotrophicrespiration += CB.Autotrophicrespiration;
                        netpsn += CB.NetPsn;
                        grosspsn += CB.GrossPsn;

                        if (cohortisalive == false)
                        {
                            deadcohorts.Add(CB.Cohort);
                            forestfloor.WoodyDebris.AddMass(CB.Cohort.Biomass, KWdLit[CB.Cohort.Species]);
                            forestfloor.Litter.AddMass(CB.Cohort.Fol, ForestFloor.KNwdLitter[PlugIn.modelCore.Ecoregion[site], CB.Cohort.Species]);

                        }
                       
                        transpiration += transpiration_lyr;

                        if (transpiration_lyr > hydrology.Water)
                        {
                            IncrementIMAX(site, date);

                            return;
                        }
                        else hydrology.SubtractTranspiration(date, transpiration_lyr);

                        if (ix == 0)
                        {
                            totalbiomass += CB.Cohort.Biomass;
                            totalfoliage += CB.Cohort.Fol;
                            totalnsc += CB.Cohort.NSC;
                            totalroot += CB.Cohort.Root;

                            CB.MaintenanceRespiration(PlugIn.modelCore.Ecoregion[site], date);

                            
                            if (siteoutput != null)
                            {
                                //System.Console.WriteLine("WriteCohortData\t" + site.ToString() + "\t" + date.ToString());
                                CohortOutput.WriteCohortData(date, site, canopy[canopylayer][coh], hydrology.Water);
                            }
                        }
                    }

                }
            }//IMAX
        }
        public void CalculateRadiationProfile(ActiveSite site, DateTime date)
        {
            float belowcohortparsum = 0;
            float belowcohortparcnt = 0;

            float canopylai = 0;

            IEcoregion ecoregion = PlugIn.modelCore.Ecoregion[site];

            float AboveCanopyRadiation = StaticVariables.PAR0[ecoregion, date];
            for (int cl = canopy.Length-1 ; cl >= 0; cl--)
            {
                float canopylaisum = 0;
                float canopylaicnt = 0;

                foreach (CohortBiomass cohbiom in canopy[cl])
                {
                    float cumulative_lai = 0;
                    cohbiom.SetRadiationStress(cl, AboveCanopyRadiation, (int)IMAX, out cumulative_lai);

                    belowcohortparsum += cohbiom.BelowCanopyPAR;
                    belowcohortparcnt++;

                    canopylaisum += cumulative_lai;
                    canopylaicnt++;

                    nrofcohorts++;
                }
                if (belowcohortparcnt > 0)
                {
                    AboveCanopyRadiation = belowcohortparsum / belowcohortparcnt;

                    canopylai += canopylaisum / canopylaicnt;
                    if (canopylai > CanopyLAImax)
                    {
                        canopylaimax = canopylai;
                    }
                }
            }
            subcanopypar = AboveCanopyRadiation;

            if (subcanopypar > subcanopyparmax) subcanopyparmax = subcanopypar;
        }
        public void IncrementIMAX(ActiveSite site, DateTime date)
        {
            IMAX++;
        }
        
        
    }
}
