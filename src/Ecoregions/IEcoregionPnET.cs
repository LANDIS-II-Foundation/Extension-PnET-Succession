using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEcoregionPnET : IEcoregion
    {
        float LeakageFrac
        {
            get;
          
        }
        float PrecLossFrac { get; }
        string SoilType { get; }
        
        float PrecIntConst { get;  }
        float RootingDepth { get;   }
        float FieldCap { get; set; }
        float WiltPnt { get; set; }
        float Porosity { get; set; }

        EcoregionPnETVariables Variables { get; set; }
        
    }
}
