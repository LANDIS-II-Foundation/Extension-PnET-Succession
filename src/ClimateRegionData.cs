//  Author: Robert Scheller, Melissa Lucash

using Landis.Core;
using Landis.Library.Climate;
using System.Linq;
using Landis.Library.DensityCohorts;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class ClimateRegionData
    {
        public static Library.Parameters.Ecoregions.AuxParm<AnnualClimate_Monthly> AnnualWeather;

        //---------------------------------------------------------------------
        //public static void Initialize(IInputParameters parameters)
        public static void Initialize()
        {
            AnnualWeather = new Library.Parameters.Ecoregions.AuxParm<AnnualClimate_Monthly>(PlugIn.ModelCore.Ecoregions);

            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions)
            {
                if (ecoregion.Active)
                {
                    // Latitude is contained in the PnET Ecoregion
                    Climate.GenerateEcoregionClimateData(ecoregion, 0, EcoregionData.GetPnETEcoregion(ecoregion).Latitude);
                    SetSingleAnnualClimate(ecoregion, 0, Climate.Phase.SpinUp_Climate);  // Some placeholder data to get things started.
                }
            }
        }

        public static void SetSingleAnnualClimate(IEcoregion ecoregion, int year, Climate.Phase spinupOrfuture)
        {
            int actualYear = Climate.Future_MonthlyData.Keys.Min() + year;

            if (spinupOrfuture == Climate.Phase.Future_Climate)
            {
                if (Climate.Future_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Future_MonthlyData[actualYear][ecoregion.Index];
                }
            }
            else
            {
                if (Climate.Spinup_MonthlyData.ContainsKey(actualYear))
                {
                    AnnualWeather[ecoregion] = Climate.Spinup_MonthlyData[actualYear][ecoregion.Index];
                }
            }
        }
    }
}
