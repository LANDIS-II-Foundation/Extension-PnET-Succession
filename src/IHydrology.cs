


namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IHydrology
    {
        float Water { get; }
        float GetPressureHead(IEcoregionPnET ecoregion);
        bool AddWater(float water);
        bool AddWater(float water, float activeSoilDepth);
        float CalculateEvaporation(SiteCohorts sitecohorts);
        float FrozenWaterPct { get; }
        float FrozenDepth { get; }
        bool SetFrozenWaterPct(float water);
        bool SetFrozenDepth(float depth);
      
    }
}
