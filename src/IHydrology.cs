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
        void AddWater(float water);
        void SubtractEvaporation(SiteCohorts sitecohorts);
        void SubtractTranspiration(IEcoregionPnET Ecoregion, ushort transpiration);
    }
}
