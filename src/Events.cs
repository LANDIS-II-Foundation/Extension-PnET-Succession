using Landis.Core;
using Landis.Library.DensityCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.BiomassPnET.DisturbanceReductions
{
    class Events
    {
        //---------------------------------------------------------------------
        public static void CohortDied(object sender, DeathEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
        }
        //---------------------------------------------------------------------

    }
}
