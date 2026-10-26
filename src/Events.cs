using Landis.Core;
using Landis.Library.PnETCohorts;
using Landis.Library.UniversalCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.BiomassPnET.DisturbanceReductions
{
    class Events
    {
        public static void CohortDied(object sender, Library.UniversalCohorts.DeathEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
        }
    }
}
