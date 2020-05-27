namespace Landis.Library.DensityCohorts.InitialCommunities
{
    /// <summary>
    /// Collection of initial communities.
    /// </summary>
    public interface IDataset
    {
        /// <summary>
        /// Finds a community by its map code.
        /// </summary>
        /// <param name="mapCode">
        /// The community's map code.
        /// </param>
        ICommunity Find(uint mapCode);
    }
}
