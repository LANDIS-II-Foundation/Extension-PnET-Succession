using Landis.Core;
using System;
using Landis.SpatialModeling;
using System.Collections.Generic;
 
namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IPressureHeadTable
    {

        ushort this[IEcoregion ecoregion, int water] { get; }
        float CalculateWaterContent(ushort WaterPressure /* meter pressure head*/, string soiltype);
        float Porosity(float RootingDepth, string SoilType);
    }
}
