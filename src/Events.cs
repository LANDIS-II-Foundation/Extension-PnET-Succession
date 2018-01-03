using Landis.Core;
using Landis.Library.BiomassCohorts;

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
