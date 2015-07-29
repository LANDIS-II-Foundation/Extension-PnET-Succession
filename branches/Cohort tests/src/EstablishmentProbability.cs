using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class EstablishmentProbability
    {
        private ActiveSite site;
        private IEcoregion ecoregion;
        private static Random R = new Random();
        
        private Landis.Library.Biomass.Species.AuxParm<float> pest;
        private static Landis.Library.Biomass.Species.AuxParm<float> GDDFolSt;
        private static Landis.Library.Biomass.Species.AuxParm<float> HalfSat;
        private static Landis.Library.Biomass.Species.AuxParm<float> PsnTMin;
        private static Landis.Library.Biomass.Species.AuxParm<int> CDDFolEnd;
        private static Landis.Library.Biomass.Species.AuxParm<float> EstRad;
        private static Landis.Library.Biomass.Species.AuxParm<float> EstMoist;
        private static Landis.Library.Biomass.Ecoregions.AuxParm<int> Porosity;

        private Landis.Library.Biomass.Species.AuxParm<int> establishments;

        public Landis.Library.Biomass.Species.AuxParm<int> Establishments
        {
            get
            {
                return establishments;
            }
        }


        private Landis.Library.Biomass.Species.AuxParm<int> potestablishments;
        public Landis.Library.Biomass.Species.AuxParm<int> PotEstablishments
        {
            get
            {
                return potestablishments;
            }
        }
        
        public static void Initialize(IInputParameters parameters)
        {
            EstRad = parameters.EstRad;
            EstMoist = parameters.EstMoist;
            GDDFolSt = parameters.GDDFolSt;
            PsnTMin = parameters.PsnTMin;
            CDDFolEnd = parameters.CDDFolEnd;
            HalfSat = parameters.HalfSat;
            Porosity = parameters.Porosity;
        }
        
        public EstablishmentProbability(ActiveSite site)
        {
            this.site = site;
            this.ecoregion = PlugIn.modelCore.Ecoregion[site];
           
            pest = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
            potestablishments = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.ModelCore.Species);
            establishments = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.ModelCore.Species);
            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                potestablishments[spc] = 0;
                establishments[spc] = 0;
                pest[spc] = 0;
            }
        }
       
        
        public Landis.Library.Biomass.Species.AuxParm<float> Pest
        {
            get
            {
                return pest;
            }
        }
        public static ISpecies[] ShuffledSpecies()
        {
            ISpecies[] species = PlugIn.ModelCore.Species.ToArray < ISpecies>();
            int n = species.Length;

            while (n > 1)
            {
                int k = R.Next(n--);
                ISpecies temp = species[n];
                species[n] = species[k];
                species[k] = temp;
            }
            return species;
        }

        int PossibleEstmonthCount(DateTime date, ISpecies spc, IEcoregion ecoregion)
        {
            int pem=0;
            for (int m = 1; m <= 12; m++)
            {
                if (StaticVariables.PossibleEstmonth[ecoregion,spc, date]) pem++;
            }
            return pem;
        }

        public void ComputeEstablishment( DateTime date, SiteConditions sc)
        {
            double frad;
            double fwater;
            double w = sc.hydrology.Water / Porosity[PlugIn.modelCore.Ecoregion[site]];

            // number of months that est is possible for stat purposes
            foreach (ISpecies spc in PlugIn.modelCore.Species)
            {
                if (StaticVariables.PossibleEstmonth[PlugIn.modelCore.Ecoregion[sc.Site], spc, date] == false)
                {
                    Pest[spc] = 0;
                    continue;
                }

                double v= Math.Min(1, sc.SubCanopyPAR / (2 * HalfSat[spc]));

                if (v > 1 || v < 0) throw new System.Exception("Error, subcanopy radiation cannot be " + sc.SubCanopyPAR);
                if (w > 1 || w < 0) throw new System.Exception("Error, bulk water cannot be " + sc.hydrology.Water);

                frad = Math.Pow(v, EstRad[spc]);
                fwater = Math.Pow(w, EstMoist[spc]);

                Pest[spc] = 1 - (float)Math.Pow((1 - (float)Math.Min(1.0, frad * fwater)), (1 / (PlugIn.TStep * (float)PossibleEstmonthCount(date, spc, ecoregion))));
                 
                if (Pest[spc] > (float)R.NextDouble())
                {
                    PotEstablishments[spc] = 1;
                }
            }
             

        }
    }
}
