using System;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class SpeciesParameters
    {
        List<object> parameters;

        private SpeciesParameter<T> GetPar<T>(string label)
            where T : IComparable<T>
        {
            foreach (object par in parameters)
            {
                SpeciesParameter<T> mypar = (SpeciesParameter<T>)par;
                if (string.Equals(mypar.Label, label, StringComparison.OrdinalIgnoreCase))
                {
                    return mypar;
                }
            }
            throw new System.Exception("Cannot get parameter " + label);
        }
        

        public void SetValue<T>(string label, ISpecies species, InputValue<T> value)
            where T : IComparable<T>
        {
            foreach (object par in parameters)
            {
                SpeciesParameter<T> mypar = (SpeciesParameter<T>)par;
                if (string.Equals(mypar.Label, label, StringComparison.OrdinalIgnoreCase))
                {
                    mypar.SetValue(label, species, value);
                    return;
                }
            }
            throw new System.Exception("Cannot get parameter " + label);
        }


        public Landis.Library.Parameters.Species.AuxParm<T> GetParameter<T>(string label)
            where T : IComparable<T>
        {
            foreach (object par in parameters)
            {
                return GetPar<T>(label).Values;
            }
            throw new System.Exception("Cannot get parameter "+ label);
        }
        public void PrintValuesToLogFile()
        {
            string Line;

            Line = "\tSpeciesParameters\t";
            foreach (SpeciesParameter<float> parameter in parameters)
            {
                Line+=parameter.Label +"\t";
            }
            PlugIn.ModelCore.UI.WriteLine(Line);
            foreach(ISpecies species in PlugIn.modelCore.Species)
            {
                Line = "\t" + species.Name + "\t";
                foreach (SpeciesParameter<float> parameter in parameters)
                {
                    Line += parameter.Values[species] + "\t";
                }
                PlugIn.ModelCore.UI.WriteLine(Line);
            }
        }
        public SpeciesParameters()
        {
            parameters = new List<object>();
                
            parameters.Add(new SpeciesParameter<float>(0,float.MaxValue,0.1F, "slwdel"));
            parameters.Add(new SpeciesParameter<float>(0,float.MaxValue, 0.01F, "towood"));
            parameters.Add(new SpeciesParameter<float>(0,1,0.6F,"folret"));
            parameters.Add(new SpeciesParameter<float>(0, 1000, 0.25F ,"tofol"));  
            parameters.Add(new SpeciesParameter<float>(0, 2000, 220, "gddfolst"));
            parameters.Add(new SpeciesParameter<float>(0, 1000, 0.01F, "toroot"));
            parameters.Add(new SpeciesParameter<float>(0,float.MaxValue,200F ,"slwmax"));
            parameters.Add(new SpeciesParameter<float>(0, 5000,700,"cddfolend"));
            parameters.Add(new SpeciesParameter<float>(0, float.MaxValue,6.0F, "wuecnst"));
            parameters.Add(new SpeciesParameter<float>(0,5,2.0F,"q10"));
            parameters.Add(new SpeciesParameter<float>(-10, 10,1.5F,"psntmin"));
            parameters.Add(new SpeciesParameter<float>(0,float.MaxValue,200F, "halfsat"));
            parameters.Add(new SpeciesParameter<float>(0,float.MaxValue,20F, "EstRad"));
            parameters.Add(new SpeciesParameter<float>(0, float.MaxValue,20F, "EstMoist"));
            parameters.Add(new SpeciesParameter<float>(0, 40, 22F, "psntopt"));
            parameters.Add(new SpeciesParameter<float>(0,10, 2.2F, "foln")); 
            parameters.Add(new SpeciesParameter<float>(0,float.MaxValue,0.1F,  "bfolresp")); 
            parameters.Add(new SpeciesParameter<float>(0,float.MaxValue, 0.2F, "follignin")); 
            parameters.Add(new SpeciesParameter<float>(0, 1,0.1F, "kwdlit"));
            parameters.Add(new SpeciesParameter<float>(0.1F, float.MaxValue,1.3F, "grMstSens")); 
            parameters.Add(new SpeciesParameter<float>(0,5,2.0F, "dvpd2")); 
            parameters.Add(new SpeciesParameter<float>(0,5,0.05F, "dvpd1")); 
            parameters.Add(new SpeciesParameter<float>(-500, 500,-25F, "amaxa")); 
            parameters.Add(new SpeciesParameter<float>(0, float.MaxValue,50F, "amaxb")); 
            parameters.Add(new SpeciesParameter<float>(0,float.MaxValue ,4F, "psnagered")); 
            parameters.Add(new SpeciesParameter<float>(0, float.MaxValue,0.5F, "k")); 
            parameters.Add(new SpeciesParameter<float>(0,10,0.06F, "dnsc")); 
            parameters.Add(new SpeciesParameter<float>(0,1,0.0009F, "maintresp")); 
            parameters.Add(new SpeciesParameter<float>(0,1,0.1F, "wltpnt")); 
            parameters.Add(new SpeciesParameter<float>(0,1,0.35F, "fracbelowg")); 
             
        }
    }
}
