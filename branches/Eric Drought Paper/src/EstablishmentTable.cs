using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public static class EstablishmentTable
    {
        static string keyword;
        static string filename;
        static int[] CategoryMin;
        static float[] CategoryValues;

        static void MakeSureValuesAreFractions(float[] values)
        {
            foreach (int v in values)
            {
                if (v > 1 || v<0) throw new System.Exception("Unexpected probability value in " + filename + "\t" + v);
            }
        }
        static void MakeSureCategoriesDescend(int[] values)
        {
            int v_last = int.MaxValue;
            foreach (int v in values)
            {
                if(v > v_last) throw new System.Exception("Unexpected probability value in " + filename + "\t" + v);
                v_last = v; 
                
            }
        }
        static void MakeSureCategoriesDescend(float[] values)
        {
            float v_last = int.MaxValue;
            foreach (float v in values)
            {
                if (v > v_last) throw new System.Exception("Unexpected probability value in " + filename + "\t" + v +" values should descend");
                v_last = v;

            }
        }
        static void MakeSureFileContains(string term)
        {
            List<string> content = new List<string>(System.IO.File.ReadAllLines(filename));
            for (int l = 0; l < content.Count; l++)
            {
                if (content[l].Contains(term))
                {
                    return;
                }
            }
            throw new System.Exception("Expected " + term + " in " + filename);
        
        }
        public static float[] ReadEstablishmentTable(string Keyword, string Filename, ISpecies species)
        {
            keyword = Keyword;
            filename = Filename;
            string line="";
            try
            {
                List<string> content = new List<string>(System.IO.File.ReadAllLines(filename));

                MakeSureFileContains(Keyword);
                MakeSureFileContains("CategoryMin");
                MakeSureFileContains(species.Name);

                for (int l = 0; l < content.Count; l++)
                {
                    line = content[l];
                    if (line.Contains("<<"))
                    {
                        line= line.Remove(line.IndexOf("<<"));
                    }

                    if (line.Contains("CategoryMin"))
                    {
                        
                        string[] terms = line.Trim().Split('\t');
                        CategoryMin = new int[terms.Count() - 1];
                        CategoryValues = new float[terms.Count() - 1];
                        for (int t = 1; t < terms.Count(); t++)
                        {
                            CategoryMin[t - 1] = (int)(100 * float.Parse(terms[t]));
                        }
                    }
                    if (line.Contains(species.Name))
                    {
                        string[] terms = line.Trim().Split('\t');
                        for (int t = 1; t < terms.Count(); t++)
                        {
                            if (float.TryParse(terms[t],out CategoryValues[t - 1])==false)
                            {
                                throw new System.Exception("Irregular value in "+ filename);
                            }
                             
                        }
                    }
                }
                
                MakeSureValuesAreFractions(CategoryValues);
                MakeSureCategoriesDescend(CategoryValues);
                MakeSureCategoriesDescend(CategoryMin);

                return GetProbabilityArray(CategoryMin, CategoryValues);
            }
            catch(System.Exception e)
            {
                throw new System.Exception("Cannot read "+ filename + " " + e.Message +" "+ line);
            }
        }
        public static float[] InitProbabilityArray()
        {
            int MaxValue = 100;
            float[] probability = new float[MaxValue + 1];
            for (int c = MaxValue; c >= 0; c--)
            {
                probability[c] = 1;
            }
            return probability;
        }
        static private float[] GetProbabilityArray(int[] CategoryMin, float[] CategoryValues)
        {
            float[] probability = InitProbabilityArray();
            try
            {
                for (int c = probability.Count()-1; c >= 0; c--)
                {
                    for (int cat = 0; cat < CategoryMin.Count(); cat++)
                    {
                        if (c >= CategoryMin[cat])
                        {
                            probability[c] = CategoryValues[cat];
                            break;
                        }
                    }
                }
                return probability;
            }
            catch(System.Exception e)
            {
                throw new System.Exception("Cannot creat probability arrays "+ e.Message);
            }
        
        }
        static public float GetProbability(float EnvironmentalVariable)
        {
            for (int v = 0; v < CategoryMin.Count(); v++)
            {
                if (EnvironmentalVariable > CategoryMin[v])
                {
                    return CategoryValues[v];
                }
            }
            return 0;
        }
    }
}
