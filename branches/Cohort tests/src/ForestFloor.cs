//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Core;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Biomass;
namespace Landis.Extension.Succession.BiomassPnET
{
    public class ForestFloor
    {
        private float heterotrophicrespiration;
        private static Landis.Library.Biomass.Ecoregions.AuxParm<int> AET;
        private static Landis.Library.Biomass.Species.AuxParm<float> FolLignin;
        private Pool woodyDebris;
        private Pool litter;

        public float HeterotrophicRespiration { get { return heterotrophicrespiration; } }
        public Pool WoodyDebris {    get {   return woodyDebris;  } }
        public Pool Litter {    get   {   return litter;   }    }

        public static VarEcoregionSpecies<double> KNwdLitter = null;

        public ForestFloor()
        {
            woodyDebris = new Pool();
            litter = new Pool();
        }   
        public void Decompose()
        {
            heterotrophicrespiration = (float)litter.Decompose();
            heterotrophicrespiration += (float)woodyDebris.Decompose();
        }
        public static void Initialize(IInputParameters parameters)
        {
             AET = parameters.AET;
             FolLignin = parameters.FolLignin;

             KNwdLitter = GetKNwdLitter();
        }
        
        //---------------------------------------------------------------------
        /// <summary>
        /// Adds some biomass for a species to the LITTER pools at a site.
        /// </summary>
        public static VarEcoregionSpecies<double> GetKNwdLitter()
        {
            KNwdLitter = new VarEcoregionSpecies<double>("KNwdLitter");

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                double siteAET = (double)AET[ecoregion]; 

                foreach (ISpecies spc in PlugIn.modelCore.Species)
                {
                    //Calculation of decomposition rate for species litter cohort
                    // Decay rate from Meentemeyer 1978.  Ecology 59: 465-472.

                    KNwdLitter[ecoregion, spc] =1.0/12.0 * (-0.5365 + (0.00241 * siteAET)) - (((-0.01586 + (0.000056 * siteAET)) * FolLignin[spc] * 100)); 
                }
            }
            return KNwdLitter;
        }
        
    }
}
