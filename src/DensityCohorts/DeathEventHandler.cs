namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// Represents a method that handles cohort-death events.
    /// </summary>
    public delegate void DeathEventHandler<TDeathEventArgs>(object          sender,
                                                            TDeathEventArgs eventArgs);
}
