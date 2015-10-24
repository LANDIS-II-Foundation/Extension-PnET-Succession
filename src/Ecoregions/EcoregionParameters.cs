using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
namespace Landis.Extension.Succession.BiomassPnET
{
    public class EcoregionParameters : IEcoregionParameters
    {
        private static Landis.Library.Parameters.Ecoregions.AuxParm<float> precintconst;
       private static Landis.Library.Parameters.Ecoregions.AuxParm<float> preclossfrac;
       private static Landis.Library.Parameters.Ecoregions.AuxParm<float> leakagefrac;
       private static Landis.Library.Parameters.Ecoregions.AuxParm<float> waterholdingcapacity;
        public static void Initialize()
        {
            precintconst = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PrecIntConst", 0, 1);
            preclossfrac = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("PrecLossFrac", 0, 1);
            leakagefrac = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("LeakageFrac", 0, 1);
            waterholdingcapacity = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)PlugIn.GetParameter("WaterHoldingCapacity", 0, 1);
        }

        private float _precintconst;
        private float _preclossfrac;
        private float _rootingdepth;
        private float _leakagefrac;
        private float _waterholdingcapacity;

        public float WaterHoldingCapacity
        {
            get
            {
                return _waterholdingcapacity;
            }
        }
        public float LeakageFrac
        {
            get
            {
                return _leakagefrac;
            }
        }
        public float RootingDepth
        {
            get
            {
                return _rootingdepth;
            }
        }
        public float PrecIntConst
        {
            get
            {
                return _precintconst;
            }
        }
        public float PrecLossFrac  
        {
            get
            {
                return _preclossfrac;
            }
        }
        public static List<string> ParameterNames
        {
            get
            {
                System.Type type = typeof(SpeciesParameters); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields


                return names;
            }
        }

        public EcoregionParameters(IEcoregion ecoregion)
        {
            this._precintconst = precintconst[ecoregion];
            this._preclossfrac = preclossfrac[ecoregion];
             
        }
        public EcoregionParameters(IEcoregionParameters  parameters)
        {
            this._precintconst = parameters.PrecIntConst;
            this._preclossfrac = parameters.PrecLossFrac;
            
        }
    }

}
