//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Arjan de Bruijn

using Landis.Library.Succession;
using Edu.Wisc.Forest.Flel.Util;
using Landis.SpatialModeling;
using System.Collections.Generic;
using System;
using Landis.Core;
namespace Landis.Extension.Succession.BiomassPnET
{

    /// <summary>
    /// The parameters for biomass succession.
    /// </summary>
    public class InputParameters 
    
    {
        //public Dictionary<string, Landis.Library.Parameters.Species.AuxParm<string>> Speciesparameters = new Dictionary<string, Library.Parameters.Species.AuxParm<string>>(StringComparer.InvariantCultureIgnoreCase);
        public Dictionary<string, Parameter> Parameters = new Dictionary<string, Parameter>(StringComparer.InvariantCultureIgnoreCase);
         
       // public static ISiteVar<bool> HasSiteOutput;
        public static string InitialCommunitiesMap { get; set; }
        public static string OutputSitesFile { get; set; }
        public static int StartYear { get; set; }
        public static SeedingAlgorithms SeedAlgorithm { get; set; }
        public static string InitialCommunities { get; set; }
        public static int Timestep { get; set; }

        public static float MaxDevLyrAv { get; set; }
        public static int MaxCanopyLayers { get; set; }
        

        public static int IMAX { get; set; }
        public static double Latitude { get; set; }

        public string AgeOnlyDisturbancesParameterFile { get; set; }

        public string PnETEcoregionsParameterFile { get; set; }
        public string PnETGenericParameterFile { get; set; }
        public string VanGenughtenParameterFile { get; set; }
        
        public string PnETSpeciesParameterFile { get; set; }

        /*
        public Landis.Library.Parameters.Ecoregions.AuxParm<string> GetParameter(string label)
        {
            Parameter MyParameter = null;

            if (EcoregionParameters.TryGetValue(label, out MyParameter) == false)
            {
                throw new System.Exception("Cannot find a value for ecoregion parameter " + label + " parameter values should be given in " + PnETEcoregionsParameterFile);
            }

            Landis.Library.Parameters.Species.AuxParm<string> values = (Landis.Library.Parameters.Species.AuxParm<string>)MyParameter;

            return values;
        }



        public object GetParameter<T>(string label, T Min, T Max)
            where T : IComparable<T>
        {
            object par = null;

            if (EcoregionParameters.TryGetValue(label, out par))
            {
                 
                return par;
            }
            else throw new System.Exception("Cannot find a value for ecoregion parameter " + label + " parameter values should be given in " + PnETEcoregionsParameterFile + " or " + PnETGenericParameterFile);
           
             
        }
         */
         /*
        public Landis.Library.Parameters.Ecoregions.AuxParm<T> GetEcoregionParameterValues<T>(string label, T Min, T Max) where T : IComparable<T>
        {
            object Parameter = null;
            if (EcoregionParameters.TryGetValue(label, out Parameter) == false)
            {
                throw new System.Exception("Cannot find a value for ecoregion parameter " + label + " parameter values should be given in " + PnETEcoregionsParameterFile +" or " + PnETGenericParameterFile);
            }

            Landis.Library.Parameters.Ecoregions.AuxParm<T> values = new Landis.Library.Parameters.Ecoregions.AuxParm<T>(PlugIn.ModelCore.Ecoregions);
            foreach (IEcoregion eco in PlugIn.ModelCore.Ecoregions)if(eco.Active)
            {
                try
                {
                    values[eco] = (T)Convert.ChangeType(Parameter[eco], typeof(T));

                    if (Max  != null) if (Landis.Library.Parameters.InputValue_ExtensionMethods.GreaterThan<T>(values[eco], Max))
                    {
                        throw new System.Exception("Species parameter " + label + " with value " + values[eco] + " should be smaller than " + Max);
                    }
                    if (Min != null) if (Landis.Library.Parameters.InputValue_ExtensionMethods.LessThan<T>(values[eco], Min))
                    {
                        throw new System.Exception("Species parameter " + label + " with value " + values[eco] + " should be larger than " + Min);
                    }

                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Cannot retrieve species parameter " + label + " " + e.Message);
                }


            }
            return values;
        }
         */
        /*
        public Landis.Library.Parameters.Species.AuxParm<T> GetSpeciesParameterValues<T>(string label, T Min, T Max)
            where T : IComparable<T>
        {
            Landis.Library.Parameters.Species.AuxParm<string> Parameter = null;
            if (Speciesparameters.TryGetValue(label, out Parameter) == false)
            {
                throw new System.Exception("Cannot find a value for species parameter " + label + " parameter values should be given in " + PnETSpeciesParameterFile + " or " + PnETGenericParameterFile);
            }

            Landis.Library.Parameters.Species.AuxParm<T> values = new Landis.Library.Parameters.Species.AuxParm<T>(PlugIn.ModelCore.Species);
            foreach (ISpecies spc in PlugIn.ModelCore.Species)
            {
                try
                {
                    values[spc] = (T)Convert.ChangeType(Parameter[spc], typeof(T));

                    if (Landis.Library.Parameters.InputValue_ExtensionMethods.GreaterThan<T>(values[spc], Max))
                    {
                        throw new System.Exception("Species parameter " + label + " with value " + values[spc] + " should be smaller than " + Max);
                    }
                    if (Landis.Library.Parameters.InputValue_ExtensionMethods.LessThan<T>(values[spc], Min))
                    {
                        throw new System.Exception("Species parameter " + label + " with value " + values[spc] + " should be larger than " + Min);
                    }

                }
                catch (System.Exception e)
                {
                    throw new System.Exception("Cannot retrieve species parameter " + label + " " + e.Message);
                }


            }
            return values;
        }
        */

        
        
        

    }
}
