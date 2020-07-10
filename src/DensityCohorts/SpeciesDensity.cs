using Landis.Core;
using System.Collections.Generic;
using System.Linq;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class SpeciesDensity : ISpeciesDensity
    {
        static List<Tuple<ISpecies, ISpeciesDensity>> SpeciesCombinations;

        public List<ISpeciesDensity> AllSpecies 
         { 
             get
             { 
                 return SpeciesCombinations.Select(combination => combination.Item2).ToList(); 
             } 
         } 
 
 
         public ISpeciesDensity this[ISpecies species] 
         { 
             get
             { 
                 return SpeciesCombinations.Where(spc => spc.Item1 == species).First().Item2; 
             } 
         } 
         public ISpecies this[ISpeciesDensity species] 
         { 
             get
             { 
                 return SpeciesCombinations.Where(spc => spc.Item2 == species).First().Item1; 
             } 
         }


        #region private variables

        // standard LANDIS-II species parameters
        private string name;
        private int index;
        private int maxSproutAge;
        private int minSproutAge;
        private Landis.Core.PostFireRegeneration postfireregeneration;
        private int maxSeedDist;
        private int effectiveSeedDist;
        private float vegReprodProb;
        private byte fireTolerance;
        private byte shadeTolerance;
        int maturity;
        int longevity;

        // Density succession species parameters
        private int _sptype;
        private int _biomassclass;
        private float _maxdia;
        private int _maxsdi;
        private int _totalseed;
        private float _carboncoef;

        # endregion


        #region private static species variables

        // Density succession species parameters
        private static Landis.Library.Parameters.Species.AuxParm<int> sptype;
        private static Landis.Library.Parameters.Species.AuxParm<int> biomassclass;
        private static Landis.Library.Parameters.Species.AuxParm<float> maxdia;
        private static Landis.Library.Parameters.Species.AuxParm<int> maxsdi;
        private static Landis.Library.Parameters.Species.AuxParm<int> totalseed;
        private static Landis.Library.Parameters.Species.AuxParm<float> carboncoef;

        #endregion

        public SpeciesDensity()
        {
            #region initialization of private static species variables
            // Density succession species paramters
            sptype = ((Landis.Library.Parameters.Species.AuxParm<int>)(Parameter<int>)SpeciesParameters.GetParameter("sptype"));
            biomassclass = ((Landis.Library.Parameters.Species.AuxParm<int>)(Parameter<int>)SpeciesParameters.GetParameter("biomassclass"));
            maxdia = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)SpeciesParameters.GetParameter("maxdia"));
            maxsdi = ((Landis.Library.Parameters.Species.AuxParm<int>)(Parameter<int>)SpeciesParameters.GetParameter("maxsdi"));
            totalseed = ((Landis.Library.Parameters.Species.AuxParm<int>)(Parameter<int>)SpeciesParameters.GetParameter("totalseed"));
            carboncoef = ((Landis.Library.Parameters.Species.AuxParm<float>)(Parameter<float>)SpeciesParameters.GetParameter("carboncoef"));
            #endregion

            SpeciesCombinations = new List<Tuple<ISpecies, ISpeciesDensity>>();
             
            foreach (ISpecies spc in EcoregionData.ModelCore.Species)
            {
                SpeciesDensity species = new SpeciesDensity(spc);

                SpeciesCombinations.Add(new Tuple<ISpecies, ISpeciesDensity>(spc, species));
            }


        }


        SpeciesDensity(PostFireRegeneration postFireGeneration,
            int Index,
            string name,
            int maxSproutAge,
            int minSproutAge,
            int maxSeedDist,
            int effectiveSeedDist,
            float vegReprodProb,
            byte fireTolerance,
            byte shadeTolerance,
            int maturity,
            int longevity
            )
        {
            this.postfireregeneration = postFireGeneration;
            this.index = Index;
            this.name = name;
            this.maxSproutAge = maxSproutAge;
            this.minSproutAge = minSproutAge;
            this.postfireregeneration = postFireGeneration;
            this.maxSeedDist = maxSeedDist;
            this.effectiveSeedDist = effectiveSeedDist;
            this.vegReprodProb = vegReprodProb;
            this.fireTolerance = fireTolerance;
            this.shadeTolerance = shadeTolerance;
            this.maturity = maturity;
            this.longevity = longevity;
        }
       
        private SpeciesDensity(ISpecies species)
        {
            // Density succession species parameters
            _sptype = sptype[species];
            _biomassclass = biomassclass[species];
            _maxdia = maxdia[species];
            _maxsdi = maxsdi[species];
            _totalseed = totalseed[species];
            _carboncoef = carboncoef[species];
            index = species.Index;
            name = species.Name;
            maxSproutAge = species.MaxSproutAge;
            minSproutAge = species.MinSproutAge;
            postfireregeneration = species.PostFireRegeneration;
            maxSeedDist = species.MaxSeedDist;
            effectiveSeedDist = species.EffectiveSeedDist;
            vegReprodProb = species.VegReprodProb;
            fireTolerance = species.FireTolerance;
            shadeTolerance = species.ShadeTolerance;
            maturity = species.Maturity;
            longevity = species.Longevity;

        }
        

        #region Accessors

        // Density succession species paramters
        public int SpType
        {
            get
            {
                return _sptype;
            }
        }

        public int BiomassClass
        {
            get
            {
                return _biomassclass;
            }
        }

        public float MaxDia
        {
            get
            {
                return _maxdia;
            }
        }

        public int MaxSDI
        {
            get
            {
                return _maxsdi;
            }
        }

        public int TotalSeed
        {
            get
            {
                return _totalseed;
            }
        }

        public float CarbonCoef
        {
            get
            {
                return _carboncoef;
            }
        }

        public int Index
        {
            get
            {
                return index;
            }
        }
        public string Name
        {
            get
            {
                return name;
            }
        }

     
        public int MaxSproutAge
        {
            get
            {
                return maxSproutAge;
            }
        }
        public int MinSproutAge
        {
            get
            {
                return  minSproutAge;
            }
        }

        public Landis.Core.PostFireRegeneration PostFireRegeneration
        {
            get
            {
                return postfireregeneration;

            }
        }
        
        public int MaxSeedDist
        {
            get
            {
                return maxSeedDist;
            }
        }
        public int EffectiveSeedDist
        {
            get
            {
                return effectiveSeedDist;
            }
        }
       
        public float VegReprodProb
        {
            get
            {
                return vegReprodProb;
            }
        }
        public byte FireTolerance
        {
            get
            {
                return fireTolerance;
            }
        }
        public byte ShadeTolerance
        {
            get
            {
                return shadeTolerance;
            }
        }
        public int Maturity
        {
            get
            {
                return maturity;
            }
        }
        public int Longevity
        {
            get
            {
                return longevity;
            }
        }
        #endregion

        public static List<string> ParameterNames
        {
            get
            {
                System.Type type = typeof(SpeciesDensity); // Get type pointer
                List<string> names = type.GetProperties().Select(x => x.Name).ToList(); // Obtain all fields


                return names;
            }
        }

    }

    class Tuple<T1, T2>
    {
        public T1 Item1 { get; set; }
        public T2 Item2 { get; set; }

        public Tuple(T1 Item1, T2 Item2)
        {
            this.Item1 = Item1;
            this.Item2 = Item2;

        }

    }
}
