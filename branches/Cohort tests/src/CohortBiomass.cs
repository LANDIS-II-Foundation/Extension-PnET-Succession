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
    public class CohortBiomass  

    {
        const float MaxNscFrac = 0.035F;

        private  float fage;
        private  float folalloc;
        private  float releasednsc;
        private  float rootalloc;
        private  float woodalloc;
        private  float rootsenescence;
        private  float woodsenescence;
        private  float fWater;
        private  float folresp;
        private  float grosspsn;
        private  float transpiration;
        private  float belowcanopyradiation;
        private float abovecanopyradiation;
        private  float slwlayer;
        private  float netpsn;
        
        private static Landis.Library.Biomass.Species.AuxParm<float> RtStRatio;
        private static Landis.Library.Biomass.Species.AuxParm<float> MaintRespFrac;
        private static Landis.Library.Biomass.Species.AuxParm<float> DNSC;
        private static Landis.Library.Biomass.Species.AuxParm<float> TOfol;
        private static Landis.Library.Biomass.Species.AuxParm<float> FolRet;
        private static Landis.Library.Biomass.Species.AuxParm<float> TOroot;
        private static Landis.Library.Biomass.Species.AuxParm<float> TOwood;
        private static Landis.Library.Biomass.Species.AuxParm<float> HalfSat;
        private static Landis.Library.Biomass.Species.AuxParm<float> BFolResp;
        private static Landis.Library.Biomass.Species.AuxParm<float> PsnTMin;
        private static Landis.Library.Biomass.Species.AuxParm<float> GrMstSens;
        private static Landis.Library.Biomass.Species.AuxParm<float> SLWmax;
        private static Landis.Library.Biomass.Species.AuxParm<float> SLWDel;
        private static Landis.Library.Biomass.Species.AuxParm<float> k;
        private static Landis.Library.Biomass.Species.AuxParm<int> CDDFolEnd;
        private static Landis.Library.Biomass.Species.AuxParm<float> PsnAgeRed;
        private static Landis.Library.Biomass.Species.AuxParm<float> GDDFolSt;

        private static Landis.Library.Biomass.Ecoregions.AuxParm<int> Porosity;


        private static bool hasdeadcohorts;
        public static bool HasDeadCohorts
        {
            get
            {
                return hasdeadcohorts;
            }
            set
            {
                hasdeadcohorts = value;
            }

        }
        private float wue;
        
        private float autotrophicrespiration;


        private Cohort cohort;

        public float WUE {  get { return wue;   }     }
        public float Autotrophicrespiration    {  get  { return autotrophicrespiration;   }      }
        public float BelowCanopyPAR { get { return belowcanopyradiation; } }
        public float AboveCanopyRadiation { get { return abovecanopyradiation; } }
        public float FWater {   get  {  return fWater;}     }
        public float GrossPsn  {  get    {     return grosspsn;   }}
        public float NetPsn { get { return netpsn; } }
        public float FolResp   {    get  {    return folresp;    } }
        public float Fage {   get    {    return fage;   }   }
        //public float FRad  { get{  return Average(frad); }  }
        public float FRad { get { return frad[0]; } }
        public float Transpiration {     get   {    return transpiration;  } }
    
        public Cohort Cohort {   get { return cohort;     }  }
        public float ReleasedNSC   {     get    {     return releasednsc;  }     }
        public float Folalloc  {    get     {     return folalloc;    } }
        public float WoodAlloc   { get    {  return woodalloc;     }  }
        public float RootAlloc {    get   {     return rootalloc; }    }

        
        public float[] frad;
        float Average(float[] values)
        {
            float sum = 0;
            foreach (float v in values)
            {
                sum += v;
            }
            return sum / (float)values.Length;
        }

        public static void Initialize(IInputParameters parameters)
        {
            Canopy.Initialize(parameters);

            TOroot = parameters.TOroot;
            TOwood = parameters.TOwood;
            TOfol = parameters.TOfol;
            FolRet = parameters.FolRet;
            HalfSat = parameters.HalfSat;
            PsnTMin = parameters.PsnTMin;
            GrMstSens = parameters.GrMstSens;
        
            SLWmax = parameters.SLWmax;
            SLWDel = parameters.SLWDel;
            k = parameters.K;
            CDDFolEnd = parameters.CDDFolEnd;
            PsnAgeRed = parameters.PsnAgeRed;
            GDDFolSt = parameters.GDDFolSt;
             
            RtStRatio = parameters.RtStRatio;
            MaintRespFrac = parameters.MaintResp;
            DNSC = parameters.DNSC;
            BFolResp = parameters.BFolResp;
            Porosity = parameters.Porosity;
        }
        private static int CurrentImax;

        public float SumLAI()
        {
            float[] cumlai = CumulativeLAI();
            return cumlai[cumlai.Length - 1];
        }

        private float[] CumulativeLAI()
        {
            float[] cumlai = new float[CurrentImax];
            float lai = 0;
            for (int ix = 0; ix < CurrentImax; ix++)
            {
                slwlayer = Math.Max(0.75F * SLWmax[Cohort.Species], SLWmax[Cohort.Species] - (SLWDel[Cohort.Species] * (ix * (cohort.Fol / (float)CurrentImax))));                        // SLW for this layer
                lai += (cohort.Fol / CurrentImax) / slwlayer;              // cumulative LAI for whole canopy to this depth
                cumlai[ix] = lai;
            }//for ix=1 to 50
            return cumlai;
        }
        public void SetRadiationStress(int canopylayer, float AboveCanopyRadiation, int IMAX, out float cumulative_lai)
        {
            CurrentImax = IMAX;
            netpsn = 0;
            grosspsn = 0;
            folresp = 0;
            transpiration = 0;
            frad = new float[IMAX];
            this.abovecanopyradiation = AboveCanopyRadiation;
            fage = Math.Max(0, 1 - (float)Math.Pow((cohort.Age / (float)cohort.Species.Longevity), PsnAgeRed[Cohort.Species]));


            if (cohort.Fol > 0)
            {
                float[] lai = CumulativeLAI();
                cumulative_lai = lai[lai.Length - 1];

                for (int ix = 0; ix < IMAX; ix++)
                {
                    belowcanopyradiation = AboveCanopyRadiation * (float)Math.Exp(-k[Cohort.Species] * lai[ix]);

                    // light intensity at this level
                    frad[ix] = (1 - (float)Math.Exp(-belowcanopyradiation * Math.Log(2) / HalfSat[Cohort.Species]));

                }//for ix=1 to 50
            }//if FolMass>0
            else
            {
                belowcanopyradiation = AboveCanopyRadiation;
                cumulative_lai = 0;
            }

            
        }

        public CohortBiomass(Cohort Cohort)
        {
            this.cohort = Cohort;
        }
        public bool ComputePhotosynthesis(DateTime Date, IEcoregion ecoregion, int ix, int IMAX, Hydrology hydrology, ref float transpiration)
        {
            rootalloc = 0;
            folalloc = 0;
            woodalloc = 0;
            releasednsc = 0;
            netpsn = 0;
            folresp = 0;
            grosspsn = 0;

            if (StaticVariables.Leaf_On[ecoregion, Cohort.Species, Date] == false) return true;

            fWater = (float)Math.Pow(Math.Max(0, ((hydrology.Water - StaticVariables.WiltingPoint_mm[ecoregion, Cohort.Species]) / (Porosity[ecoregion] - StaticVariables.WiltingPoint_mm[ecoregion, Cohort.Species]))), GrMstSens[Cohort.Species]);
            
            if (cohort.Fol > 0)
            {
                netpsn = fWater * frad[ix] * fage * StaticVariables.FTempPSN[ecoregion, Cohort.Species, Date] * StaticVariables.RefNetPsn[ecoregion, Cohort.Species, Date] * (cohort.Fol / IMAX);

                folresp = fWater * StaticVariables.DTempRespDay[ecoregion, Cohort.Species, Date] * StaticVariables.RefResp[ecoregion, Cohort.Species, Date] * (cohort.Fol / IMAX);

                grosspsn = netpsn + folresp;
            }
             
            //mm
            // Constants.MCO2 / Constants.MC: gCO2/gC
            transpiration = (grosspsn * (Constants.MCO2 / Constants.MC)) / StaticVariables.WUE_CO2_corr[ecoregion, Cohort.Species, Date];

            if (transpiration > 0) wue = netpsn / transpiration;
            else wue = 0;

            cohort.NSC += netpsn;

            
            if (cohort.NSC > (MaxNscFrac * cohort.Biomass)) releasednsc = cohort.NSC - (MaxNscFrac  * cohort.Biomass);
            else releasednsc = Math.Min((DNSC[Cohort.Species] * (1 - frad[0])) / (float)IMAX * cohort.NSC, cohort.NSC);
           
            if (cohort.NSC < 0.01F * cohort.Wood)
            {
                hasdeadcohorts = true;

                return false;
            }

            cohort.NSC -= releasednsc;
             
            folalloc = frad[frad.Length - 1] * releasednsc;

            cohort.Fol += folalloc / ConstantParameters.CFracBiomass;

            if ((cohort.Root / cohort.Wood) < RtStRatio[Cohort.Species])
            {
                rootalloc = (releasednsc - folalloc) / ConstantParameters.CFracBiomass;
                
                cohort.Root += rootalloc;
            }
            else
            {
                woodalloc = (releasednsc - folalloc) / ConstantParameters.CFracBiomass;
                //System.Console.WriteLine(cohort.Root / cohort.Wood +"\t"  + cohort.Root + "\t" + cohort.Wood + "\t" + RtStRatio[spc] + "\twoodalloc " + woodalloc);
                
                cohort.Wood += woodalloc;
            }

          
            return true;
        }
         
        public float FoliageSenescence(DateTime date, IEcoregion ecoregion, ISpecies spc)
        {
            if (StaticVariables.Leaf_On[ecoregion,spc,date] == false && StaticVariables.Leaf_On[ecoregion,spc,date.AddMonths(-1)] == true)
            {
                Cohort.FolShed = FolRet[spc] * TOfol[spc] * Cohort.Fol;

                float Litter = TOfol[spc] * Cohort.Fol;
                Cohort.Fol -= Litter;

                return Litter;
            }
            else if (StaticVariables.Leaf_On[ecoregion, spc, date] == true)
            {
                cohort.Fol += cohort.FolShed;
                cohort.FolShed = 0;
            }
            return 0;
        }
        public void MaintenanceRespiration(IEcoregion ecoregion, DateTime date)
        {
            autotrophicrespiration = Math.Min(cohort.NSC, StaticVariables.FTempResp[ecoregion, Cohort.Species, date] * MaintRespFrac[Cohort.Species] * Cohort.Biomass);
            cohort.NSC -= autotrophicrespiration;
        }
        public float WoodSenescence()
        {
            rootsenescence = Cohort.Root * TOroot[Cohort.Species] / 12;
            Cohort.Root -= rootsenescence;

            woodsenescence = Cohort.Wood * TOwood[Cohort.Species] / 12;
            Cohort.Wood -= woodsenescence;
            

            return woodsenescence + rootsenescence;
        }
        public Percentage ComputeNonWoodyPercentage(ICohort cohort, ActiveSite site)
        {
            return new Percentage(cohort.Fol / (cohort.Wood + cohort.Fol));
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
