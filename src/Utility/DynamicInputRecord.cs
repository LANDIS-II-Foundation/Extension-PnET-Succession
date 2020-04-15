//  Authors:  Robert M. Scheller

using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Values for each ecoregion x species combination.
    /// </summary>
    public interface IDynamicInputRecord
    {

        double ProbEst{get;set;}

    }

    public class DynamicInputRecord
    : IDynamicInputRecord
    {

        private double probEst;
        private int anpp_max_spp;
        private int b_max_spp;

        public double ProbEst
        {
            get {
                return probEst;
            }
            set {
                probEst = value;
            }
        }

        public DynamicInputRecord()
        {
        }

    }
}
