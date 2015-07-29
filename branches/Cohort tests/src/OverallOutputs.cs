using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Succession;
using Landis.Library.InitialCommunities;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System.IO;
using System;
namespace Landis.Extension.Succession.BiomassPnET
{
    static class OverallOutputs
    {
        public static int DeadCohorts = 0;
        public static int NewCohorts = 0;
        private static FileProps f;
        public static float AverageSiteVar(ISiteVar<float> sitevar)
        {
            float total = 0;
            float n = 0;
            foreach (ActiveSite s in PlugIn.ModelCore.Landscape)
            {
                float v = sitevar[s];
                total += v;
                n++;
            }
            return total / n;
        }

        public static float AverageB()
        {
            float totalB = 0;
            int n = 0;
            foreach (ActiveSite s in PlugIn.ModelCore.Landscape)
            {
                foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[s])
                {
                    foreach (ICohort cohort in speciesCohorts)
                    {
                        totalB += cohort.Biomass;
                       
                    }
                }
                n++;
            }
            return totalB / (float)n;


        }
        public static int NumberOfCohorts(string speciesname)
        {
            int totalCohorts = 0;
            foreach (ActiveSite s in PlugIn.ModelCore.Landscape)
            {
                bool hasspecies = false;
                foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[s])
                {
                    foreach (ICohort cohort in speciesCohorts)
                    {
                        if (cohort.Species.Name == speciesname)
                        {
                            hasspecies = true;
                            continue;
                        }
                    }
                }
                if (hasspecies) totalCohorts++;
            }
            return totalCohorts;
        }
        public static int NumberOfCohorts()
        {
            int totalCohorts = 0;
            foreach (ActiveSite s in PlugIn.ModelCore.Landscape)
            {
                foreach (ISpeciesCohorts speciesCohorts in SiteVars.Cohorts[s])
                {
                    foreach (ICohort cohort in speciesCohorts)
                    {
                        totalCohorts++;
                    }
                }
            }
            return totalCohorts;
        }
        
        static List<string> FileContent = null;
        static void InitFileContent()
        {
            f = new FileProps(FileProps.FileDelimiters.comma);

            FileContent = new List<string>();
            FileContent.Add("Time" + f.Delim + "#Cohorts" + f.Delim + "DeadCohorts" + f.Delim + "NewCohorts" + f.Delim + "AverageB" + f.Delim + "AverageLAI" + f.Delim + "AverageWater" + f.Delim + "SubCanopyPAR");
        }
        public static void WriteNrOfCohortsBalance()
        {
            for (; ; )
            {
                try
                {
                    // Hack: get out the number of cohorts
                    if (FileContent == null) InitFileContent();

                    int noc = NumberOfCohorts();
                    float B = AverageB();
                    float W = AverageSiteVar(SiteVars.Water);
                    float lai = AverageSiteVar(SiteVars.CanopyLAImax);
                    float scp = AverageSiteVar(SiteVars.SubCanopyPAR);
                    FileContent.Add(PlugIn.modelCore.CurrentTime.ToString() + f.Delim + noc + f.Delim + DeadCohorts + f.Delim + NewCohorts + f.Delim + B + f.Delim + lai + f.Delim + W + f.Delim + scp);

                    System.IO.File.WriteAllLines("output/TotalCohorts"+ f.Ext, FileContent.ToArray());
                     

                    DeadCohorts = 0;
                    NewCohorts = 0;
                    break;
                }
                catch (System.Exception e)
                {
                    System.Console.WriteLine("Cannot write to output/TotalCohorts.txt" + e.Message);
                }
            }

        }
    }
}
