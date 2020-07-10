using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Landis.Core;
using Landis.Utilities;
using Landis.Library.DensityCohorts;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;
using System.Linq;


namespace Landis.Library.DensityCohorts
{
    public class SiteDynamics
    {
        private static uint UINT_MAX = uint.MaxValue;
        private List<float> AreaList = new List<float>();
        private List<int> SpecIndexArray = new List<int>();
        private List<int> AgeIndexArray = new List<int>();

        public static void siteSuccession(Landis.Library.DensityCohorts.SiteCohorts siteCohorts)
        {
            int RDflag;

            float siteRD = SiteVars.SiteRD[siteCohorts.Site];
            double GSO1 = EcoregionData.GSO1[siteCohorts.Ecoregion];
            double GSO2 = EcoregionData.GSO2[siteCohorts.Ecoregion];
            double GSO3 = EcoregionData.GSO3[siteCohorts.Ecoregion];
            double GSO4 = EcoregionData.GSO4[siteCohorts.Ecoregion];

            if (siteRD < GSO1)
            {
                RDflag = 0;
            }
            else if (siteRD >= GSO1 && siteRD < GSO2)
            {
                RDflag = 1;
            }
            else if (siteRD >= GSO2 && siteRD <= GSO3)
            {
                RDflag = 2;
            }
            else if (siteRD > GSO3 && siteRD <= GSO4)
            {
                RDflag = 3;
            }
            else
            {
                Debug.Assert(siteRD > GSO4);
                RDflag = 4;
            }

            if (0 == RDflag || 1 == RDflag || 2 == RDflag)
            {
                //GetSeedNumberOnSite(Row, Col);

                //SeedGermination(siteptr, l, RDflag);

                NaturalMortality(siteCohorts, 1);//kill the youngest of trees


            }
            else if (3 == RDflag)
            {
                NaturalMortality(siteCohorts, 0);//kill all ages of trees
            }
            else
            {
                NaturalMortality(siteCohorts, 0);//kill all ages of trees

                if (SiteVars.SiteRD[siteCohorts.Site] > GSO4)
                {
                    Selfthinning(siteCohorts);
                }
            }
        }

        public static void NaturalMortality(Landis.Library.DensityCohorts.SiteCohorts siteCohorts, int StartAge)
        {
            double DQ_const = 3.1415926 / (4 * 0.0002471 * Math.Pow(EcoregionData.ModelCore.CellLength, 2) * Math.Pow(30.48, 2));
            Random rand = new Random();
            //kill all tree, else kill youngest tree
            if (StartAge == 0)
            {
                double tmpDQ = 0;

                foreach (Landis.Library.DensityCohorts.ISpeciesCohorts speciesCohorts in siteCohorts)
                {
                    ISpeciesDensity speciesDensity = SpeciesParameters.SpeciesDensity.AllSpecies[speciesCohorts.Species.Index];
                    foreach (Landis.Library.DensityCohorts.ICohort cohort in speciesCohorts)
                    {
                        if (speciesDensity.SpType >= 0)
                        {
                            tmpDQ += Math.Pow(cohort.Diameter, 2) * DQ_const * cohort.Treenumber;
                        }
                    }
                }
                //====================================================================================================
                foreach (Landis.Library.DensityCohorts.ISpeciesCohorts speciesCohorts in siteCohorts)
                {
                    ISpeciesDensity speciesDensity = SpeciesParameters.SpeciesDensity.AllSpecies[speciesCohorts.Species.Index];
                    foreach (Landis.Library.DensityCohorts.Cohort cohort in speciesCohorts)
                    {
                        if (speciesDensity.SpType >= 0)
                        {
                            double TmpMortality = Landis.Library.DensityCohorts.Cohorts.SuccessionTimeStep / 10 / (1.0 + Math.Exp(3.25309 - 0.00072647 * tmpDQ + 0.01668809 * cohort.Diameter / 2.54));
                            TmpMortality = (1.0f < TmpMortality ? 1.0f : TmpMortality);

                            double DeadTree = cohort.Treenumber * TmpMortality;

                            Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                            int DeadTreeInt = (int)DeadTree;

                            //if (DeadTree - DeadTreeInt >= 0.0001)
                            if (DeadTree > DeadTreeInt)
                            {
                                if (rand.NextDouble() < 0.1)
                                    DeadTreeInt++;
                            }

                            cohort.ChangeTreenumber(-DeadTreeInt);

                            tmpDQ -= Math.Pow(cohort.Diameter, 2) * DQ_const * DeadTree;
                        }
                        else
                        {
                            cohort.ChangeTreenumber((int)Math.Pow(EcoregionData.ModelCore.CellLength, 2));
                        }

                    }
                }
                //====================================================================================================

            }
            else //kill youngest tree
            {
                double tmpDQ = 0;

                foreach (Landis.Library.DensityCohorts.ISpeciesCohorts speciesCohorts in siteCohorts)
                {
                    ISpeciesDensity speciesDensity = SpeciesParameters.SpeciesDensity.AllSpecies[speciesCohorts.Species.Index];
                    foreach (Landis.Library.DensityCohorts.ICohort cohort in speciesCohorts)
                    {
                        if (speciesDensity.SpType >= 0)
                        {
                            tmpDQ += Math.Pow(cohort.Diameter, 2) * DQ_const * cohort.Treenumber;
                        }
                    }
                }

                foreach (Landis.Library.DensityCohorts.ISpeciesCohorts speciesCohorts in siteCohorts)
                {
                    ISpeciesDensity speciesDensity = SpeciesParameters.SpeciesDensity.AllSpecies[speciesCohorts.Species.Index];
                    foreach (Landis.Library.DensityCohorts.Cohort cohort in speciesCohorts)
                    {
                        if (speciesDensity.SpType >= 0)
                        {
                            if (cohort.Age <= (Landis.Library.DensityCohorts.Cohorts.SuccessionTimeStep * StartAge))
                            {
                                double TmpMortality = Landis.Library.DensityCohorts.Cohorts.SuccessionTimeStep / 10 / (1.0 + Math.Exp(3.25309 - 0.00072647 * tmpDQ + 0.01668809 * cohort.Diameter / 2.54));
                                TmpMortality = (1.0f < TmpMortality ? 1.0f : TmpMortality);

                                double DeadTree = cohort.Treenumber * TmpMortality;

                                Debug.Assert(DeadTree <= UINT_MAX && DeadTree >= 0);

                                int DeadTreeInt = (int)DeadTree;

                                //if (DeadTree - DeadTreeInt >= 0.0001)
                                if (DeadTree > DeadTreeInt)
                                {
                                    if (rand.NextDouble() < 0.1)
                                        DeadTreeInt++;
                                }

                                cohort.ChangeTreenumber(-DeadTreeInt);

                                tmpDQ -= Math.Pow(cohort.Diameter, 2) * DQ_const * DeadTree;
                            }
                        }
                        else
                        {
                            cohort.ChangeTreenumber((int)Math.Pow(EcoregionData.ModelCore.CellLength, 2));
                        }
                    }
                }
            }
        }
        //====NaturalMortality()====================================================================================================

        public static void Selfthinning(Landis.Library.DensityCohorts.SiteCohorts siteCohorts)
        {
            double targetRD = SiteVars.SiteRD[siteCohorts.Site] - EcoregionData.GSO4[siteCohorts.Ecoregion];
            double qmd = siteCohorts.siteQMD;
            float[] shadeArray = new float[] { 0.0f, 0.1f, 0.3f, 0.5f, 0.7f, 0.9f }; 
            //SortedList<double, Landis.Library.DensityCohorts.ICohort> cohortMortality = new SortedList<double, Library.DensityCohorts.ICohort>();
            SortedDictionary<int, double> cohortMortality = new SortedDictionary<int, double>();

            for (int i = 0; i < siteCohorts.AllCohorts.Count; i++)
            {
                double reldia = siteCohorts.AllCohorts[i].Diameter / qmd;
                double mort = (0.84525 - (0.01074 * reldia) + (0.0000002 * Math.Pow(reldia, 3))) * (1 - shadeArray[siteCohorts.AllCohorts[i].Species.ShadeTolerance]);
                cohortMortality.Add(i, mort);
            }

            var sortedMortality = (cohortMortality.OrderByDescending(o => o.Key).ToDictionary(o => o.Key, o => o.Value));

            double countRD = targetRD;
            
            foreach (KeyValuePair<int, double> item in sortedMortality)
            {
                int deadTrees = (int)Math.Round(item.Value * siteCohorts.AllCohorts[item.Key].Treenumber);
                float deadRD = computeMortalityRD(siteCohorts.AllCohorts[item.Key], deadTrees);
                siteCohorts.AllCohorts[item.Key].ChangeTreenumber(-deadTrees);
                countRD -= deadRD;
                if (countRD < 0) break;
            }
            
        }

        public static float computeMortalityRD(Landis.Library.DensityCohorts.ICohort cohort, int deadTrees)
        {
            ISpeciesDensity speciesDensity = SpeciesParameters.SpeciesDensity.AllSpecies[cohort.Species.Index];

            float tmp_term1 = (float)Math.Pow((cohort.Diameter / 25.4), 1.605);
            float tmp_term2 = 10000 / speciesDensity.MaxSDI;
            int tmp_term3 = deadTrees;
            float deadRD = tmp_term1 * tmp_term2 * tmp_term3 / (float)Math.Pow(EcoregionData.ModelCore.CellLength, 2);
            return deadRD;
        }
    }
}
