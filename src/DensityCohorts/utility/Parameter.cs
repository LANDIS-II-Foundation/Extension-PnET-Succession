using Landis.Core;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.DensityCohorts
{
    public class Parameter<T> : IEnumerable<KeyValuePair<string, T>>  
    {
        private Dictionary<string, T> values = new Dictionary<string, T>();
        string p_label;

        public List<T> Values
        {
            get
            {
                return new List<T>(values.Values);
            }
        }

        public Parameter(string name, T value, string label = null)
        {
            this.p_label = label;
            Add(name, value);
        }
        public Parameter(string label =null)
        {
            this.p_label = label;
        }
        public T Value
        {
            get
            {
                if (values.Count() != 1) throw new System.Exception(p_label + " is a multiple value parameter, but expecting a single value");
                return values.First().Value;
            }
        }
        public bool ContainsKey(string key)
        {
            return values.ContainsKey(key);
        }
        public T this[string label]
        {
            get
            {
                if (values.ContainsKey(label) == false)
                {
                    string msg = "No value for " + label;
                    if (p_label != null) msg = "Parameter " + p_label +" has no value for " + label;

                    throw new System.Exception(msg);
                }
                return values[label];
            }
            set
            {
                values[label] = value;
            }
        }
        
        public void Add(string key, T value)
        {
            values.Add(key, value);
        }
        public static explicit operator Parameter<bool>(Parameter<T> m)
        {
            try
            {
                Parameter<bool> P = new Parameter<bool>(m.p_label);

                foreach (KeyValuePair<string, T> i in m.values)
                {
                    P.Add(i.Key, bool.Parse(i.Value.ToString()));
                }
                return P;
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Cannot parse parameter " + m.p_label + " " + e.Message);
            }
        }
        public static explicit operator Parameter<float>(Parameter<T> m)
        {
            try
            {
                Parameter<float> P = new Parameter<float>(m.p_label);

                foreach (KeyValuePair<string, T> i in m.values)
                {
                    P.Add(i.Key,float.Parse(i.Value.ToString()));
                }
                return P;
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Cannot parse parameter "+ m.p_label +" "+e.Message);
            }
        }
        public static explicit operator Parameter<ushort>(Parameter<T> m)
        {
            try
            {
                Parameter<ushort> P = new Parameter<ushort>(m.p_label);

                foreach (KeyValuePair<string, T> i in m.values)
                {
                    P.Add(i.Key, ushort.Parse(i.Value.ToString()));
                }
                return P;
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Cannot parse parameter " + m.p_label + " " + e.Message);
            }
        }
        public static explicit operator Parameter<byte>(Parameter<T> m)
        {
            try
            {
                Parameter<byte> P = new Parameter<byte>(m.p_label);

                foreach (KeyValuePair<string, T> i in m.values)
                {
                    P.Add(i.Key, byte.Parse(i.Value.ToString()));
                }
                return P;
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Cannot parse parameter " + m.p_label + " " + e.Message);
            }
        }
        public static explicit operator Parameter<int>(Parameter<T> m)
        {
            try
            {
                Parameter<int> P = new Parameter<int>(m.p_label);

                foreach (KeyValuePair<string, T> i in m.values)
                {
                    P.Add(i.Key, ushort.Parse(i.Value.ToString()));
                }
                return P;
            }
            catch (System.Exception e)
            {
                throw new System.Exception("Cannot parse parameter " + m.p_label + " " + e.Message);
            }
        }
        
        
        public static explicit operator Landis.Library.Parameters.Ecoregions.AuxParm<T>(Parameter<T> m)
        {
            string ecoregionname = null;
            T value;
            try
            {
                Landis.Library.Parameters.Ecoregions.AuxParm<T> newvalues = new Landis.Library.Parameters.Ecoregions.AuxParm<T>(EcoregionData.ModelCore.Ecoregions);
                foreach (IEcoregion eco in EcoregionData.ModelCore.Ecoregions) if (eco.Active)
                {
                    ecoregionname = eco.Name;
                    if (m.values.Count == 1) value = m.values.First().Value;
                    else value = m.values[eco.Name];
                    newvalues[eco] = value;
                }
                return newvalues;
            }
            catch (System.Exception e)
            {
                if (e.GetType() == typeof(System.Collections.Generic.KeyNotFoundException))
                {
                    throw new System.Exception("Error parsing parameter " + m.p_label + " for species " + ecoregionname + " missing value");
                }
                else throw new System.Exception("Error parsing parameter " + m.p_label + " for species " + ecoregionname + e.Message);
            }
        
        }
        
        
        public static explicit operator Landis.Library.Parameters.Species.AuxParm<T>(Parameter<T> m)
        {
            T value;
            string speciesname = null;
            try
            {
                Landis.Library.Parameters.Species.AuxParm<T> newvalues = new Library.Parameters.Species.AuxParm<T>(EcoregionData.ModelCore.Species);

                foreach (ISpecies species in EcoregionData.ModelCore.Species)
                {
                    speciesname = species.Name;
                    if (m.values.Count == 1) value = m.values.First().Value;
                    else value = m.values[species.Name];
                    newvalues[species] = value;
                }

                return newvalues;
            }
            catch (System.Exception e)
            {
                if (e.GetType() == typeof(System.Collections.Generic.KeyNotFoundException))
                {
                    throw new System.Exception("Error parsing parameter " + m.p_label + " for species " + speciesname + " missing value");
                }
                else throw new System.Exception("Error parsing parameter " + m.p_label + " for species " + speciesname + e.Message);
            }
        }
        
        



        
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
       
        



    }
}
