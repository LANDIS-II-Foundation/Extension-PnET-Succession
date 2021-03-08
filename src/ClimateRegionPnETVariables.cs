using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>John McNabb: This is a copy of EcoregionPnETVariables substituting MonthlyClimateRecord _monthlyClimateRecord for IObservedClimate obs_clim</summary>
    public class ClimateRegionPnETVariables : IEcoregionPnETVariables
    {
        #region private fields

        private MonthlyClimateRecord _monthlyClimateRecord;
        private DateTime _date;
        private float _vpd;
        private float _dayspan;
        private float _tave;
        private float _tday;
        private float _daylength;

        private Dictionary<string, SpeciesPnETVariables> speciesVariables;

        #endregion

        #region constructor

        public ClimateRegionPnETVariables(MonthlyClimateRecord monthlyClimateRecord, DateTime date, bool wythers, bool dTemp, List<ISpeciesPNET> Species, float latitude)
        {
            _monthlyClimateRecord = monthlyClimateRecord;

            _date = date;

            speciesVariables = new Dictionary<string, SpeciesPnETVariables>();


            _tave = (float)(0.5 * (monthlyClimateRecord.Tmin + monthlyClimateRecord.Tmax));

            _dayspan = EcoregionPnETVariables.Calculate_DaySpan(date.Month);

            float hr = EcoregionPnETVariables.Calculate_hr(date.DayOfYear, latitude);
            _daylength = EcoregionPnETVariables.Calculate_DayLength(hr);
            float nightlength = EcoregionPnETVariables.Calculate_NightLength(hr);

            _tday = (float)(0.5 * (monthlyClimateRecord.Tmax + _tave));
            _vpd = EcoregionPnETVariables.Calculate_VPD(Tday, (float)monthlyClimateRecord.Tmin);


            foreach (ISpeciesPNET spc in Species)
            {
                SpeciesPnETVariables speciespnetvars = GetSpeciesVariables(monthlyClimateRecord, wythers, dTemp, Daylength, nightlength, spc);

                speciesVariables.Add(spc.Name, speciespnetvars);
            }
        }

        #endregion

        #region properties

        public float VPD => _vpd;
        public byte Month => (byte)_date.Month;
        public float Tday => _tday;
        public float Prec => (float)_monthlyClimateRecord.Prec;
        public float O3 => (float)_monthlyClimateRecord.O3;
        public float CO2 => (float)_monthlyClimateRecord.CO2;
        public float PAR0 => (float)_monthlyClimateRecord.PAR0;
        public DateTime Date => _date;
        public float DaySpan => _dayspan;
        public float Time => _date.Year + 1F / 12F * (_date.Month - 1);
        public int Year => _date.Year;
        public float Tave => _tave;
        public float Tmin => (float)_monthlyClimateRecord.Tmin;
        public float Tmax => (float)_monthlyClimateRecord.Tmax;
        public float Daylength => _daylength;

        public SpeciesPnETVariables this[string species] => speciesVariables[species];

        #endregion

        #region private methods

        private SpeciesPnETVariables GetSpeciesVariables(MonthlyClimateRecord monthlyClimateRecord, bool wythers, bool dTemp, float daylength, float nightlength, ISpeciesPNET spc)
        {
            // Class that contains species specific PnET variables for a certain month
            SpeciesPnETVariables speciespnetvars = new SpeciesPnETVariables();

            // Gradient of effect of vapour pressure deficit on growth. 
            speciespnetvars.DVPD = Math.Max(0, 1 - spc.DVPD1 * (float)Math.Pow(VPD, spc.DVPD2));

            // ** CO2 effect on growth **
            // M. Kubiske method for wue calculation:  Improved methods for calculating WUE and Transpiration in PnET.
            float JH2O = (float)(0.239 * ((VPD / (8314.47 * (monthlyClimateRecord.Tmin + 273)))));
            speciespnetvars.JH2O = JH2O;

            // NETPSN net photosynthesis
            // Modify AmaxB based on CO2 level
            // Equations solved from 2 known points: (350, AmaxB) and (550, AmaxB * CO2AmaxBEff)
            float AmaxB_slope = (float)(((spc.CO2AMaxBEff - 1.0) * spc.AmaxB) / 200.0);  // Derived from m = [(AmaxB*CO2AMaxBEff) - AmaxB]/[550 - 350]
            float AmaxB_int = (float)(-1.0 * (((spc.CO2AMaxBEff - 1.0) * 1.75) - 1.0) * spc.AmaxB);  // Derived from b = AmaxB - (AmaxB_slope * 350)
            float AmaxB_CO2 = (float)(AmaxB_slope * monthlyClimateRecord.CO2 + AmaxB_int);
            speciespnetvars.AmaxB_CO2 = AmaxB_CO2;

            // FTempPSN: reduction factor due to temperature (public for output file)
            if (dTemp)
            {
                speciespnetvars.FTempPSN = EcoregionPnETVariables.DTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin, spc.PsnTMax);
            }
            else
            {
                //speciespnetvars.FTempPSN = EcoregionPnETVariables.LinearPsnTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin); // Original PnET-Succession
                speciespnetvars.FTempPSN = EcoregionPnETVariables.CurvelinearPsnTempResponse(Tday, spc.PsnTOpt, spc.PsnTMin, spc.PsnTMax); // Modified 051216(BRM)
            }

            // Daytime maintenance respiration factor (scaling factor of actual vs potential respiration applied to daily temperature)
            float fTempRespDay = EcoregionPnETVariables.CalcQ10Factor(spc.Q10, Tday, spc.PsnTOpt);

            // Night maintenance respiration factor (scaling factor of actual vs potential respiration applied to night temperature)
            float fTempRespNight = EcoregionPnETVariables.CalcQ10Factor(spc.Q10, Tmin, spc.PsnTOpt);

            // Unitless respiration adjustment based on temperature: public for output file only
            float FTempRespWeightedDayAndNight = (float)Math.Min(1.0, (fTempRespDay * daylength + fTempRespNight * nightlength) / ((float)daylength + (float)nightlength)); ;
            speciespnetvars.FTempRespWeightedDayAndNight = FTempRespWeightedDayAndNight;
            // Scaling factor of respiration given day and night temperature and day and night length
            speciespnetvars.MaintRespFTempResp = spc.MaintResp * FTempRespWeightedDayAndNight;

            // Respiration gC/timestep (RespTempResponses[0] = day respiration factor)
            // Respiration acclimation subroutine From: Tjoelker, M.G., Oleksyn, J., Reich, P.B. 1999.
            // Acclimation of respiration to temperature and C02 in seedlings of boreal tree species
            // in relation to plant size and relative growth rate. Global Change Biology. 49:679-691,
            // and Tjoelker, M.G., Oleksyn, J., Reich, P.B. 2001. Modeling respiration of vegetation:
            // evidence for a general temperature-dependent Q10. Global Change Biology. 7:223-230.
            // This set of algorithms resets the veg parameter "BaseFolRespFrac" from
            // the static vegetation parameter, then recalculates BaseFolResp based on the adjusted
            // BaseFolRespFrac

            // Base foliage respiration 
            float BaseFolRespFrac;

            // Base parameter in Q10 temperature dependency calculation
            float Q10base;
            if (wythers == true)
            {
                //Computed Base foliar respiration based on temp; this is species-level, so you can compute outside this IF block and use for all cohorts of a species
                BaseFolRespFrac = (0.138071F - 0.0024519F * Tave);

                //Midpoint between Tave and Optimal Temp; this is also species-level
                float Tmidpoint = (Tave + spc.PsnTOpt) / 2F;

                // Base parameter in Q10 temperature dependency calculation in current temperature
                Q10base = (3.22F - 0.046F * Tmidpoint);
            }
            else
            {
                // The default PnET setting is that these 
                BaseFolRespFrac = spc.BFolResp;
                Q10base = spc.Q10;
            }
            speciespnetvars.BaseFolRespFrac = BaseFolRespFrac;

            // Respiration Q10 factor
            speciespnetvars.Q10Factor = EcoregionPnETVariables.CalcQ10Factor(Q10base, Tave, spc.PsnTOpt);

            return speciespnetvars;
        }

        #endregion

    }
}
