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
