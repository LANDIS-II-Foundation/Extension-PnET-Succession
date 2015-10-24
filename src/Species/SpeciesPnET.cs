
using Landis.Core;
 
namespace Landis.Extension.Succession.BiomassPnET 
{
    /// <summary>
    /// The information for a tree species (its index and parameters).
    /// </summary>
    public class SpeciesPnET
        : SpeciesParameters, ISpeciesPNET
    {
        private Landis.Core.ISpecies species;
         
        public int MaxSproutAge
        {
            get
            {
                return species.MaxSproutAge;
            }
        }
        public int MinSproutAge
        {
            get
            {
                return species.MinSproutAge;
            }
        }
        public Landis.Core.PostFireRegeneration PostFireRegeneration
        {
            get
            {
                return species.PostFireRegeneration;
            }
        }
        public int MaxSeedDist
        {
            get
            {
                return species.MaxSeedDist;
            }
        }
        public int EffectiveSeedDist
        {
            get
            {
                return species.EffectiveSeedDist;
            }
        }
        public float VegReprodProb
        {
            get
            {
                return species.VegReprodProb;
            }
        }
        public byte FireTolerance
        {
            get
            {
                return species.FireTolerance;
            }
        }
        public byte ShadeTolerance
        {
            get
            {
                return species.ShadeTolerance;
            }
        }
        public int Maturity
        {
            get
            {
                return species.Maturity;
            }
        }
        public int Longevity
        {
            get
            {
                return species.Longevity;
            }
        }
        public string Name
        {
            get {
                return species.Name;
            }
        }
        public int Index
        {
            get {
                return species.Index;
            }
        }

        //---------------------------------------------------------------------

        public SpeciesPnET(Landis.Core.ISpecies species, ISpeciesParameters parameters)
            : base(parameters)
        {
            this.species = species;
        }
    }
}
 