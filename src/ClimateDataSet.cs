using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class ClimateDataSet
    {
        // One line in a climate file
        public string Year;
        public string Month;
        public float CO2;
        public string O3;
        public ushort PAR0;
        public float Prec;
        public float Tmin;
        public float Tmax;
    }

    
}
