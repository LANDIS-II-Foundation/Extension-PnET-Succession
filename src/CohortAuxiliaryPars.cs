using System;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class CohortAuxiliaryPars
    {
        public float fage;
        public ushort index;
        public float FActiveBiom;

        public void Update(ushort age, ISpecies species, float biomass)
        {
            fage = Math.Max(0, 1 - (float)Math.Pow((age / (float)species.Longevity), species.PsnAgeRed()));
            FActiveBiom = (float)Math.Exp(-species.FrActWd() * biomass);
        }

        public CohortAuxiliaryPars()
        {
            
           
        }

    }
}
