//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using System;
using System.IO;
using Landis.Extension.Succession.Biomass;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Site Variables for a landscape.
    /// </summary>
    public static class SiteVars
    {
        private static ISiteVar<int> numberofcanopylayers;
        private static ISiteVar<Landis.Extension.Succession.Biomass.Species.AuxParm<bool>> establishments;
        private static ISiteVar<Landis.Extension.Succession.Biomass.Species.AuxParm<float>> pest;
        private static ISiteVar<ISiteCohorts> cohorts;
        private static BiomassCohortsSiteVar BiomassCohortsSiteVar;
        private static Landis.Extension.Succession.Biomass.BaseCohortsSiteVar baseCohortsSiteVar;
        private static ISiteVar<Pool> woodyDebris;
        private static ISiteVar<Pool> litter;
        public static ISiteVar<bool> HasSiteOutput;
        public static ISiteVar<float> SnowPack;
        public static ISiteVar<float> Water; //mm
        public static ISiteVar<float> WFPS; //mm
        
        public static ISiteVar<float> SubCanopyPAR;
        public static ISiteVar<float> SubCanopyPARmax;
        public static ISiteVar<float> AnnualTranspiration; //mm
        private static ISiteVar<float> transpiration; //mm
        public static ISiteVar<float> Infiltration;
        public static ISiteVar<int> TotalBiomass;
        public static ISiteVar<float> CanopyLAI;
        public static ISiteVar<float> CanopyLAImax;
        public static ISiteVar<float> GrossPsn;
        public static ISiteVar<float> NetPsn;
        public static ISiteVar<float> AutotrophicRespiration;
        public static ISiteVar<float> HeterotrophicRespiration;
        //---------------------------------------------------------------------

        /// <summary>
        /// Initializes the module.
        /// </summary>
        public static void Initialize()
        {
            numberofcanopylayers = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            cohorts = PlugIn.ModelCore.Landscape.NewSiteVar<ISiteCohorts>();
            BiomassCohortsSiteVar = new BiomassCohortsSiteVar(cohorts);
            baseCohortsSiteVar = new Landis.Extension.Succession.Biomass.BaseCohortsSiteVar(BiomassCohortsSiteVar);
            woodyDebris     = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
            litter          = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
            HasSiteOutput = PlugIn.ModelCore.Landscape.NewSiteVar<bool>();
            SnowPack = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            Water = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            Infiltration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            establishments = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Extension.Succession.Biomass.Species.AuxParm<bool>>();
            pest = PlugIn.ModelCore.Landscape.NewSiteVar<Landis.Extension.Succession.Biomass.Species.AuxParm<float>>();
            TotalBiomass = PlugIn.ModelCore.Landscape.NewSiteVar<int>();
            AnnualTranspiration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            CanopyLAI = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            GrossPsn = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            NetPsn = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            transpiration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            AutotrophicRespiration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            HeterotrophicRespiration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            WFPS = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            CanopyLAImax = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            
            SubCanopyPAR = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            SubCanopyPARmax = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                //  site cohorts are initialized by the PlugIn.InitializeSite method
                woodyDebris[site] = new Pool();
                litter[site] = new Pool();
                HasSiteOutput[site] = false;
                SnowPack[site] = 0;
                Water[site] = 0;
                Infiltration[site] = 0;
                TotalBiomass[site] = 0;
                numberofcanopylayers[site] = 0;
                GrossPsn[site] = 0;
                NetPsn[site] = 0;
                AutotrophicRespiration[site] = 0;
                HeterotrophicRespiration[site] = 0;
                transpiration[site] = 0;
                WFPS[site] = 0;
                establishments[site] = new Landis.Extension.Succession.Biomass.Species.AuxParm<bool>(PlugIn.ModelCore.Species);
                pest[site] = new Landis.Extension.Succession.Biomass.Species.AuxParm<float>(PlugIn.ModelCore.Species);


                AnnualTranspiration[site] = 0;
                CanopyLAI[site] = 0;
                SubCanopyPAR[site] = 0;
                SubCanopyPARmax[site] = 0;
                foreach (ISpecies spc in PlugIn.ModelCore.Species)
                {
                    establishments[site][spc] = false;
                    pest[site][spc] = 0;
                }
            }
            PlugIn.ModelCore.RegisterSiteVar(cohorts, "Succession.BiomassCohortsPnET");
            PlugIn.ModelCore.RegisterSiteVar(BiomassCohortsSiteVar, "Succession.BiomassCohorts");
            PlugIn.ModelCore.RegisterSiteVar(baseCohortsSiteVar, "Succession.AgeCohorts");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Establishments, "Succession.Establishments");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.WoodyDebris, "Succession.WoodyDebris");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Litter, "Succession.Litter");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.HasSiteOutput, "Succession.HasSiteOutput");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.Water, "Succession.SoilWater");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.AnnualTranspiration , "Succession.AnnualTranspiration");
            PlugIn.ModelCore.RegisterSiteVar(SiteVars.SubCanopyPAR, "Succession.SubCanopyPARmax");

            PlugIn.ModelCore.RegisterSiteVar(SiteVars.CanopyLAImax, "Succession.CanopyLAImax");
            
        }
        
        //---------------------------------------------------------------------
        public static ISiteVar<float> Transpiration
        {
            get
            {
                return transpiration;
            }
            set
            {
                transpiration = value;
            }
        }
        /// <summary>
        /// Biomass cohorts at each site.
        /// </summary>
        public static ISiteVar<ISiteCohorts> Cohorts
        {
            get
            {
                return cohorts;
            }
            set
            {
                cohorts = value;
            }
        }

        
        /// <summary>
        /// The intact dead woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> WoodyDebris
        {
            get
            {
                return woodyDebris;
            }
        }
        public static ISiteVar<int> NumberOfCanopyLayers
        {
            get
            {
                return numberofcanopylayers;
            }
        }
        public static ISiteVar<Landis.Extension.Succession.Biomass.Species.AuxParm<bool>> Establishments
        {
            get
            {
                return establishments;
            }
        }
        public static ISiteVar<Landis.Extension.Succession.Biomass.Species.AuxParm<float>> Pest
        {
            get
            {
                return pest;
            }
        }
        
        //---------------------------------------------------------------------

        /// <summary>
        /// The dead non-woody pools for the landscape's sites.
        /// </summary>
        public static ISiteVar<Pool> Litter
        {
            get
            {
                return litter;
            }
        }

         
         
        
    }
}
