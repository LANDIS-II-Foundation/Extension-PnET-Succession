using Landis.Core;
using System;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class PressureHeadVanGenuchten : IPressureHeadTable
    {
        public const string VanGenuchtenParameters = "VanGenuchtenParameters";

        public static List<string> ParameterNames
        {
            get
            {
                return typeof(PressureHeadVanGenuchten).GetFields().Select(x => x.Name).ToList();
            }
        }
        public float Porosity(float RootingDepth, string soiltype)
        {
            return RootingDepth * thetas[soiltype];
        }
        public int WiltingPoint(string SoilType)
        {
            return (int)CalculateWaterContent((ushort)153, SoilType);
        }
        public int FieldCapacity(string SoilType)
        {
            return (int)CalculateWaterContent((ushort)3.36F, SoilType);
        }
        Landis.Library.Parameters.Ecoregions.AuxParm<ushort[]> table = new Library.Parameters.Ecoregions.AuxParm<ushort[]>(PlugIn.ModelCore.Ecoregions);
        public static Parameter<float> thetar;
        public static Parameter<float> thetas;
        public static Parameter<float> alpha;
        public static Parameter<float> n;

        /// <summary>
        ///  Calculate water pressure in meter
        /// </summary>
        public ushort CalculateWaterPressure(double VolWaterContent, string soiltype)
        {
            //string soiltype = SoilType[ecoregion];
            // VolWaterContent water content (m3H2O/m3 SOIL)

            // effective degree of saturation (-)
            double Se = Math.Min(1, Math.Max(0, (VolWaterContent - thetar[soiltype]) / (thetas[soiltype] - thetar[soiltype])));

            // pressure head (m)
            double a = Math.Pow(Se, (-1 / (1 - 1.0 / n[soiltype])));
            double hm = Math.Pow((a - 1), (1 / n[soiltype])) / alpha[soiltype];

            // Water pressure in m
            return (ushort)Math.Min(int.MaxValue, Math.Max(hm, -int.MaxValue));
        }
        public float CalculateWaterContent(ushort WaterPressure /* meter pressure head*/, string soiltype)
        {
            // thetas: pore space
            // thetar: residual water content
            //string soiltype = SoilType[ecoregion];
            float SE = (float)(1.0 / Math.Pow((Math.Pow((WaterPressure * alpha[soiltype]), n[soiltype]) + 1), (1 - 1.0 / n[soiltype])));
            float VolWaterContent = (float)(SE * (thetas[soiltype] - thetar[soiltype]) + thetar[soiltype]);
            return VolWaterContent;
        }
        public ushort this[IEcoregion ecoregion, int water]
        {
            get
            {
                if (water >= table[ecoregion].Length) return 0;
                return table[ecoregion][water];
            }
        }
        public PressureHeadVanGenuchten()
        {
            Landis.Library.Parameters.Ecoregions.AuxParm<float> RootingDepth = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter(Names.RootingDepth, 0, float.MaxValue);

            table = new Library.Parameters.Ecoregions.AuxParm<ushort[]>(PlugIn.ModelCore.Ecoregions);

            Landis.Library.Parameters.Ecoregions.AuxParm<string> SoilType = (Landis.Library.Parameters.Ecoregions.AuxParm<string>)PlugIn.GetParameter(Names.SoilType);

            thetar = (Parameter<float>)PlugIn.GetParameter("thetar");
            thetas = (Parameter<float>)PlugIn.GetParameter("thetas");
            alpha = (Parameter<float>)PlugIn.GetParameter("alpha");
            n = (Parameter<float>)PlugIn.GetParameter("n"); 
            
            foreach (IEcoregion ecoregion in PlugIn.ModelCore.Ecoregions) if (ecoregion.Active)
            {
                List<ushort> PressureHead = new List<ushort>();
                    
                int watercontent = 0;

                ushort ph = ushort.MaxValue;
                while(ph > 0)
                {
                    ph = CalculateWaterPressure(watercontent / RootingDepth[ecoregion], SoilType[ecoregion]);
                    //System.Console.WriteLine(watercontent + "\t" + ph);
                    PressureHead.Add(ph);
                    watercontent++;
                }
                table[ecoregion] = PressureHead.ToArray();
            }

        }

    }
}
