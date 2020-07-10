using Landis.Core;

namespace Landis.Library.DensityCohorts
{
    public interface IEcoregionData : IEcoregion
    {
        //float LeakageFrac{ get;}
        //float RunoffFrac { get; }
        //float PrecLossFrac { get; }
        //string SoilType { get; }        
        //float PrecIntConst { get;  }
        //float RootingDepth { get;   }
        //float FieldCap { get; set; }
        //float WiltPnt { get; set; }
        //float Porosity { get; set; }
        //float SnowSublimFrac { get; }
        //float LeakageFrostDepth { get; }
        //int PrecipEvents { get; }
        float Latitude { get; } 
        //float WinterSTD { get; }
        
       
        //IEcoregionClimateVariables Variables { get; set; }
        
    }
}
