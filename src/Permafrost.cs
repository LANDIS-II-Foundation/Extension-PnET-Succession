using System;
using System.Collections.Generic;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    class Permafrost
    {

        public static SortedList<float, float> CalculateMonthlySoilTemps(SortedList<float, float> depthTempDict,IEcoregionPnET Ecoregion, int daysOfWinter, float snowPack, IHydrology hydrology, float lastTempBelowSnow)
        {
            //SortedList<float, float> depthTempDict = new SortedList<float, float>();  //for permafrost



            //float lambAir = 0.023f;
            //float lambIce = 2.29f;
            //float omega = (float)(2 * Math.PI / 12.0);


            float[] snowResults = CalculateSnowDepth(daysOfWinter, snowPack);
            float sno_dep = snowResults[0];
            float Psno_kg_m3 = snowResults[1];
            if (Ecoregion.Variables.Tave >= 0)
            {
                float fracAbove0 = Ecoregion.Variables.Tmax / (Ecoregion.Variables.Tmax - Ecoregion.Variables.Tmin);
                sno_dep = sno_dep * fracAbove0;
            }
            // from CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
            // Eq. 85 - Jordan (1991)
            //float lambda_Snow = (float)(lambAir + ((0.0000775 * Psno_kg_m3) + (0.000001105 * Math.Pow(Psno_kg_m3, 2))) * (lambIce - lambAir)) * 3.6F * 24F; //(kJ/m/d/K) includes unit conversion from W to kJ
            //float damping = (float)Math.Sqrt(omega / (2.0F * lambda_Snow));
            float damping = CalculateSnowDamping(Psno_kg_m3);
            float DRz_snow = (float)Math.Exp(-1.0F * sno_dep * damping); // Damping ratio for snow - adapted from Kang et al. (2000) and Liang et al. (2014)


            // Permafrost calculations - from "Soil thawing worksheet.xlsx"
            // 
            //if (Ecoregion.Variables.Tave < minMonthlyAvgTemp)
            //    minMonthlyAvgTemp = Ecoregion.Variables.Tave;
            float porosity = Ecoregion.Porosity / Ecoregion.RootingDepth;  //m3/m3
            float waterContent = hydrology.Water / Ecoregion.RootingDepth;  //m3/m3
            float ga = 0.035F + 0.298F * (waterContent / porosity);
            float Fa = ((2.0F / 3.0F) / (1.0F + ga * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))) + ((1.0F / 3.0F) / (1.0F + (1.0F - 2.0F * ga) * ((Constants.lambda_a / Constants.lambda_w) - 1.0F))); // ratio of air temp gradient
            float Fs = PressureHeadSaxton_Rawls.GetFs(Ecoregion.SoilType);
            float lambda_s = PressureHeadSaxton_Rawls.GetLambda_s(Ecoregion.SoilType);
            float lambda_theta = (Fs * (1.0F - porosity) * lambda_s + Fa * (porosity - waterContent) * Constants.lambda_a + waterContent * Constants.lambda_w) / (Fs * (1.0F - porosity) + Fa * (porosity - waterContent) + waterContent); //soil thermal conductivity (kJ/m/d/K)
            float D = lambda_theta / PressureHeadSaxton_Rawls.GetCTheta(Ecoregion.SoilType);  //m2/day
            float Dmonth = D * Ecoregion.Variables.DaySpan; // m2/month
            float ks = Dmonth * 1000000F / (Ecoregion.Variables.DaySpan * (Constants.SecondsPerHour * 24)); // mm2/s
            float d = (float)Math.Pow((Constants.omega / (2.0F * Dmonth)), (0.5));

            float maxDepth = Ecoregion.RootingDepth + Ecoregion.LeakageFrostDepth;
            float freezeDepth = maxDepth;
            float testDepth = 0;
            //if (lastTempBelowSnow == float.MaxValue)
            //{
            //    //int mCount = Math.Min(12, data.Count());
            //    //float tSum = 0;
            //    //foreach (int z in Enumerable.Range(0, mCount))
            //    //{
            //     //   tSum += data[z].Tave;
            //    //}
            //    //float annualTavg = tSum / mCount;
            //    float tempBelowSnow = Ecoregion.Variables.Tave;
            //    if (sno_dep > 0)
            //    {
            //        tempBelowSnow = annualTavg + (Ecoregion.Variables.Tave - annualTavg) * DRz_snow;
            //    }
            //    lastTempBelowSnow = tempBelowSnow;
            //    while (testDepth <= (maxDepth / 1000.0))
            //    {
            //        float DRz = (float)Math.Exp(-1.0F * testDepth * d); // adapted from Kang et al. (2000) and Liang et al. (2014)
            //        float zTemp = annualTavg + (tempBelowSnow - annualTavg) * DRz;
            //        depthTempDict[testDepth] = zTemp;
            //        if ((zTemp <= 0) && (testDepth < freezeDepth))
            //            freezeDepth = testDepth;
            //        testDepth += 0.25F;
            //    }
            //}
            //else
            //{
                float tempBelowSnow = Ecoregion.Variables.Tave;
                if (sno_dep > 0)
                {
                    tempBelowSnow = lastTempBelowSnow + (Ecoregion.Variables.Tave - lastTempBelowSnow) * DRz_snow;
                }
                lastTempBelowSnow = tempBelowSnow;
                while (testDepth <= (maxDepth / 1000.0))
                {
                    float DRz = (float)Math.Exp(-1.0F * testDepth * d); // adapted from Kang et al. (2000) and Liang et al. (2014)
                    float zTemp = depthTempDict[testDepth] + (tempBelowSnow - depthTempDict[testDepth]) * DRz;
                    depthTempDict[testDepth] = zTemp;
                    //if ((zTemp <= 0) && (testDepth < freezeDepth))
                    //    freezeDepth = testDepth;
                    testDepth += 0.25F;
                }
            //}
            return depthTempDict;
        }

        public static float[] CalculateSnowDepth(int daysOfWinter, float snowPack)
        {
            float bulkIntercept = 165.0f; //kg/m3
            float bulkSlope = 1.3f; //kg/m3
            float Pwater = 1000.0f;

            float Psno_kg_m3 = bulkIntercept + (bulkSlope * daysOfWinter); //kg/m3
            float Psno_g_cm3 = Psno_kg_m3 / 1000; //g/cm3

            float sno_dep = Pwater * snowPack / Psno_kg_m3 / 1000; //m
            float[] snowArray = new float[2];
            snowArray[0] = sno_dep;
            snowArray[1] = Psno_kg_m3;
            return snowArray;
        }

        public static float CalculateSnowDamping(float Psno_kg_m3)
        {
            float lambAir = 0.023f;
            float lambIce = 2.29f;
            float omega = (float)(2 * Math.PI / 12.0);

            // from CLM model - https://escomp.github.io/ctsm-docs/doc/build/html/tech_note/Soil_Snow_Temperatures/CLM50_Tech_Note_Soil_Snow_Temperatures.html#soil-and-snow-thermal-properties
            // Eq. 85 - Jordan (1991)
            float lambda_Snow = (float)(lambAir + ((0.0000775 * Psno_kg_m3) + (0.000001105 * Math.Pow(Psno_kg_m3, 2))) * (lambIce - lambAir)) * 3.6F * 24F; //(kJ/m/d/K) includes unit conversion from W to kJ
            float damping = (float)Math.Sqrt(omega / (2.0F * lambda_Snow));
            return damping;
        }
    }
}
