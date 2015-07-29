//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller, James B. Domingo

 

using Landis.Library.Succession;
using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.Diagnostics;

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

        List<int> CanopyLayerAges { get; }
        List<float[]> CanopyLayerBiomFractions { get; }
        List<int> CanopyLayerNumbers { get; }
        

        
     
        Landis.Library.Biomass.Species.AuxParm<float> RtStRatio { get; }
        Landis.Library.Biomass.Species.AuxParm<float> DNSC { get; }
        Landis.Library.Biomass.Species.AuxParm<float> MaintResp { get; }
        Landis.Library.Biomass.Species.AuxParm<float> TOwood { get; }
        Landis.Library.Biomass.Species.AuxParm<float> TOroot { get; }
        Landis.Library.Biomass.Species.AuxParm<float> TOfol { get; }
        Landis.Library.Biomass.Species.AuxParm<float> FolRet { get; }
        Landis.Library.Biomass.Species.AuxParm<float> HalfSat { get; }
        Landis.Library.Biomass.Species.AuxParm<float> EstRad { get; }
        Landis.Library.Biomass.Species.AuxParm<float> EstMoist { get; }
        Landis.Library.Biomass.Species.AuxParm<float> PsnTOpt { get; }
        Landis.Library.Biomass.Species.AuxParm<float> AmaxA { get; }
        Landis.Library.Biomass.Species.AuxParm<float> AmaxB { get; }
        Landis.Library.Biomass.Species.AuxParm<float> PsnTMin { get; }
        Landis.Library.Biomass.Species.AuxParm<float> DVPD1 { get; }
        Landis.Library.Biomass.Species.AuxParm<float> DVPD2 { get; }
        Landis.Library.Biomass.Species.AuxParm<float> FolN { get; }
        Landis.Library.Biomass.Species.AuxParm<float> BFolResp { get; }
        Landis.Library.Biomass.Species.AuxParm<float> GrMstSens { get; }
        Landis.Library.Biomass.Species.AuxParm<float> WltPnt { get; }
        Landis.Library.Biomass.Species.AuxParm<float> SLWDel { get; }
        Landis.Library.Biomass.Species.AuxParm<float> SLWmax { get; }
        Landis.Library.Biomass.Species.AuxParm<int> CDDFolEnd { get; }
        Landis.Library.Biomass.Species.AuxParm<float> WUEcnst { get; }   
        Landis.Library.Biomass.Species.AuxParm<float> Q10 { get; }
        Landis.Library.Biomass.Species.AuxParm<float> PsnAgeRed { get; }
        Landis.Library.Biomass.Species.AuxParm<float> GDDFolSt { get; }
        Landis.Library.Biomass.Species.AuxParm<float> K { get; }
        Landis.Library.Biomass.Species.AuxParm<float> FolLignin { get; }
        Landis.Library.Biomass.Species.AuxParm<float> KWdLit { get; }

        Library.Biomass.Ecoregions.AuxParm<string>climateFileName { get; }
         
       
        int Latitude { get; }

        Landis.Library.Biomass.Ecoregions.AuxParm<int> WHC { get; }
        Landis.Library.Biomass.Ecoregions.AuxParm<int> AET { get; }
        Landis.Library.Biomass.Ecoregions.AuxParm<float> PrecipLossFrac { get; }
        Landis.Library.Biomass.Ecoregions.AuxParm<int> Porosity { get; }
        Landis.Library.Biomass.Ecoregions.AuxParm<float> LeakageFrac { get; }
        
         
        string DynamicInputFile {get;set;}

        string AgeOnlyDisturbanceParms{get; set;}
    }

    
}
