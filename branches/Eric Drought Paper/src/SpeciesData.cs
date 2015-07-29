//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.SpatialModeling;
using Landis.Library.BiomassCohortsPnET;
using Landis.Core;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.Succession;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class SpeciesData
    {
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> GRespFrac;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> DFol;
        
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> DRoot;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> DStem;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> LeafLignin;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> FoliageTurnover;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> FolReten;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> RootTurnover;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> WoodTurnover;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> MortCurveShapeParm;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> GrowthCurveShapeParm;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> HalfSat;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> EstRadSensitivity;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> EstMoistureSensitivity;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> PsnTOpt;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> AmaxA;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> AmaxB;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> PsnTMin;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> DVPD1;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> DVPD2;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> FolNCon;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> BaseFolRespFrac;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> GrowthMoistureSensitivity;
        
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> SLWmax;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> k;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<int> SenescStart;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> WoodLongevity;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> WUEConst;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> RespQ10;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> PsnAgeRed;
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> InitialFol; 
        public static Landis.Extension.Succession.Biomass.Species.AuxParm<float> GDDFolStart;

        

        //---------------------------------------------------------------------
        public static void Initialize(IInputParameters parameters)
        {
            RootTurnover = parameters.RootTurnover;
            WoodTurnover = parameters.WoodTurnover;
            FoliageTurnover = parameters.FoliageTurnover;
            FolReten = parameters.FolReten;
            LeafLignin = parameters.LeafLignin;
            MortCurveShapeParm      = parameters.MortCurveShapeParm;
            GrowthCurveShapeParm = parameters.GrowthCurveShapeParm;
            HalfSat = parameters.HalfSat;
            EstRadSensitivity = parameters.EstRadSensitivity;
            EstMoistureSensitivity = parameters.EstMoistureSensitivity;
            PsnTOpt = parameters.PsnTOpt;
            AmaxA = parameters.AmaxA;
            AmaxB = parameters.AmaxB;
            PsnTMin = parameters.PsnTMin;
            DVPD1 = parameters.DVPD1;
            DVPD2 = parameters.DVPD2;
            FolNCon = parameters.FolNCon;
            BaseFolRespFrac = parameters.BaseFolRespFrac;
            GrowthMoistureSensitivity = parameters.GrowthMoistureSensitivity;
            SLWmax = parameters.SLWmax;
            k=parameters.K;
            SenescStart = parameters.SenescStart;
            WUEConst = parameters.WUEConst;
            RespQ10 = parameters.RespQ10;
            PsnAgeRed = parameters.PsnAgeRed;
            InitialFol = parameters.InitialFol;
            GDDFolStart = parameters.GDDFolStart;
            DFol = parameters.DFol;
            GRespFrac = parameters.GRespFrac;
            DRoot = parameters.DRoot;
            DStem = parameters.DStem;
        }

        

    }
}
