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
        static List<List<ISpecies>> LumpedSpecies = new List<List<ISpecies>>();
        static Dictionary<ISpecies, ISpecies> AssociateSpeciesWith = new Dictionary<ISpecies, ISpecies>();
        static int Timestep;
        public static void Initialize(int timestep)
        {
            Timestep = timestep;

            Dictionary<string, List<ISpecies>> mydict = new Dictionary<string, List<ISpecies>>();

            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                string pars = spc.HalfSat().ToString().PadRight(5) + spc.EstRad().ToString().PadRight(5) + spc.EstRad().ToString().PadRight(5)
                              + spc.H2().ToString().PadRight(5) + spc.H3().ToString().PadRight(5) + spc.H4().ToString().PadRight(5);

                if (mydict.ContainsKey(pars) == false)
                {
                    mydict.Add(pars, new List<ISpecies>());
                }
                mydict[pars].Add(spc);
                AssociateSpeciesWith.Add(spc, mydict[pars][0]);
            }
            LumpedSpecies = mydict.Values.ToList();
        }
        static public Dictionary<ISpecies, float[]> InitialPest
        {
            get
            {
                Dictionary<ISpecies, float[]> Pest = new Dictionary<ISpecies, float[]>();

                foreach (List<ISpecies> spc in LumpedSpecies)
                {
                    Pest.Add(spc[0], new float[]{1,1,1});
                }
                return Pest;
            }
        }

        public static Dictionary<ISpecies, float[]> Calculate_Establishment(float PAR, float PressureHead, Dictionary<ISpecies, float[]> Pest)
        {
            foreach (List<ISpecies> spc in LumpedSpecies)
            {
                if (SiteCohorts.monthdata.Leaf_On[spc[0]])
                {
                    float frad = (float)Math.Pow(Cohort.CumputeFrad(PAR, spc[0].HalfSat()), spc[0].EstRad());
                    float fwater = (float)Math.Pow(Cohort.CumputeFWater(spc[0].H2(), spc[0].H3(), spc[0].H4(), PressureHead), spc[0].EstMoist());

                    float pest = 1 - (float)Math.Pow(1.0 - (frad * fwater), Timestep);

                    if(pest < Pest[spc[0]][0])
                    {
                        Pest[spc[0]] = new float[] { pest, fwater, frad };
                    }
                }
            }
            return Pest;
        }
        public static bool ComputeEstablishment(DateTime date, Dictionary<ISpecies, float[]> Pest, ISpecies Species, LocalOutput establishment_siteoutput)
        {
            float pest = Pest[AssociateSpeciesWith[Species]][0];
            

            bool est = pest > (float)PlugIn.ContinuousUniformRandom();

            if (establishment_siteoutput != null)
            {
                float fwater = Pest[AssociateSpeciesWith[Species]][1];
                float frad = Pest[AssociateSpeciesWith[Species]][2]; 

                establishment_siteoutput.Add(date.Year.ToString() + "," + Species.Name + "," + pest + "," + fwater + "," + frad + "," + est );

                // TODO: win time by reducing calls to write
                establishment_siteoutput.Write();
            }
            if (Species.PreventEstablishment()) return false;
            return est;
        }


        
    }
}
