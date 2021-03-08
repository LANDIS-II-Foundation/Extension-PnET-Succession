
namespace Landis.Extension.Succession.BiomassPnET
{
    public class SpeciesPnETVariables
    {
       
        public float FTempRespWeightedDayAndNight; // Unitless respiration adjustment based on temperature: for output only
        public float MaintRespFTempResp; // Scaling factor of respiration given day and night temperature and day and night length
        public float Q10Factor;  // Respiration Q10 factor
        public float BaseFolRespFrac; // Base foliar respiration fraction (using Wythers when selected)
        public float FTempPSN; // reduction factor due to temperature: for output only
        public float DelAmax;  // Adjustment to Amax based on CO2: for output only
        public float JH2O;
        public float AmaxB_CO2;
        public float DVPD; // Gradient of effect of vapour pressure deficit on growth
    }
}
