using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IEcoregionParameters
    {
        float PrecLossFrac
        {
            get;
        }
        float PrecIntConst
        {
            get;
        }
        float RootingDepth
        {
            get;
        }
        float WaterHoldingCapacity
        {
            get;
        }
        float LeakageFrac
        {
            get;
        }
        
    }
}
