// uses dominance to allocate psn and subtract transpiration from soil water, average cohort vars over layer

using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using Landis.Library.Biomass;
namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Calculations for an individual cohort's biomass.
    /// </summary>
    public class CohortBiomass //: ICohortBiomass 

    {
        private ActiveSite site;
        private ISpecies spc;
        private static float fage;
        private static float waterconsumption;
        private static float folalloc;
        private static float releasednsc;
        private static float rootalloc;
        private static float woodalloc;
        private static float rootsenescence;
        private static float woodsenescence;
        private static float IMAX = 5;
        private static float RootPlusWoodAlloc;
        private static float dWater;
        private static float wfps;
        private static float abovecanopyradiation;
        private static float netpsn;
        private static float folresp;
        private static float WiltingPoint_mm;
        private static float grosspsn;
        private static float frad;
        private static float transpiration;
        private static float belowcanopyradiation;
        private static float slwlayer;

        private static Landis.Library.Biomass.Species.AuxParm<float> RootStemRatio;
        private static Landis.Library.Biomass.Species.AuxParm<float> MaintRespFrac;
        private static Landis.Library.Biomass.Species.AuxParm<float> DNSC;
        private static Landis.Library.Biomass.Species.AuxParm<float> FoliageTurnover;
        private static Landis.Library.Biomass.Species.AuxParm<float> FolReten;
        private static Landis.Library.Biomass.Species.AuxParm<float> RootTurnover;
        private static Landis.Library.Biomass.Species.AuxParm<float> WoodTurnover;
         
        private static Landis.Library.Biomass.Species.AuxParm<float> HalfSat;
        private static Landis.Library.Biomass.Species.AuxParm<float> BaseFolRespFrac;
        private static Landis.Library.Biomass.Species.AuxParm<float> PsnTMin;
        private static Landis.Library.Biomass.Species.AuxParm<float> GrowthMoistureSensitivity;
        private static Landis.Library.Biomass.Species.AuxParm<float> WiltingPoint;
        private static Landis.Library.Biomass.Species.AuxParm<float> SLWmax;
        private static Landis.Library.Biomass.Species.AuxParm<float> SLWDel;
        private static Landis.Library.Biomass.Species.AuxParm<float> k;
        private static Landis.Library.Biomass.Species.AuxParm<int> SenescStart;
        private static Landis.Library.Biomass.Species.AuxParm<float> PsnAgeRed;
        private static Landis.Library.Biomass.Species.AuxParm<float> GDDFolStart;
        private static Landis.Library.Biomass.Species.AuxParm<float> GDDFolEnd;
        
        private float wue;
        private float lai;
        private float maintresp;
        private ICohort cohort;

        public float WUE
        {
            get
            {
                return wue;
            }
        }
        public float MaintResp
        {
            get
            {
                return maintresp;
            }
        }
        public float RootAlloc
        {
            get
            {
                return rootalloc;
            }
        }
        public float WoodAlloc
        {
            get
            {
                return woodalloc;
            }
        }
        public float ReleasedNSC
        {
            get
            {
                return releasednsc;
            }
        }
        public float Folalloc
        {
            get
            {
                return folalloc;
            }
        }
        public float BelowCanopyRadiation
        {
            get
            {
                return belowcanopyradiation;
            }
        
        }
        public ActiveSite Site
        {
            get
            {
                return site;
            }
        }
        public float AboveCanopyRadiation
        {
            get
            {
                return abovecanopyradiation;
            }
        }
         
        public float DWater
        {
            get
            {
                return dWater;
            }
        }
        public float WFPS
        {
            get
            {
                return wfps;
            }
        } 
        public float GrossPsn
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
        
        public float NetPsn
        {
            get
            {
                return netpsn;
            }
        }
        public float Fage
        {
            get
            {
                return fage;
            }
            private set
            {
                fage = value;
            }
        }
        public float fRad
        {
            get
            {
                return frad;
            }
            private set
            {
                frad = value;
            }
        }
        
        public float Transpiration
        {
            get
            {
                return transpiration;
            }
        }
        
        public float LAI
        {
            get
            {
                return lai;
            }
        }
        public ICohort Cohort
        {
            get
            {
                return cohort;
            }
        }
        public static void Initialize(IInputParameters parameters)
        {
            RootTurnover = parameters.RootTurnover;
            WoodTurnover = parameters.WoodTurnover;
            FoliageTurnover = parameters.FoliageTurnover;
            FolReten = parameters.FolReten;
            HalfSat = parameters.HalfSat;
            PsnTMin = parameters.PsnTMin;
            GrowthMoistureSensitivity = parameters.GrowthMoistureSensitivity;
            WiltingPoint = parameters.WiltingPoint;
            SLWmax = parameters.SLWmax;
            SLWDel = parameters.SLWDel;
            k = parameters.K;
            SenescStart = parameters.SenescStart;
            PsnAgeRed = parameters.PsnAgeRed;
            GDDFolStart = parameters.GDDFolStart;
            GDDFolEnd = parameters.GDDFolEnd;
            RootStemRatio = parameters.RootStemRatio;
            MaintRespFrac = parameters.MaintResp;
            DNSC = parameters.DNSC;
            BaseFolRespFrac = parameters.BaseFolRespFrac;
        }
        
        public CohortBiomass(ActiveSite Site, ICohort Cohort, int Index)
        {
            this.cohort = Cohort;
            fRad = 0;
            site = Site;
            spc = Cohort.Species;
         
        }
        
        public static void GrowTree(DateTime date, int canopylayer, CohortBiomass cohortbiomass, float AboveCanopyRadiation)
        {
            cohortbiomass = ComputePhotosynthesis(date, canopylayer, cohortbiomass, AboveCanopyRadiation);

            CohortBiomass.Allocation(cohortbiomass);
            CohortBiomass.MaintenanceRespiration(cohortbiomass);

            ForestFloor.Litter[cohortbiomass.site].AddMass(FoliageSenescence(cohortbiomass), 0.1);
            ForestFloor.WoodyDebris[cohortbiomass.site].AddMass(WoodSenescence(cohortbiomass), 0.1);

            CohortOutput.WriteCohortData(date, cohortbiomass.site, cohortbiomass);      
        }
        public static bool CohortIsDead(ICohort cohort)
        {
            //System.Console.WriteLine("COHORT AGE\t" + cohort.Age);
            if (cohort.NSC <= 1)
            {
                //System.Console.WriteLine("DEAD COHORT\t" + cohort.Age);
                return true;
            }
           
           return false;
        }
        
        public static void SubtractTranspiration(CohortBiomass cohortbiomass)
        {
            if (cohortbiomass.NetPsn <= 0) return;
            waterconsumption =  (float)Math.Min(Hydrology.Water[cohortbiomass.site], cohortbiomass.Transpiration);
            Hydrology.Water[cohortbiomass.site] -= waterconsumption;
            Hydrology.AnnualTranspiration[cohortbiomass.site] += waterconsumption;
            
        }

        private static float Get_frad(float radiation, float halfsat)
        {
            return (1 - (float)Math.Exp(-radiation * Math.Log(2) / halfsat ));
        }
        static System.DateTime date;
        private static CohortBiomass ComputePhotosynthesis(DateTime Date, int canopylayer, CohortBiomass cohortbiomass, float AboveCanopyRadiation)
        {
            date = Date;
            cohortbiomass.cohort.CanopyLayer = canopylayer;

            abovecanopyradiation = AboveCanopyRadiation;

            cohortbiomass.lai = 0;
            grosspsn = 0;
            folresp = 0;
            netpsn = 0;
            folresp = 0;

            fage = 1 - cohortbiomass.cohort.Age / (cohortbiomass.cohort.Age + 1 / PsnAgeRed[cohortbiomass.spc]);

            WiltingPoint_mm = WiltingPoint[cohortbiomass.spc] * Hydrology.WHC[PlugIn.modelCore.Ecoregion[cohortbiomass.site]];

            wfps = (Hydrology.Water[cohortbiomass.site] - WiltingPoint_mm) / (Hydrology.WHC[PlugIn.ModelCore.Ecoregion[cohortbiomass.site]] - WiltingPoint_mm);

            dWater = (float)Math.Pow(Math.Max(0, wfps), GrowthMoistureSensitivity[cohortbiomass.spc]);

            frad = Get_frad(AboveCanopyRadiation, HalfSat[cohortbiomass.spc]);

            if (cohortbiomass.cohort.Fol > 0)
            {

                //float PosCBalMass = cohortbiomass.cohort.Fol;
                for (int ix = 1; ix <= IMAX; ix++)
                {
                    slwlayer = SLWmax[cohortbiomass.spc] - (SLWDel[cohortbiomass.spc] * ix * (cohortbiomass.cohort.Fol / IMAX));                        // SLW for this layer
                    cohortbiomass.lai += (cohortbiomass.cohort.Fol / IMAX) / slwlayer;              // cumulative LAI for whole canopy to this depth
                    belowcanopyradiation = AboveCanopyRadiation * (float)Math.Exp(-k[cohortbiomass.spc] * cohortbiomass.lai);                             // light intensity at this level
                    frad = Get_frad(belowcanopyradiation, HalfSat[cohortbiomass.spc]);

                    netpsn += dWater * frad * fage * Static.DTempPSN[date][cohortbiomass.spc] * Static.RefNetPsn[date][cohortbiomass.spc] * (cohortbiomass.cohort.Fol / IMAX);

                    folresp += dWater * Static.DTempRespDay[date][cohortbiomass.spc] * Static.RefResp[date][cohortbiomass.spc] * (cohortbiomass.cohort.Fol / IMAX);

                }//for ix=1 to 50
            }//if FolMass>0

            grosspsn = netpsn + folresp;


            //mm
            // Constants.MCO2 / Constants.MC: gCO2/gC
            transpiration = (grosspsn * (Constants.MCO2 / Constants.MC)) / Static.WUE_CO2_corr[date][cohortbiomass.spc];

           

            // wue based on Net Photosynthesis 
            if (transpiration > 0) cohortbiomass.wue = netpsn / transpiration;
            else cohortbiomass.wue = 0;
            
            return cohortbiomass;
        }
        public static void Allocation(CohortBiomass cohortbiomass)
        {
            

            folalloc = 0;
            woodalloc = 0;
            rootalloc = 0;
            releasednsc = 0;

            cohortbiomass.cohort.NSC += cohortbiomass.NetPsn;
            if (cohortbiomass.cohort.NSC < 0) cohortbiomass.cohort.NSC = 0;


            if (Static.GDD[date][cohortbiomass.spc] > GDDFolStart[cohortbiomass.spc] &&
                date.DayOfYear < SenescStart[cohortbiomass.spc])// &&
            {
                cohortbiomass.cohort.Leaf_On = true;

                cohortbiomass.cohort.Fol +=  cohortbiomass.cohort.FolShed;
                cohortbiomass.cohort.FolShed = 0;

                releasednsc = DNSC[cohortbiomass.spc] * cohortbiomass.cohort.NSC;
                cohortbiomass.cohort.NSC -= releasednsc;

                if (Static.GDD[date][cohortbiomass.spc] < GDDFolEnd[cohortbiomass.spc])
                {
                    //System.Console.WriteLine("FRESP = " + ClimateDependentData.FResp[PlugIn.Date[cohortbiomass.site]][cohortbiomass.spc]);
                    folalloc =  frad * releasednsc;
                }
                
                cohortbiomass.cohort.Fol += folalloc / ConstantParameters.CFracBiomass;

                RootPlusWoodAlloc = releasednsc - folalloc;

                if (cohortbiomass.cohort.Root / cohortbiomass.cohort.Biomass < RootStemRatio[cohortbiomass.spc])
                {
                    rootalloc = RootPlusWoodAlloc / ConstantParameters.CFracBiomass;
                    cohortbiomass.cohort.Root += rootalloc;
                }
                else
                {
                    woodalloc = RootPlusWoodAlloc / ConstantParameters.CFracBiomass;
                    cohortbiomass.cohort.Wood += woodalloc;
                }
                 
            }
            
        }
        public static float FoliageSenescence(CohortBiomass cohortbiomass)
        {


            if (date.DayOfYear > SenescStart[cohortbiomass.spc] && cohortbiomass.Cohort.Leaf_On == true)//
            {
                cohortbiomass.Cohort.Leaf_On = false;
                //cohortbiomass.Cohort.FolShed += FolReten[cohortbiomass.spc] * FoliageTurnover[cohortbiomass.spc] * cohortbiomass.Cohort.Fol;
                //cohortbiomass.Cohort.Fol -= FoliageTurnover[cohortbiomass.spc] * cohortbiomass.Cohort.Fol;

                cohortbiomass.Cohort.FolShed = FolReten[cohortbiomass.spc] * FoliageTurnover[cohortbiomass.spc] * cohortbiomass.Cohort.Fol;
                cohortbiomass.Cohort.Fol -= FoliageTurnover[cohortbiomass.spc] * cohortbiomass.Cohort.Fol;
 
                return FoliageTurnover[cohortbiomass.spc] * cohortbiomass.Cohort.Fol;// lost foliage
            }

            return 0;
        }
        public static void MaintenanceRespiration(CohortBiomass cohortbiomass)
        {
            cohortbiomass.maintresp = Math.Min(cohortbiomass.cohort.NSC, Static.DTempResp[date][cohortbiomass.spc] * MaintRespFrac[cohortbiomass.spc] * cohortbiomass.Cohort.Biomass);
            cohortbiomass.cohort.NSC -= cohortbiomass.MaintResp;
        }
        public static float WoodSenescence(CohortBiomass cohortbiomass)
        {
            rootsenescence = cohortbiomass.Cohort.Root * RootTurnover[cohortbiomass.spc] / 12;
            cohortbiomass.Cohort.Root -= rootsenescence;
            ForestFloor.WoodyDebris[cohortbiomass.site].AddMass(rootsenescence, 0);

            woodsenescence = cohortbiomass.Cohort.Wood * WoodTurnover[cohortbiomass.spc] / 12;
            cohortbiomass.Cohort.Wood -= woodsenescence;
            ForestFloor.WoodyDebris[cohortbiomass.site].AddMass(woodsenescence, 0);

            return woodsenescence + rootsenescence;
        }
        public Percentage ComputeNonWoodyPercentage(ICohort cohort, ActiveSite site)
        {
            return new Percentage((float)cohort.Fol / (float)cohort.Biomass);
        }
        public Percentage ComputeNonWoodyPercentage(Library.BiomassCohorts.ICohort cohort, ActiveSite site)
        {
            throw new System.Exception("Incompatibility issue");
        }
        public int ComputeChange(Library.BiomassCohorts.ICohort cohort, ActiveSite site)
        {
            throw new System.Exception("Incompatibility issue");
        }
        
        
 
        
    } 
}
