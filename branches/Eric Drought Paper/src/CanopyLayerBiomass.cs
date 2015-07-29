using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    class CanopyLayerBiomass
    {
        public List<CohortBiomass> Cohorts = new List<CohortBiomass>();

        private float lai;
        private float netpsn;
        private float grosspsn;
        private float folresp;
        private float belowcanopyradiation;

        public float LAI
        {
            get
            {
                return lai;
            }
        }
        public float Netpsn
        {
            get
            {
                return netpsn;
            }
        }
        public float Grosspsn
        {
            get
            {
                return grosspsn;
            }
        }
        public float FolResp
        {
            get
            {
                return folresp;
            }
        }
         //public float Transpiration
        //{
        //    get
        //    {
        //        return transpiration;
        //    }
        //}
        public float BelowCanopyRadiation
        {
            get
            {
                return belowcanopyradiation;
            }
        }
        public static void Shuffle(ref  List<CohortBiomass> Cohorts)
        {
            Random rng = new Random();
            int n = Cohorts.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                CohortBiomass temp = Cohorts[n];
                Cohorts[n] = Cohorts[k];
                Cohorts[k] = temp;
            }

        }

        public void SimulateCanopyLayers(DateTime date, int canopylayer, float radiation)
        {
            netpsn = 0;
            grosspsn  = 0;
            lai = 0;
             
            if (Cohorts.Count() == 0) belowcanopyradiation = radiation;
            else belowcanopyradiation = 0;

            Shuffle(ref Cohorts);

            float SumLAI = 0;
            foreach (CohortBiomass cohortbiomass in Cohorts)
            {
                CohortBiomass.GrowTree(date, canopylayer, cohortbiomass, radiation);

                
                SumLAI += cohortbiomass.LAI;

                belowcanopyradiation += cohortbiomass.BelowCanopyRadiation / Cohorts.Count();

                netpsn += cohortbiomass.NetPsn;
                grosspsn += cohortbiomass.GrossPsn;
                folresp += cohortbiomass.FolResp;


                Hydrology.SubstractTranspiration(date,cohortbiomass.Site, cohortbiomass.Transpiration);
             }
            

            if (Cohorts.Count() > 0) lai = SumLAI / (float)Cohorts.Count();
            else lai = 0;

            
        }
        public void Add(CohortBiomass c)
        {
            Cohorts.Add(c);
        }
        public int Count
        {
            get
            {
                return Cohorts.Count();
            }
        }
        public void Clear()
        {
            Cohorts.Clear();
        }
    }
}
