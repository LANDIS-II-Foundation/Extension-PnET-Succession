using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;

namespace Landis.Extension.Succession.BiomassPnET
{

    static class TrackDeadCohortAges
    {
        private static Landis.Extension.Succession.Biomass.Species.AuxParm<Distribution> deadcohorts;
        private static int maxcat = 0;
        private static int TStep;
        private static FileProps f = new FileProps(FileProps.FileDelimiters.comma);
        private static string Explanation
        {
            get
            {
                return "This file shows the frequency distribution of ages at which tree cohorts died binned according to the time step of the model.";
            }
        }
        private class Distribution
        {
            List<int> frequencies = new List<int>();
            int TStep;

            public List<int> Frequencies
            {
                get
                {
                    return frequencies;
                }
            }
            public int Count
            {
                get
                {
                    return Frequencies.Count;
                }
            }
            public void add(int age)
            {
                int category = age/TStep;
                while (Frequencies.Count < category+1)
                {
                    Frequencies.Add(0);
                    if (Frequencies.Count > maxcat) maxcat = Frequencies.Count;
                }
                Frequencies[category]++;
            }
            public Distribution(int TStep)
            {
                this.TStep=TStep;
            }
        }

        public static void Initialize(int tstep)
        {
            TStep = tstep;
            deadcohorts = new Landis.Extension.Succession.Biomass.Species.AuxParm<Distribution>(PlugIn.ModelCore.Species);
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                deadcohorts[species] = new Distribution(TStep);
            }
        }
        public static void Add(ICohort cohort)
        {
            deadcohorts[cohort.Species].add(cohort.Age);
        }
        public static void PrintDeadCohorts()
        {
            List<string> FileContent = new List<string>();
            string hdr = "Species" + f.Delim;
            for (int cat = 0; cat < maxcat; cat++) hdr += "Age[" + cat * TStep + "-" + (cat + 1) * TStep + "]" + f.Delim;
            FileContent.Add(hdr);
            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                string line = species.Name + f.Delim;
                List<int> freq = deadcohorts[species].Frequencies;

                for (int cat = 0; cat < maxcat; cat++)
                {
                    if (cat >= deadcohorts[species].Count) line += "0" + f.Delim;
                    else line += freq[cat].ToString() + f.Delim;
                }

                FileContent.Add(line);
            }


            FileContent.Add(Explanation);

            System.IO.File.WriteAllLines("output/deadcohorts" + f.Ext, FileContent.ToArray());
            
        }
    }
}
