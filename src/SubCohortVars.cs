using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class SubCohortVars
    {
        public float LAI;
        public float GrossPsn;
        public float FolResp;
        public float NetPsn;
        public float Transpiration;
        public float FRad;
        public float FWater;
        public float MaintenanceRespiration;

        public SubCohortVars()
        {
        }

        public void Reset()
        {
            FWater = FRad = LAI = GrossPsn = FolResp = NetPsn = Transpiration = 0;
        }
    }
}
