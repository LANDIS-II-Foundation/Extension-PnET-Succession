using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.BiomassCohorts;
using Landis.Core;

namespace Landis.Extension.Succession.BiomassPnET
{
    class Events
    {
        public static void CohortDied(object sender, DeathEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
        }
    }
}
