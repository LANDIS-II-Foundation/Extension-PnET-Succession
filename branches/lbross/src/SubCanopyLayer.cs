
using Landis.Core;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{

    public class SubCanopyLayer  
    {
        public static float GetFWater(float H2, float H3, float H4, float pressurehead)
        {
            if (pressurehead < 0 || pressurehead > H4) return 0;
            else if (pressurehead > H3) return 1 - ((pressurehead - H3) / (H4 - H3));
            else if (pressurehead < H2) return pressurehead / H2;
            else return 1;
        }
        public static float GetFWater(ISpecies species, float pressurehead)
        {
            if (pressurehead < 0 || pressurehead > species.H4()) return 0;
            else if (pressurehead > species.H3()) return 1 - ((pressurehead - species.H3()) / (species.H4() - species.H3()));
            else if (pressurehead < species.H2()) return pressurehead / species.H2();
            else return 1;
        }
        public static float CumputeFrad(float Radiation, float HalfSat)
        {
            return Radiation / (Radiation + HalfSat);
        }
        public Cohort cohort;

        public byte LayerIndex;
        public byte CanopyLayer;

        public ushort CumCohortBiomass;
        public ushort  Radiation;
        public float Fwater;
        public float Frad;
        public float LAI;
        public float NetPsn;
        public float FolResp;
        public float Transpiration;
        
        public ISpecies Species
        {
            get
            {
                return cohort.Species;
            }
        }

        public SubCanopyLayer(Cohort cohort, byte LayerIndex)
        {
            this.cohort = cohort;
            this.LayerIndex = LayerIndex;
        }
        public void ComputePhotosynthesis(ref float IncomingRadiation, int PressureHead, EcoregionDateData data, ref ushort Water)
        {
            if (LayerIndex == 0)
            {
                cohort.MaintenanceRespiration = (ushort) Math.Min(cohort.NSC, data.FTempRespMaintResp[cohort.species] * (cohort.Root + cohort.Wood));//gC //IMAXinverse
                cohort.NSC -=  cohort.MaintenanceRespiration;
            }

            LAI = cohort.Fol / (cohort.species.SLWmax() / (1 / (float)PlugIn.IMAX) - cohort.species.SLWDel() * (float)LayerIndex * (1 / (float)PlugIn.IMAX) * cohort.Fol);
 
            Radiation = (ushort)(IncomingRadiation * (float)Math.Exp(-cohort.species.K() * LAI));

            Frad = CumputeFrad(Radiation, cohort.species.HalfSat());

            Fwater = GetFWater(cohort.species, PressureHead);

            NetPsn = Fwater * Frad * cohort.Fage() * data.FTempPSNRefNetPsn[cohort.species] * cohort.Fol * (1 / (float)PlugIn.IMAX);

            cohort.NSC += NetPsn;

            FolResp = Fwater * data.FTempRespDayRefResp[cohort.species] * cohort.Fol * (1 / (float)PlugIn.IMAX);

            float GrossPsn = NetPsn + FolResp;

            if (GrossPsn > 0)
            {
                Transpiration = (GrossPsn * Constants.MCO2_MC) / data.WUE_CO2_corr[cohort.species];

                if (Transpiration>0) Hydrology.SubtractTranspiration((ushort)Transpiration, ref Water);
            }

            if (data.Leaf_On[cohort.Species])
            {
                //float IdealFol = (cohort.species.FracFol() * cohort.FActiveBiom * cohort.MaxBiomass);
                float IdealFol = (cohort.species.FracFol() * cohort.FActiveBiom() * cohort.Biomass);

                if (IdealFol > cohort.Fol)
                {
                    float Folalloc = Math.Max(0, Math.Min(cohort.NSC, cohort.species.CFracBiomass() * IdealFol - cohort.Fol)); // gC/mo

                    if (Folalloc > 0) cohort.Fol += (ushort)(Folalloc / cohort.species.CFracBiomass());// gDW

                    cohort.NSC -= (ushort)Folalloc;
                }

                //if (sl.LayerIndex == sl.cohort.NrOfSublayers-1)
                //{
                float rootpluswoodalloc = Math.Max(cohort.NSC - (cohort.species.DNSC() * cohort.FActiveBiom() * (cohort.Biomass + cohort.Root)), 0);//gC

                if (rootpluswoodalloc > 0)
                {
                    cohort.NSC -= (ushort)rootpluswoodalloc;

                    float RootAlloc = Math.Min(rootpluswoodalloc, cohort.species.CFracBiomass() * Math.Max(0, (cohort.species.FracBelowG() * (cohort.Root + cohort.Wood)) - cohort.Root));// gC
                    cohort.Root += (ushort)(RootAlloc / cohort.species.CFracBiomass()); // gDW

                    float WoodAlloc = rootpluswoodalloc - RootAlloc;// gC

                    cohort.Wood += (ushort)(WoodAlloc / cohort.species.CFracBiomass());// DW

                }
                //}
            
            }
         
        }
        
        
    }
}
