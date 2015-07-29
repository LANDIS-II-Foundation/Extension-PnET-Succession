//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

using Landis.Library.Succession;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using Landis.SpatialModeling;
 
 

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public interface IInputParameters
    {
        int StartYear { get; }
        int Timestep {get; set;}
         
        
        SeedingAlgorithms SeedAlgorithm {get; set;}
        string InitialCommunities{get; set;}
        string InitialCommunitiesMap{get; set;}

        ISiteVar<bool> HasSiteOutput { get; }

        Landis.Library.Biomass.Ecoregions.AuxParm<float> WHC { get; }
     
        Landis.Library.Biomass.Species.AuxParm<float> RootStemRatio { get; }
        Landis.Library.Biomass.Species.AuxParm<float> DNSC { get; }
        Landis.Library.Biomass.Species.AuxParm<float> MaintResp { get; }
        Landis.Library.Biomass.Species.AuxParm<float> WoodTurnover { get; }
        Landis.Library.Biomass.Species.AuxParm<float> RootTurnover { get; }
        Landis.Library.Biomass.Species.AuxParm<float> FoliageTurnover { get; }
        Landis.Library.Biomass.Species.AuxParm<float> FolReten { get; }
        Landis.Library.Biomass.Species.AuxParm<float> HalfSat { get; }
        Landis.Library.Biomass.Species.AuxParm<float[]> EstRadSensitivity { get; }
        Landis.Library.Biomass.Species.AuxParm<float[]> EstMoistureSensitivity { get; }
        Landis.Library.Biomass.Species.AuxParm<float> PsnTOpt { get; }
        Landis.Library.Biomass.Species.AuxParm<float> AmaxA { get; }
        Landis.Library.Biomass.Species.AuxParm<float> AmaxB { get; }
        Landis.Library.Biomass.Species.AuxParm<float> PsnTMin { get; }
        Landis.Library.Biomass.Species.AuxParm<float> DVPD1 { get; }
        Landis.Library.Biomass.Species.AuxParm<float> DVPD2 { get; }
        Landis.Library.Biomass.Species.AuxParm<float> FolNCon { get; }
        Landis.Library.Biomass.Species.AuxParm<float> BaseFolRespFrac { get; }
        Landis.Library.Biomass.Species.AuxParm<float> GrowthMoistureSensitivity { get; }
        Landis.Library.Biomass.Species.AuxParm<float> WiltingPoint { get; }
        Landis.Library.Biomass.Species.AuxParm<float> SLWDel { get; }
        Landis.Library.Biomass.Species.AuxParm<float> SLWmax { get; }
        Landis.Library.Biomass.Species.AuxParm<int> SenescStart { get; }
        Landis.Library.Biomass.Species.AuxParm<float> WUEConst { get; }   
        Landis.Library.Biomass.Species.AuxParm<float> RespQ10 { get; }
        Landis.Library.Biomass.Species.AuxParm<float> PsnAgeRed { get; }
        Landis.Library.Biomass.Species.AuxParm<float> GDDFolStart { get; }
        Landis.Library.Biomass.Species.AuxParm<float> GDDFolEnd { get; }
        Landis.Library.Biomass.Species.AuxParm<float> K { get; }

        string climateFileName { get; }
       
        int Latitude { get; }

        Landis.Library.Biomass.Ecoregions.AuxParm<int> AET { get; }
        Landis.Library.Biomass.Ecoregions.AuxParm<float> EvaporationFraction { get; }
        Landis.Library.Biomass.Ecoregions.AuxParm<float> LeakageFraction { get; }
        
         
        string DynamicInputFile {get;set;}

        string AgeOnlyDisturbanceParms{get; set;}
    }

    
}
