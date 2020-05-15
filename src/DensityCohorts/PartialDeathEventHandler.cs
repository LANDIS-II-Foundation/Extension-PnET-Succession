namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// Represents a method that handles cohort-death events.
    /// </summary>
    public delegate void PartialDeathEventHandler<TDeathEventArgs>(object          sender,
                                                            TDeathEventArgs eventArgs);
}
