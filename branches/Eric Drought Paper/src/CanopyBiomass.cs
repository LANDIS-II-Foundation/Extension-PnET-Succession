//  Copyright ...
//  Authors:  Arjan de Bruijn

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;

namespace Landis.Extension.Succession.BiomassPnET
{
    class CanopyBiomass
    {
        static List<CanopyLayerBiomass> canopy = new List<CanopyLayerBiomass>();
        private static ISiteVar<Landis.Library.Biomass.Species.AuxParm<List<int>>> deadcohortages;
        private static ISiteVar<Landis.Library.Biomass.Species.AuxParm<int>> deadcohorts;

        float radiation;
        //static int CanopyLayerAgeSpan;

        public static  ISiteVar<float> AutotrophicRespiration;
        public static  ISiteVar<float> grosspsn;
        public static  ISiteVar<float> netpsn;
        private static ISiteVar<float> canopylaimax;
        private static ISiteVar<float> canopylai;
        private static ISiteVar<float> subcanopyparmax;
        private static ISiteVar<int> numberofcanopylayers;
        private static ISiteVar<float> subcanopypar;
        private static ISiteVar<float> totalbiomass;
        private static ISiteVar<float> totalroot;
        private static ISiteVar<float> totalfoliage;
        private static ISiteVar<float> totalnsc;
        
        public static ISiteVar<float> TotalBiomass
        {
            get
            {
                return totalbiomass;
            }
        }
        public static ISiteVar<float> TotalRoot
        {
            get
            {
                return totalroot;
            }
        }
        public static ISiteVar<float> TotalFoliage
        {
            get
            {
                return totalfoliage;
            }
        }
        public static ISiteVar<float> TotalNSC
        {
            get
            {
                return totalnsc;
            }
        }

        

        public static ISiteVar<float> GrossPsn
        {
            get
            {
                return grosspsn;
            }
        }
        public static ISiteVar<float> NetPsn
        {
            get
            {
                return netpsn;
            }
        }
         
        public static ISiteVar<float> SubCanopyPARmax
        {
            get
            {
                return subcanopyparmax;
            }
        }
        public static ISiteVar<float> SubCanopyPAR
        {
            get
            {
                return subcanopypar;
            }
        }
         
         
        public static ISiteVar<Landis.Library.Biomass.Species.AuxParm<int>> DeadCohorts
        {
            get
            {
                return deadcohorts;
            }
        }
        public static ISiteVar<Landis.Library.Biomass.Species.AuxParm<List<int>>> DeadCohortAges
        {
            get
            {
                return deadcohortages;
            }
        }
        public static ISiteVar<int> NumberOfCanopyLayers
        {
            get
            {
                return numberofcanopylayers;
            }
        }
        public float Radiation
        {
            get
            {
                return radiation;
            }
        }
         
         
        public static ISiteVar<float> CanopyLAImax
        {
            get
            {
                return canopylaimax;
            }
        }
        public static ISiteVar<float> CanopyLAI
        {
            get
            {
                return canopylai;
            }
        }
        

        public static void Initialize(IInputParameters parameters)
        {
            totalbiomass = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            totalroot = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            totalfoliage = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            totalnsc = PlugIn.ModelCore.Landscape.NewSiteVar<float>();

            subcanopypar = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            subcanopyparmax = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            canopylai = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            canopylaimax = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            deadcohortages = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Species.AuxParm<List<int>>>();
            deadcohorts = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Species.AuxParm<int>>();

            numberofcanopylayers = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            AutotrophicRespiration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape) numberofcanopylayers[site] = 0;
            
            grosspsn = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            netpsn  = PlugIn.ModelCore.Landscape.NewSiteVar<float>();

            //CanopyLayerAgeSpan = parameters.CanopyLayerAgeSpan;

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                CanopyLAI[site] = 0;
                SubCanopyPAR[site] = 0;
                SubCanopyPARmax[site] = 0;
                GrossPsn[site] = 0;
                NetPsn[site] = 0;
                AutotrophicRespiration[site] = 0;

                deadcohortages[site] = new Landis.Library.Biomass.Species.AuxParm<List<int>>(PlugIn.ModelCore.Species);
                deadcohorts[site] = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.ModelCore.Species);
                foreach (ISpecies species in PlugIn.ModelCore.Species)
                {
                    deadcohortages[site][species] = new List<int>();
                }
               
            }

            PlugIn.ModelCore.RegisterSiteVar(DeadCohorts, "Succession.DeadCohorts");
            PlugIn.ModelCore.RegisterSiteVar(DeadCohortAges, "Succession.DeadCohortAges");
            PlugIn.ModelCore.RegisterSiteVar(SubCanopyPAR, "Succession.SubCanopyPARmax");
            PlugIn.ModelCore.RegisterSiteVar(CanopyLAImax, "Succession.CanopyLAImax");
        }

        private static void CalculateBiomassSums(ActiveSite site)
        {
            
            totalbiomass[site] = 0;
            totalroot[site] = 0;
            totalnsc[site] = 0;
            totalfoliage[site] = 0;
            foreach (ISpeciesCohorts sppCo in PlugIn.Cohorts[site])
            {
                foreach (ICohort cohort in sppCo)
                {
                    totalroot[site] += cohort.Root;
                    totalnsc[site] += cohort.NSC;
                    totalfoliage[site] += cohort.Fol;
                    totalbiomass[site] += cohort.Biomass;
                }
            }
        }
        
        static public void DefineBiomassLayers(ActiveSite site)
        {
            if (PlugIn.Cohorts[site] == null) return;

            CalculateBiomassSums(site);
             
            canopy.Clear();
           
            int maxage = 0;
            
            List<ISpecies> species = new List<ISpecies>();
            List<int> cohort_index = new List<int>();
            List<float> biomass = new List<float>();
            List<int> old_canopylayers = new List<int>();
            foreach (ISpeciesCohorts speciesCohorts in PlugIn.Cohorts[site])
            {
                int index = 0;
                foreach (ICohort cohort in speciesCohorts)
                {
                    if (cohort.Age > maxage) maxage = cohort.Age;
                    int a = 0;
                    for (a = 0; a < biomass.Count; a++)
                    {
                        if (biomass[a] > cohort.Biomass)
                        {
                            biomass.Insert(a, cohort.Biomass);
                            species.Insert(a, cohort.Species);
                            cohort_index.Insert(a, index);
                            old_canopylayers.Insert(a, cohort.CanopyLayer);
                            break;
                        }
                    }
                    if (a == biomass.Count)
                    {
                        species.Add(cohort.Species);
                        biomass.Add(cohort.Biomass);
                        cohort_index.Add(index);
                        old_canopylayers.Add(cohort.CanopyLayer);
                    }
                    index++;
                
                }
            }
            if (TotalBiomass[site] <= 0) return;
            float[] fractions = CanopyLayerCategories.GetCumBiomFractions(maxage);

            float includedbiomfrac = 0;
            for (int b = 0; b < biomass.Count(); b++)
            {
                includedbiomfrac += biomass[b] / TotalBiomass[site];

                for (int f = 0; f < fractions.Count(); f++)
                {
                    
                    if (includedbiomfrac <= fractions[f])
                    {
                        int CanopyLayer = Math.Max(old_canopylayers[b], f);
                        while (canopy.Count < CanopyLayer + 1)
                        {
                            canopy.Add(new CanopyLayerBiomass());
                        }
                        canopy[CanopyLayer].Add(new CohortBiomass(site, PlugIn.Cohorts[site][species[b]].Get(cohort_index[b]), cohort_index[b]));
                        break;
                    }
                }

            }
        }
        /*
        public static void DefineAgeLayers(ActiveSite site)
        {
            canopy.Clear();

            float SumFoliageBiomass = 0;
            foreach (ISpeciesCohorts speciesCohorts in PlugIn.Cohorts[site])
            {
                int index = 0;
                foreach (ICohort cohort in speciesCohorts)
                {
                    SumFoliageBiomass += cohort.Fol;

                    int CanopyLayer = (int)(cohort.Age / CanopyLayerAgeSpan);

                    while (canopy.Count < CanopyLayer + 1)
                    {
                        canopy.Add(new CanopyLayerBiomass());
                    }

                    canopy[CanopyLayer].Add(new CohortBiomass(site, PlugIn.Cohorts[site][cohort.Species].Get(index), index));
                    index++;
                }
            }
            
            for (int l = 0; l < canopy.Count; l++)
            {
                if (canopy[l].Count == 0)
                {
                    canopy.Remove(canopy[l]);
                    l--;
                }
            }
            
            NumberOfCanopyLayers[site] = canopy.Count;  

        }
        */
         
        public static void RemoveDeadCohorts(ActiveSite site)
        {
            
            foreach (ISpeciesCohorts speciesCohorts in PlugIn.Cohorts[site])
            {
                for (int index = 0; index < speciesCohorts.Count; index++)
                {

                    if (CohortBiomass.CohortIsDead(PlugIn.Cohorts[site][speciesCohorts.Species].Get(index)))
                    {
                        ForestFloor.WoodyDebris[site].AddMass(PlugIn.Cohorts[site][speciesCohorts.Species].Get(index).Biomass, 0.001);

                        DeadCohortAges[site][speciesCohorts.Species].Add(PlugIn.Cohorts[site][speciesCohorts.Species].Get(index).Age);
                        deadcohorts[site][speciesCohorts.Species]++;

                        PlugIn.Cohorts[site][speciesCohorts.Species].RemoveCohort(index, PlugIn.Cohorts[site][speciesCohorts.Species].Get(index), site, null);
                      
                    }
                }
            }

         
        }
        private CanopyBiomass(float PAR0, ActiveSite site)
        {
            radiation = PAR0;
            canopylai[site]  = 0;
        }
        public static void SimulateCanopy(DateTime date, ActiveSite site)
        {
            CanopyBiomass canopybiomass = new CanopyBiomass(Static.PAR0[date], site);

            CanopyLAI[site] = 0;
            for (int c =  canopy.Count()-1; c >= 0; c--)
            {
                canopy[c].SimulateCanopyLayers(date, c, canopybiomass.radiation);

                CanopyLAI[site] += canopy[c].LAI;
                 
                canopybiomass.radiation = canopy[c].BelowCanopyRadiation;

                NetPsn[site] += canopy[c].Netpsn;
                GrossPsn[site] += canopy[c].Grosspsn;
                AutotrophicRespiration[site] += canopy[c].FolResp;

           
            }
            

            SubCanopyPAR[site] = canopybiomass.Radiation;
            if (CanopyLAI[site] > CanopyLAImax[site]) CanopyLAImax[site] = CanopyLAI[site];
            if (SubCanopyPAR[site] > SubCanopyPARmax[site]) SubCanopyPARmax[site] = SubCanopyPAR[site];
               
        }
         
       
         
    
    }


}
