using System;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class ParameterList 
    {
        Dictionary<string, Landis.Library.Parameters.Species.AuxParm<string>> genericparameters = new Dictionary<string,Library.Parameters.Species.AuxParm<string>>();

        string Parameterlabels
        {
            get
            {
                string Parameterlabels = "";
                foreach (System.Collections.Generic.KeyValuePair<string, Landis.Library.Parameters.Species.AuxParm<string>> par in genericparameters)
                {
                    Parameterlabels += par.Key + "\t";
                }
                return Parameterlabels;
            }
        }

        public void Add(string key, Landis.Library.Parameters.Species.AuxParm<string> toadd)
        {
            genericparameters.Add(key.ToLower(), toadd);
        }


        public ParameterList Add(ParameterList toadd)
        {
            foreach (System.Collections.Generic.KeyValuePair<string, Landis.Library.Parameters.Species.AuxParm<string>> par in toadd.genericparameters)
            {
                genericparameters.Add(par.Key.ToLower(), par.Value);
            }
            return this;
        }

        public Landis.Library.Parameters.Species.AuxParm<string> this[string label]
        {
            get
            {

                return GetParameter(label.ToLower());
                 
                
            }
        }
        public Landis.Library.Parameters.Species.AuxParm<T> GetValues<T>(string label, T Min, T Max)
             where T: IComparable<T>   
        {
            Landis.Library.Parameters.Species.AuxParm<string> par = GetParameter(label);
            if (par == null) throw new System.Exception("Cannot find parameter " + label);

            Landis.Library.Parameters.Species.AuxParm<T> values = new Landis.Library.Parameters.Species.AuxParm<T>(PlugIn.modelCore.Species);
            foreach (ISpecies spc in PlugIn.modelCore.Species)
            {
                try
                {
                    values[spc] =  (T)Convert.ChangeType(par[spc], typeof(T));

                    if(Landis.Library.Parameters.InputValue_ExtensionMethods.GreaterThan<T>(values[spc],Max))
                    {
                        throw new System.Exception("Species parameter " + label + " with value " + values[spc] + " should be smaller than " + Max);
                    }
                    if (Landis.Library.Parameters.InputValue_ExtensionMethods.LessThan<T>(values[spc], Min))
                    {
                        throw new System.Exception("Species parameter " + label + " with value " + values[spc] + " should be larger than " + Min);
                    }

                }
                catch(System.Exception e)
                {
                    throw new System.Exception("Cannot retrieve species parameter " + label + " " + e.Message);
                }

                 
            }
            return values;
        }
        public Landis.Library.Parameters.Species.AuxParm<string> GetParameter(string label)
        {
            Landis.Library.Parameters.Species.AuxParm<string> result =null;

            if (genericparameters.TryGetValue(label.ToLower(), out result) == true) return result;

            else return null;
            
        }

        public void SetValue(string label, ISpecies spc, string value)
        {
            Landis.Library.Parameters.Species.AuxParm<string> Parameter = GetParameter(label);

            if (Parameter != null)
            {
                Parameter[spc] = value;
                return;
            }

            throw new System.Exception("Cannot get parameter " + label + " parameter names are " + Parameterlabels);
        }
        public void SetValue(string label, string value)
        {
            Landis.Library.Parameters.Species.AuxParm<string> Parameter =  GetParameter(label);


            if(Parameter != null)
            {
                foreach (ISpecies spc in PlugIn.modelCore.Species)
                {
                    Parameter[spc] = value;
                }
                return;
            }
 
            throw new System.Exception("Cannot get parameter " + label +" parameter names are "+ Parameterlabels);
        }
      
        public void PrintValuesToLogFile()
        {
            string Line;

            Line = "\tSpeciesParameters\t";
            foreach (System.Collections.Generic.KeyValuePair<string, Landis.Library.Parameters.Species.AuxParm<string>> parameter in genericparameters)
            {
                Line += parameter.Key + "\t";
            }
            PlugIn.ModelCore.UI.WriteLine(Line);
            foreach(ISpecies species in PlugIn.modelCore.Species)
            {
                Line = "\t" + species.Name + "\t";
                foreach (System.Collections.Generic.KeyValuePair<string, Landis.Library.Parameters.Species.AuxParm<string>> parameter in genericparameters)
                {
                    Line += parameter.Value[species] + "\t";
                }
                PlugIn.ModelCore.UI.WriteLine(Line);
            }
        }
         
    }
}
