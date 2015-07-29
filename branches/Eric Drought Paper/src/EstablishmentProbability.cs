using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;

namespace Landis.Extension.Succession.BiomassPnET
{
    static class EstablishmentProbability
    {
        
        private static Random R = new Random();

        private static ISiteVar<Landis.Library.Biomass.Species.AuxParm<int>> establishments;
        private static ISiteVar<Landis.Library.Biomass.Species.AuxParm<float>> pest;
        private static Landis.Library.Biomass.Species.AuxParm<float> GDDFolStart;
        private static Landis.Library.Biomass.Species.AuxParm<float> GDDFolEnd;
        private static Landis.Library.Biomass.Species.AuxParm<float> HalfSat;
        private static Landis.Library.Biomass.Species.AuxParm<float> PsnTMin;
        private static Landis.Library.Biomass.Species.AuxParm<int> SenescStart;
        private static Landis.Library.Biomass.Species.AuxParm<float[]> EstRadSensitivity;
        private static Landis.Library.Biomass.Species.AuxParm<float[]> EstMoistureSensitivity;
        
        private static void InitializeSpeciesParameters(IInputParameters parameters)
        {
            EstRadSensitivity = parameters.EstRadSensitivity;
            EstMoistureSensitivity = parameters.EstMoistureSensitivity;
            GDDFolStart = parameters.GDDFolStart;
            GDDFolEnd = parameters.GDDFolEnd;
            PsnTMin = parameters.PsnTMin;
            SenescStart = parameters.SenescStart;
            HalfSat= parameters.HalfSat;
        }
        private static void InitializeSiteVariables()
        {
            establishments = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Species.AuxParm<int>>();
            pest = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Library.Biomass.Species.AuxParm<float>>();

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                pest[site] = new Landis.Library.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);
                establishments[site] = new Landis.Library.Biomass.Species.AuxParm<int>(PlugIn.ModelCore.Species);
                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    establishments[site][spc] = 0;
                    pest[site][spc] = 0;
                }
            }
        }

        public static void Initialize(IInputParameters parameters)
        {
             
            InitializeSpeciesParameters(parameters);
            InitializeSiteVariables();

            
            PlugIn.ModelCore.RegisterSiteVar(establishments, "Succession.Establishments");
        }
        
        public static ISiteVar<Landis.Library.Biomass.Species.AuxParm<int>> Establishments
        {
            get
            {
                return establishments;
            }
        }
        public static ISiteVar<Landis.Library.Biomass.Species.AuxParm<float>> Pest
        {
            get
            {
                return pest;
            }
        }
        public static void Compute(DateTime date, ActiveSite site)
        {
            if (Static.PossibleEstmonth[date] == false) return;

            //System.Console.WriteLine(PlugIn.Date[site].Month);
            float frad;
            float fwater;


            float dw = 100 * Hydrology.Water[site] / Hydrology.WHC[PlugIn.modelCore.Ecoregion[site]];

            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                if (Establishments[site][species] >0) continue;
                frad = EstRadSensitivity[species][(int)(100*Math.Min(1,CanopyBiomass.SubCanopyPAR[site]/ 2 * HalfSat[species]))];
                fwater = EstMoistureSensitivity[species][(int)(100 * Hydrology.Water[site] / Hydrology.WHC[PlugIn.modelCore.Ecoregion[site]])];
                Pest[site][species] = Static.DTempPSN[date][species] * frad * fwater; 
                if (Pest[site][species] > (float)R.NextDouble())
                {
                    Establishments[site][species] = 1;
                }
            }
             

        }
    }
}
