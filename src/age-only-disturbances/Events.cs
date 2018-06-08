using Landis.Core;
using Landis.Library.BiomassCohorts;
using Landis.SpatialModeling;

namespace Landis.Extension.Succession.BiomassPnET.AgeOnlyDisturbances
{
    class Events
    {
        public static void CohortDied(object sender, DeathEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
        }
        //---------------------------------------------------------------------
        // Copied from Biomass Succession code. - not functional yet
        public static void SiteDisturbed(object sender,
                                         DisturbanceEventArgs eventArgs)
        {
            ExtensionType disturbanceType = eventArgs.DisturbanceType;
            PoolPercentages poolReductions = Module.Parameters.PoolReductions[disturbanceType];

            ActiveSite site = eventArgs.Site;
            PlugIn.WoodyDebris[site].ReduceMass(poolReductions.Wood);
            PlugIn.Litter[site].ReduceMass(poolReductions.Foliar);
        }
    }
}
