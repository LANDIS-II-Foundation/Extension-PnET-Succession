
using Landis.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Landis.Library.Climate;
using System.Net.NetworkInformation;
//using Landis.Extension.Succession.BiomassPnET;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class EcoregionData : IEcoregionData
    {
        #region private variables
        private Landis.Core.IEcoregion ecoregion;
        private float _latitude;
        #endregion

        #region private static variables
        private static bool wythers;
        private static bool dtemp;

        private static Dictionary<IEcoregion, IEcoregionData> AllEcoregions;
        private static Landis.Library.Parameters.Ecoregions.AuxParm<float> latitude;

        public static Landis.Library.Parameters.Ecoregions.AuxParm<double> GSO1;
        public static Landis.Library.Parameters.Ecoregions.AuxParm<double> GSO2;
        public static Landis.Library.Parameters.Ecoregions.AuxParm<double> GSO3;
        public static Landis.Library.Parameters.Ecoregions.AuxParm<double> GSO4;
        #endregion

        #region accessors for private static variables
        public static List<IEcoregionData> Ecoregions
        {
            get 
            {
                return AllEcoregions.Values.ToList();
            }
        }

        /// <summary>Returns the PnET Ecoregion for a given Landis Core Ecoregion</summary>
        /// <param name="landisCoreEcoregion"></param>
        public static IEcoregionData GetPnETEcoregion(IEcoregion landisCoreEcoregion)
        {
            return AllEcoregions[landisCoreEcoregion];
        }
        #endregion

        #region accessors for private variables

        
        public string Description
        {
            get
            {
                return ecoregion.Description;
            }
        }
        public bool Active
        {
            get
            {
                return ecoregion.Active;
            }
        }
        public ushort MapCode
        {
            get
            {
                return ecoregion.MapCode;
            }
        }
        public int Index
        {
            get
            {
                return ecoregion.Index;
            }
        }
        public string Name
        {
            get
            {
                return ecoregion.Name;
            }
        }
        public float Latitude
        {
            get
            {
                return _latitude;
            }
        }
        #endregion

        public static ICore ModelCore;

        public static List<string> ParameterNames
        {
            get
            {
                System.Type type = typeof(EcoregionData); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields
                names.Add("ClimateFileName");

                return names;
            }
        }

        public static void InitializeCore(ICore mCore)
        {
            ModelCore = mCore;
        }

        public static void Initialize()
        {
            latitude = (Landis.Library.Parameters.Ecoregions.AuxParm<float>)(Parameter<float>)GetParameter("Latitude", -90, 90);
            AllEcoregions = new Dictionary<IEcoregion, IEcoregionData>();
            foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
            {
                AllEcoregions.Add(ecoregion, new EcoregionData(ecoregion));
            }

        }

        public EcoregionData(Landis.Core.IEcoregion ecoregion)
        {
            this.ecoregion = ecoregion;
            this._latitude = latitude[ecoregion];
        }

        public static void EcoregionDynamicChange(IDynamicEcoregionRecord[] TimestepData)
        {

           //if (DynamicEcoregions.EcoRegData.ContainsKey(year))
            //{
                GSO1 = new Landis.Library.Parameters.Ecoregions.AuxParm<double>(ModelCore.Ecoregions);
                GSO2 = new Landis.Library.Parameters.Ecoregions.AuxParm<double>(ModelCore.Ecoregions);
                GSO3 = new Landis.Library.Parameters.Ecoregions.AuxParm<double>(ModelCore.Ecoregions);
                GSO4 = new Landis.Library.Parameters.Ecoregions.AuxParm<double>(ModelCore.Ecoregions);

                //DynamicEcoregions.TimestepData = DynamicEcoregions.EcoRegData[year];

                foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
                {
                    if (!ecoregion.Active)
                        continue;

                    if (TimestepData[ecoregion.Index] == null)
                        continue;

                    GSO1[ecoregion] = TimestepData[ecoregion.Index].GSO1;
                    GSO2[ecoregion] = TimestepData[ecoregion.Index].GSO2;
                    GSO3[ecoregion] = TimestepData[ecoregion.Index].GSO3;
                    GSO4[ecoregion] = TimestepData[ecoregion.Index].GSO4;
                }
            //}

        }

        public static void EcoregionDynamicChange(Landis.Extension.Succession.BiomassPnET.IDynamicEcoregionRecord[] TimestepData)
        {

            //if (DynamicEcoregions.EcoRegData.ContainsKey(year))
            //{
            GSO1 = new Landis.Library.Parameters.Ecoregions.AuxParm<double>(ModelCore.Ecoregions);
            GSO2 = new Landis.Library.Parameters.Ecoregions.AuxParm<double>(ModelCore.Ecoregions);
            GSO3 = new Landis.Library.Parameters.Ecoregions.AuxParm<double>(ModelCore.Ecoregions);
            GSO4 = new Landis.Library.Parameters.Ecoregions.AuxParm<double>(ModelCore.Ecoregions);

            //DynamicEcoregions.TimestepData = DynamicEcoregions.EcoRegData[year];

            foreach (IEcoregion ecoregion in ModelCore.Ecoregions)
            {
                if (!ecoregion.Active)
                    continue;

                if (TimestepData[ecoregion.Index] == null)
                    continue;

                GSO1[ecoregion] = TimestepData[ecoregion.Index].GSO1;
                GSO2[ecoregion] = TimestepData[ecoregion.Index].GSO2;
                GSO3[ecoregion] = TimestepData[ecoregion.Index].GSO3;
                GSO4[ecoregion] = TimestepData[ecoregion.Index].GSO4;
            }
            //}

        }


        public static bool TryGetParameter(string label, out Parameter<string> parameter)
        {
            parameter = null;
            if (label == null)
            {
                return false;
            }

            if (Names.parameters.ContainsKey(label) == false) return false;

            else
            {
                parameter = Names.parameters[label];
                return true;
            }
        }

        public static Parameter<string> GetParameter(string label)
        {
            if (Names.parameters.ContainsKey(label) == false)
            {
                throw new System.Exception("No value provided for parameter " + label);
            }

            return Names.parameters[label];

        }
        public static Parameter<string> GetParameter(string label, float min, float max)
        {
            if (Names.parameters.ContainsKey(label) == false)
            {
                throw new System.Exception("No value provided for parameter " + label);
            }

            Parameter<string> p = Names.parameters[label];

            foreach (KeyValuePair<string, string> value in p)
            {
                float f;
                if (float.TryParse(value.Value, out f) == false)
                {
                    throw new System.Exception("Unable to parse value " + value.Value + " for parameter " + label + " unexpected format.");
                }
                if (f > max || f < min)
                {
                    throw new System.Exception("Parameter value " + value.Value + " for parameter " + label + " is out of range. [" + min + "," + max + "]");
                }
            }
            return p;

        }
    }
}
 