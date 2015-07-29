using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.InitialCommunities;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;
using System.Linq;

/*
namespace Landis.Extension.Succession.BiomassPnET
{
    public class SpeciesParameters
    {
        static Landis.Library.Parameters.Species.AuxParm<ISpecies> AllSpecies = new Library.Parameters.Species.AuxParm<ISpecies>(PlugIn.ModelCore.Species);

        public static Landis.Library.BiomassCohortsPnET.PnETSpecies[] List
        {
            get
            {
                return AllSpecies.Values;
            }
        }

        public static Landis.Library.BiomassCohortsPnET.PnETSpecies get(ISpecies species)
        {
            return AllSpecies[species];
        }
        private static Landis.Library.Parameters.Species.AuxParm<float> FolLignin;
        private static Landis.Library.Parameters.Species.AuxParm<float> CFracBiomass;
        private static Landis.Library.Parameters.Species.AuxParm<float> SLWDel;
        private static Landis.Library.Parameters.Species.AuxParm<float> SLWmax;
        private static Landis.Library.Parameters.Species.AuxParm<float> FracFol;
        private static Landis.Library.Parameters.Species.AuxParm<float> DNSC;
        private static Landis.Library.Parameters.Species.AuxParm<float> FracBelowG;
        private static Landis.Library.Parameters.Species.AuxParm<float> TOfol;
        private static Landis.Library.Parameters.Species.AuxParm<float> TOroot;
        private static Landis.Library.Parameters.Species.AuxParm<float> TOwood;
        private static Landis.Library.Parameters.Species.AuxParm<float> BFolResp;
        private static Landis.Library.Parameters.Species.AuxParm<float> PsnAgeRed;
        private static Landis.Library.Parameters.Species.AuxParm<float> KWdLit;
        private static Landis.Library.Parameters.Species.AuxParm<float> FrActWd;
        private static Landis.Library.Parameters.Species.AuxParm<float> InitialNSC;
        private static Landis.Library.Parameters.Species.AuxParm<bool> PreventEstablishment;
        private static Landis.Library.Parameters.Species.AuxParm<float> HalfSat;
        private static Landis.Library.Parameters.Species.AuxParm<float> PsnTMin;
        private static Landis.Library.Parameters.Species.AuxParm<float> EstRad;
        private static Landis.Library.Parameters.Species.AuxParm<float> EstMoist;
        private static Landis.Library.Parameters.Species.AuxParm<float> K;
        private static Landis.Library.Parameters.Species.AuxParm<int> H2;
        private static Landis.Library.Parameters.Species.AuxParm<int> H3;
        private static Landis.Library.Parameters.Species.AuxParm<int> H4;
        private static Landis.Library.Parameters.Species.AuxParm<float> MaintResp;
        private static Landis.Library.Parameters.Species.AuxParm<float> PsnTOpt;
        private static Landis.Library.Parameters.Species.AuxParm<float> DVPD1;
        private static Landis.Library.Parameters.Species.AuxParm<float> DVPD2;
        private static Landis.Library.Parameters.Species.AuxParm<float> WUEcnst;
        private static Landis.Library.Parameters.Species.AuxParm<float> AmaxA;
        private static Landis.Library.Parameters.Species.AuxParm<float> AmaxB;
        private static Landis.Library.Parameters.Species.AuxParm<float> Q10;
        private static Landis.Library.Parameters.Species.AuxParm<float> FolN;
         

        
        public static void Initialize()
        {
            CFracBiomass = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("CFracBiomass", 0, 1);
            SLWDel = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("SLWDel", 0, float.MaxValue);
            SLWmax = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("SLWmax", 0, float.MaxValue);
            FracFol = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FracFol", 0, 1);
            DNSC = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DNSC", 0, 1);
            FracBelowG = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FracBelowG", 0, 1);
            TOroot = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOroot", 0, 1);
            TOwood = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOwood", 0, 1);
            TOfol = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("TOfol", 0, float.MaxValue);
            PsnAgeRed = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnAgeRed", 0, float.MaxValue);
            BFolResp = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("BFolResp", 0, float.MaxValue);
            KWdLit = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("KWdLit", 0, 1);
            FrActWd = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FrActWd", 0, float.MaxValue);
            KWdLit = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("KWdLit", 0, 1);
            InitialNSC = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("InitialNSC", 0, float.MaxValue);
            PreventEstablishment = (Landis.Library.Parameters.Species.AuxParm<bool>)(Parameter<bool>)PlugIn.GetParameter("PreventEstablishment");
            EstRad = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("EstRad", 0, float.MaxValue);
            EstMoist = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("EstMoist", 0, float.MaxValue);
            PsnTMin = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnTMin", -10, 10);
            HalfSat = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("HalfSat", 0, float.MaxValue);
            FolLignin = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FolLignin", 0, float.MaxValue);
            K = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("K", 0, float.MaxValue);
            H2 = (Landis.Library.Parameters.Species.AuxParm<int>)(Parameter<int>)PlugIn.GetParameter("H2", 0, int.MaxValue);
            H3 = (Landis.Library.Parameters.Species.AuxParm<int>)(Parameter<int>)PlugIn.GetParameter("H3", 0, int.MaxValue);
            H4 = (Landis.Library.Parameters.Species.AuxParm<int>)(Parameter<int>)PlugIn.GetParameter("H4", 0, int.MaxValue);
            FolN = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("FolN", 0, 10);

            MaintResp = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("MaintResp", 0, 1);
            PsnTOpt = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PsnTOpt", 0, 30);
            DVPD1 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DVPD1", 0, 10);
            DVPD2 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("DVPD2", 0, 10);
            WUEcnst = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("WUEcnst", 0, 200);
            AmaxA = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("AmaxA", -200, 200);
            AmaxB = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("AmaxB", 0, float.MaxValue);
            Q10 = (Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("Q10", 0, 10);

            foreach (ISpecies species in PlugIn.ModelCore.Species)
            {
                AllSpecies[species] = new Landis.Library.BiomassCohortsPnET.PnETSpecies
                                                (species, FolN[species], MaintResp[species], PsnTOpt[species], DVPD1[species], DVPD2[species], WUEcnst[species], AmaxA[species],
                                                AmaxB[species], Q10[species], CFracBiomass[species], SLWDel[species], SLWmax[species], FracFol[species], DNSC[species], FracBelowG[species],
                                                 TOroot[species], TOwood[species], TOfol[species], PsnAgeRed[species], BFolResp[species], KWdLit[species], FrActWd[species], InitialNSC[species],
                                                 PreventEstablishment[species], EstRad[species], EstMoist[species], PsnTMin[species], HalfSat[species], FolLignin[species], K[species], H2[species],
                                                 H3[species], H4[species]);


            }
          
        }

         

    }
}
*/