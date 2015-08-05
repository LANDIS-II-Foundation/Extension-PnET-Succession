using Landis.SpatialModeling;
using Landis.Core;
using Landis.Library.BiomassCohortsPnET;
using System.Linq;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System;


namespace Landis.Extension.Succession.BiomassPnET
{
    public interface ICohortBiomass
    {  float LAI { get; }
        float AboveCanopyRadiation { get; }
        float Transpiration { get; }
         
        float ProductiveCanopyRatio { get; }
        float NetPsn { get; }
        ICohort Cohort { get; }
         
         
    }
}
