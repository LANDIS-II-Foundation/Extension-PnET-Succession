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
        public float ConductanceCO2;


        public SubCohortVars()
        {
        }

        public void Add(SubCohortVars s)
        {
            this.LAI += s.LAI;
            this.FolResp += s.FolResp;
            this.GrossPsn += s.GrossPsn;
            this.NetPsn += s.NetPsn;
            this.Transpiration += s.Transpiration;
            this.MaintenanceRespiration += s.MaintenanceRespiration;
            this.FWater += PlugIn.fIMAX * s.FWater;
            this.FRad += PlugIn.fIMAX * s.FRad;
            this.ConductanceCO2 += PlugIn.fIMAX * s.ConductanceCO2;
        }

        public void Reset()
        {
            MaintenanceRespiration = FWater = FRad = LAI = GrossPsn = FolResp = NetPsn = Transpiration = ConductanceCO2 = 0;
        }
    }
}
