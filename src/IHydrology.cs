using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Core;

using Landis.SpatialModeling;


 namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IHydrology
    {
        float Water { get; }
        float GetPressureHead(IEcoregionPnET ecoregion);
        bool AddWater(float water);
        bool SubtractEvaporation(SiteCohorts sitecohorts);
        bool SubtractTranspiration(IEcoregionPnET Ecoregion, ushort transpiration);
    }
}
