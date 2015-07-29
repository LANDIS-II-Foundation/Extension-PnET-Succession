//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Biomass;
namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Soil organic matter (SOM) pool.
    /// Mass = C Fraction + N Fraction + P Fraction + other(inexplicit).
    /// </summary>
    public class ForestFloor
    {
        public static ISiteVar<float> HeterotrophicRespiration;
        public static Landis.Library.Biomass.Ecoregions.AuxParm<int> AET;
        public static Landis.Library.Biomass.Species.AuxParm<float> LeafLignin;
        private static ISiteVar<Pool> woodyDebris;
        private static ISiteVar<Pool> litter;
        

        public static ISiteVar<Pool> WoodyDebris
        {
            get
            {
                return woodyDebris;
            }
        }

        public static ISiteVar<Pool> Litter
        {
            get
            {
                return litter;
            }
        }

        public static void Decomposition(ActiveSite site)
        {
            HeterotrophicRespiration[site] += (float)ForestFloor.Litter[site].ReduceMass(0.1);
            HeterotrophicRespiration[site] += (float)ForestFloor.WoodyDebris[site].ReduceMass(0.001);
        }
        public static void Initialize(IInputParameters parameters)
        {
             AET = parameters.AET;
             HeterotrophicRespiration = PlugIn.ModelCore.Landscape.NewSiteVar<float>();
             woodyDebris = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
             litter = PlugIn.ModelCore.Landscape.NewSiteVar<Pool>();
             foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
             {
                 //  site cohorts are initialized by the PlugIn.InitializeSite method
                 woodyDebris[site] = new Pool();
                 litter[site] = new Pool();
                 HeterotrophicRespiration[site] = 0;
             }
             PlugIn.ModelCore.RegisterSiteVar(WoodyDebris, "Succession.WoodyDebris");
             PlugIn.ModelCore.RegisterSiteVar(Litter, "Succession.Litter");
        }
        
        //---------------------------------------------------------------------
        /// <summary>
        /// Adds some biomass for a species to the LITTER pools at a site.
        /// </summary>
        public static void AddLitter(double nonWoodyBiomass,
                                      ISpecies   species,
                                      ActiveSite site)
        {

            IEcoregion ecoregion = PlugIn.ModelCore.Ecoregion[site];
            double siteAET = (double)AET[ecoregion]; 
            
            //Calculation of decomposition rate for species litter cohort
            // Decay rate from Meentemeyer 1978.  Ecology 59: 465-472.
            double leafKReg = (-0.5365 + (0.00241 * siteAET)) - (((-0.01586 + (0.000056 * siteAET)) * LeafLignin[species] * 100));
            
            // From  Fan et al. 1998 Ecological Applications 8: 734-737:
            //double leafKReg = ((0.10015 * siteAET - 3.44618) - (0.01341 + 0.00147 * siteAET) *
            //LeafLignin[species]) / 100;
            
            //Console.WriteLine("Decay rate for {0} within {1} = {2}.  LL = {3}.", species.Name, ecoregion.Name, leafKReg, LeafLignin[species]);

            double decayValue = leafKReg;

            Litter[site].AddMass(nonWoodyBiomass, decayValue);

        }

    }
}
