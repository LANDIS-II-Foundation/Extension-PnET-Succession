//  Authors:  Robert M. Scheller

using System.Collections.Generic;

namespace Landis.Library.DensityCohorts
{
    /// <summary>
    /// Values for each ecoregion x species combination.
    /// </summary>
    public interface IDiameterInputRecord
    {

        Dictionary<int,double> Diameters{get;set;}

    }

    public class DiameterInputRecord
    : IDiameterInputRecord
    {

        private Dictionary<int,double> diameters;

        public Dictionary<int, double> Diameters
        {
            get {
                return diameters;
            }
            set {
                diameters = value;
            }
        }

        public DiameterInputRecord()
        {
            this.Diameters = new Dictionary<int, double>();
        }

    }
}
