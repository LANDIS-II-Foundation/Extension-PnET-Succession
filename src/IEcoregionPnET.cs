using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEcoregionPnET : IEcoregion
    {
        float LeakageFrac{ get; } // Proportion of water above field capacity that drains out of the soil rooting zone immediately after entering the soil (fast leakage)
        float RunoffCapture { get; } // Depth of surface water (mm) that can be held on site instead of running off
        float PrecLossFrac { get; } // Proportion of incoming precipitation that does not enter the soil - surface runoff due to impermeability, slope, etc.
        string SoilType { get; }        
        float PrecIntConst { get; } // Rate at which incoming precipitation is intercepted by foliage for each unit of LAI
        float RootingDepth { get; } // Depth of rooting zone in the soil (mm)
        float FieldCap { get; set; } // Volumetric water content (mm/m) at field capacity
        float WiltPnt { get; set; } // Volumetric water content (mm/m) at wilting point
        float Porosity { get; set; } // Volumetric water content (mm/m) at porosity
        float SnowSublimFrac { get; } // Proportion of snow pack that sublimates before melting
        float LeakageFrostDepth { get; }
        int PrecipEvents { get; }
        float Latitude { get; } 
        float WinterSTD { get; }
        float MossDepth { get; }
        float EvapDepth { get; } // Maximum soil depth susceptible to surface evaporation


        IEcoregionPnETVariables Variables { get; set; }
        
    }
}
