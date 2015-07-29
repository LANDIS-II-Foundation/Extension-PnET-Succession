using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    static class CanopyLayerCategories
    {
        static string filename;
        static List<int> maxages = new List<int>();
        static List<float[]> CumBiomassFractionsPerLayer = new List<float[]>();
        
        static public List<int> MaxAges
        {
            get
            {
                return maxages;
            }
        }

        static void MakeSureFileContains(string[] FileContent, string KeyWord)
        {
            for (int l = 0; l < FileContent.Count(); l++)
            {
                if (FileContent[l].Contains(KeyWord)) return;
            }
            throw new System.Exception("Expecting keyword " + KeyWord + " in filename " + filename);
        }
        static int GetLineNrThatContains(string[] FileContent, string KeyWord)
        {
            MakeSureFileContains(FileContent,KeyWord);

            for (int l = 0; l < FileContent.Count(); l++)
            {
                if (FileContent[l].Contains(KeyWord)) return l;
            }

            throw new System.Exception("Expecting keyword " + KeyWord + " in filename " + filename);
        }
        static private string[] RemoveOutcommentedLines(string[] content)
        {
            List<string> newcontent = new List<string>();

            for (int l = 0; l < content.Count(); l++)
            {
                string line = content[l].Trim();
                int i = line.IndexOf(">>");
                if (i != 0) newcontent.Add(content[l]);
            }
            return newcontent.ToArray();
        }


        static public void InitializeCanopyLayers(string keyword, string FileName)
        {
            try
            {
                filename = FileName;

                string[] FileContent = RemoveOutcommentedLines(System.IO.File.ReadAllLines(FileName));

                GetLineNrThatContains(FileContent, InputParametersParser.Names.CanopyLayerBiomassCategories);

                
                int NrOfLayers = FileContent[GetLineNrThatContains(FileContent, "MaxCohortAge")].Split('\t').Count() - 1;

                int Layer=0;
                for (int l = 2; l < FileContent.Count(); l++)
                {
                    string line = FileContent[l].Trim();
                    if (line.Length == 0) continue;
                    while (line.Contains("\t\t")) line = line.Replace("\t\t", "\t");
                    if (line.Contains("<<")) line = line.Remove(line.IndexOf("<<"));

                    string[] terms = line.Split('\t');

                    int maxage;
                    if (terms[0].Contains("MAX")) maxage = int.MaxValue;
                    else maxage = int.Parse(terms[0]);

                    float[] Fractions = new float[NrOfLayers];
                    for (int t = 1; t < terms.Count(); t++)
                    {
                        Fractions[t - 1] = float.Parse(terms[t]);
                    }
                    MakeSureFractionsAddUpToOne(Fractions,Layer);
                    AddLayer(maxage, Fractions);
                    Layer++;
                }
            }
            catch(System.Exception e)
            {
                throw new System.Exception("Error reading "+ FileName +" " + e.Message);
            }

        }
        static void MakeSureFractionsAddUpToOne(float[] Fractions,  int layer)
        {
            float sum = 0;
            for (int t = 0; t < Fractions.Count(); t++)
            {
                sum+=Fractions[t];
            }
            if (Math.Abs(sum - 1) > 1e-6)
            {
                throw new System.Exception("Canopy Biomass Fractions in Layer " + layer + " should add up to 1.0");
            }
        }
        static public float[] GetCumBiomFractions(int maxage)
        {
            for (int layer = 0; layer < MaxAges.Count; layer++)
            {
                if (maxage < MaxAges[layer])
                {
                    return CumBiomassFractionsPerLayer[layer];
                }
            }
            throw new System.Exception("Cannot get layer definitions, function GetCumBiomFractions");
        }

        static private float[]  GetCumBiomassFraction(float[] BiomassFractions)
        {
            float[] CumBiomassFractionsPerLayer = new float[BiomassFractions.Count()];
            float sum = 0;
            for (int f = 0; f < BiomassFractions.Count(); f++)
            {
                sum += BiomassFractions[f];
                CumBiomassFractionsPerLayer[f] = sum;
            }
            return CumBiomassFractionsPerLayer;
        }
        private static void AddLayer(int MaxAge, float[] BiomassFractions)
        {
            maxages.Add(MaxAge);
            //biomassfractions.Add(BiomassFractions);
            CumBiomassFractionsPerLayer.Add(GetCumBiomassFraction(BiomassFractions));
        }

        
    }
}
