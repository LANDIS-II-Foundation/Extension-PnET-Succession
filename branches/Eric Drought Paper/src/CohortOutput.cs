using Landis.SpatialModeling;
using Landis.Library.BiomassCohortsPnET;
using Landis.Core;
using System.Collections.Generic;
using Landis.Library.InitialCommunities;
using System.Diagnostics;
using System.Threading;
using Landis.Library.Biomass;
using System;

namespace Landis.Extension.Succession.BiomassPnET
{
     
    public class CohortOutput
    {
        private static ISiteVar<bool> HasSiteOutput;
        static string FileName;
         
        static string delim = Constants.delim;

        public static void Initialize(IInputParameters parameters)
        {
            HasSiteOutput = parameters.HasSiteOutput;
        }

        public static void WriteHeader(ActiveSite site, ICohort cohort)
        {
            if (HasSiteOutput[site] == false) return; 

            FileName = "output/CohortData_" + site + "_" + cohort.YearOfBirth + "_" + cohort.Species.Name + Constants.ext;
            if (System.IO.File.Exists(FileName)) System.IO.File.Delete(FileName);

            string hdr="";
             
                hdr = "Year" + delim +
                "Month" + delim +
                "Date" + delim +
                "CanopyLayer" + delim +
                "PAR0(W_m2)" + delim +
                "LeafOn" + delim +
                "LAI(m2)" + delim +
                "GDD(C|base=PsnTMin)" + delim +
                 "GrossPsn(g_m2_mo)" + delim +
                 "FolResp(g_m2_mo)" + delim +
                 "MaintResp(g_m2_mo)" + delim +
                "NetPsn(g_m2_mo)" + delim +
                "ReleasedNSC" + delim +
                "Folalloc" + delim +
                "RootAlloc" + delim +
                "WoodAlloc" + delim +
                "VPD" + delim +
                "WUE(g_mm)" + delim +
                "DelAmax(-)" + delim +
                "Transpiration(mm_mo)" + delim +
                "Fol(gDW_m2)" + delim +
                //"FolShed(gDW_m2)" + delim +
                "Root(gDW_m2)" + delim +
                "Wood(gDW_m2)" + delim +
                "NSC(gC_m2)" + delim +
                "Water(mm)" + delim +
                "wfps" + delim +
                "dWater(-)" + delim +
                "DTemp_psn" + delim +
                "DTemp_resp" + delim +
                "fage" + delim +
                "fRad(-)";

                System.IO.StreamWriter sw = System.IO.File.AppendText(FileName);
                sw.WriteLine(hdr);

                sw.Close();
        }
        public static void WriteCohortData(DateTime date, ActiveSite site, CohortBiomass cohortbiomass)
        {
            if (HasSiteOutput[cohortbiomass.Site] == false) return;


            FileName = "output/CohortData_" + site + "_" + cohortbiomass.Cohort.YearOfBirth + "_" + cohortbiomass.Cohort.Species.Name + Constants.ext;

            string contents = date.Year + delim +
                              date.Month + delim +
                              date.ToString("yyyy/MM") + delim +
                              cohortbiomass.Cohort.CanopyLayer + delim +
                              cohortbiomass.AboveCanopyRadiation + delim +
                              cohortbiomass.Cohort.Leaf_On + delim +
                              cohortbiomass.LAI + delim +
                              Static.GDD[date][cohortbiomass.Cohort.Species] + delim +
                              cohortbiomass.GrossPsn + delim +
                              cohortbiomass.FolResp + delim +
                              cohortbiomass.MaintResp + delim +
                              cohortbiomass.NetPsn + delim +
                              cohortbiomass.ReleasedNSC + delim +
                              cohortbiomass.Folalloc + delim +
                              cohortbiomass.WoodAlloc + delim +
                              cohortbiomass.RootAlloc + delim +
                              Static.VPD[date] + delim +
                              cohortbiomass.WUE + delim +
                              Static.DelAmax[date][cohortbiomass.Cohort.Species] + delim +
                              cohortbiomass.Transpiration + delim +
                              cohortbiomass.Cohort.Fol + delim +
                              //cohortbiomass.Cohort.FolShed + delim +
                              cohortbiomass.Cohort.Root + delim +
                              cohortbiomass.Cohort.Wood + delim +
                              cohortbiomass.Cohort.NSC  + delim +
                              Hydrology.Water[cohortbiomass.Site] + delim +
                               cohortbiomass.WFPS + delim +
                              cohortbiomass.DWater + delim +
                              Static.DTempPSN[date][cohortbiomass.Cohort.Species] + delim +
                              Static.DTempResp[date][cohortbiomass.Cohort.Species] + delim +
                              cohortbiomass.Fage + delim +
                              cohortbiomass.fRad;

            System.IO.StreamWriter sw = System.IO.File.AppendText(FileName);
            sw.WriteLine(contents);

            sw.Close();
        }
         
    }
}
