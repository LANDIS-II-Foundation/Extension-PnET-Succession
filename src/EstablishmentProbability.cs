using Landis.SpatialModeling;
using Landis.Core;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{
    public static class EstablishmentProbability  
    {
        
        public static string Header
        {
            get
            {
                return "Year" + "," + "Species" + "," + "Pest" + "," + "FWater" +"," + "FRad" +"," + "Est";
            }
        }
        
        static int Timestep;
        public static void Initialize(int timestep)
        {
            Timestep = timestep;

             
        }
        static public Dictionary<ISpeciesPNET, float[]> InitialPest
        {
            get
            {
                Dictionary<ISpeciesPNET, float[]> Pest = new Dictionary<ISpeciesPNET, float[]>();

                foreach (ISpeciesPNET spc in  SpeciesPnET.AllSpecies.Values)
                {
                    Pest.Add(spc, new float[]{1,1,1});
                }
                return Pest;
            }
        }

        public static Dictionary<ISpeciesPNET, float[]> Calculate_Establishment(EcoregionPnETVariables pnetvars, float PAR, float PressureHead, Dictionary<ISpeciesPNET, float[]> Pest)
        {
            foreach (ISpeciesPNET spc in SpeciesPnET.AllSpecies.Values)
            {
                if (pnetvars.LeafOn)
                {
                    float frad = (float)Math.Pow(Cohort.CumputeFrad(PAR, spc.HalfSat), spc.EstRad);
                    float fwater = (float)Math.Pow(Cohort.CumputeFWater(spc.H2, spc.H3, spc.H4, PressureHead), spc.EstMoist);

                    float pest = 1 - (float)Math.Pow(1.0 - (frad * fwater), Timestep);

                    if(pest < Pest[spc][0])
                    {
                        Pest[spc] = new float[] { pest, fwater, frad };
                    }
                }
            }
            return Pest;
        }
        public static bool ComputeEstablishment(DateTime date, Dictionary<ISpeciesPNET, float[]> Pest, ISpeciesPNET Species, LocalOutput establishment_siteoutput)
        {
            float pest = Pest[Species][0];
            

            bool est = pest > (float)PlugIn.ContinuousUniformRandom();

            if (establishment_siteoutput != null)
            {
                float fwater = Pest[Species][1];
                float frad = Pest[Species][2]; 

                establishment_siteoutput.Add(date.Year.ToString() + "," + Species.Name + "," + pest + "," + fwater + "," + frad + "," + est );

                // TODO: win time by reducing calls to write
                establishment_siteoutput.Write();
            }
            if (Species.PreventEstablishment) return false;
            return est;
        }


        
    }
}
