//  Authors:  Robert M. Scheller

using System.Collections.Generic;

namespace Landis.Extension.Succession.BiomassPnET
{
    /// <summary>
    /// Dynamic values for ecoregion growing space parameters.
    /// </summary>
    public interface IDynamicEcoregionRecord
    {

        double GSO1 { get; set; }

        double GSO2 { get; set; }

        double GSO3 { get; set; }

        double GSO4 { get; set; }
    }

    public class DynamicEcoregionRecord
    : IDynamicEcoregionRecord
    {

        private double gso1, gso2, gso3, gso4;


        public double GSO1
        {
            get {
                return gso1;
            }
            set {
                gso1 = value;
            }
        }

        public double GSO2
        {
            get
            {
                return gso2;
            }
            set
            {
                gso2 = value;
            }
        }

        public double GSO3
        {
            get
            {
                return gso3;
            }
            set
            {
                gso3 = value;
            }
        }

        public double GSO4
        {
            get
            {
                return gso4;
            }
            set
            {
                gso4 = value;
            }
        }

        public DynamicEcoregionRecord()
        {
        }

    }
}
