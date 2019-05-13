


namespace Landis.Extension.Succession.BiomassPnET
{
    public interface IHydrology
    {
        float Water { get; }
        float GetPressureHead(IEcoregionPnET ecoregion);
        bool AddWater(float water);
        float CalculateEvaporation(SiteCohorts sitecohorts);
      
    }
}
