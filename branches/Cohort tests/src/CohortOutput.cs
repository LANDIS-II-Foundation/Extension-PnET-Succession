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
        static string delim = Constants.delim;
         
        public static void WriteHeader(ActiveSite site, ICohort cohort)
        {
            string FileName = OutputCohortFileName(site, cohort);

            if (System.IO.File.Exists(FileName)) System.IO.File.Delete(FileName);

            string hdr="";

            hdr = "Year" + delim +
            "Month" + delim +
            "Date" + delim +
            "Age" + delim +
            "CanopyLayer" + delim +
            "PAR0(W_m2)" + delim +
            //"LeafOn" + delim +
            "LAI(m2)" + delim +
            "GDD(C|base=PsnTMin)" + delim +
            "HDD(C|base=PsnTMin)" + delim +
             "GrossPsn(g_m2_mo)" + delim +
             "FolResp(g_m2_mo)" + delim +
             "MaintResp(g_m2_mo)" + delim +
             "NetPsn(g_m2_mo)" + delim +
             "ReleasedNSC(g_m2_mo)" + delim +
             "Folalloc(g_m2_mo)" + delim +
             "RootAlloc(g_m2_mo)" + delim +
             "WoodAlloc(g_m2_mo)" + delim +
            "VPD(kPa)" + delim +
            "WUE(g_mm)" + delim +
            "Transpiration(mm_mo)" + delim +
            "Fol(gDW_m2)" + delim +
            //"FolShed(gDW_m2)" + delim +
            "Root(gDW_m2)" + delim +
            "Wood(gDW_m2)" + delim +
            "NSC(gC_m2)" + delim +
            "fWater(-)" + delim +
            "fTemp_psn" + delim +
            "fTemp_resp" + delim +
            "fage" + delim +
            "fRad(-)";

                System.IO.StreamWriter sw = System.IO.File.AppendText(FileName);
                sw.WriteLine(hdr);

                sw.Close();
        }
        private static string OutputCohortFileName(ActiveSite site, ICohort cohort)
        {
            string dir = Landis.Extension.Succession.BiomassPnET.InputParametersParser.Names.PNEToutputsites;
            if (System.IO.Directory.Exists("output/" + dir) == false)
            {
                System.IO.Directory.CreateDirectory("output/" + dir);
            }
            string FileName = "output/"+ dir +"/CohortData_" + site + "_" + cohort.YearOfBirth + "_" + cohort.Species.Name + Constants.ext;
            return FileName;
        }
        public static void WriteCohortData(DateTime date, ActiveSite site, CohortBiomass cohortbiomass, float water)
        {

            string FileName = OutputCohortFileName(site, cohortbiomass.Cohort);
            IEcoregion ecoregion = PlugIn.modelCore.Ecoregion[site];

            string contents = date.Year + delim +
                              date.Month + delim +
                              date.ToString("yyyy/MM") + delim +
                              cohortbiomass.Cohort.Age + delim +
                              cohortbiomass.Cohort.CanopyLayer + delim +
                              cohortbiomass.AboveCanopyRadiation + delim +
                              //StaticVariables.Leaf_On[ecoregion, cohortbiomass.Cohort.Species, date] + delim +
                              cohortbiomass.SumLAI() + delim +
                              StaticVariables.GDD[ecoregion, cohortbiomass.Cohort.Species, date] + delim +
                              StaticVariables.HDD[ecoregion, cohortbiomass.Cohort.Species, date] + delim +
                              cohortbiomass.GrossPsn + delim +
                              cohortbiomass.FolResp + delim +
                              cohortbiomass.Autotrophicrespiration + delim +
                              cohortbiomass.NetPsn + delim +
                              cohortbiomass.ReleasedNSC + delim +
                              cohortbiomass.Folalloc + delim +
                              cohortbiomass.WoodAlloc + delim +
                              cohortbiomass.RootAlloc + delim +
                              StaticVariables.VPD[ecoregion, date] + delim +
                              cohortbiomass.WUE + delim +
                              cohortbiomass.Transpiration + delim +
                              cohortbiomass.Cohort.Fol + delim +
                              //cohortbiomass.Cohort.FolShed + delim +
                              cohortbiomass.Cohort.Root + delim +
                              cohortbiomass.Cohort.Wood + delim +
                              cohortbiomass.Cohort.NSC + delim +
                              cohortbiomass.FWater + delim +
                              StaticVariables.FTempPSN[ecoregion, cohortbiomass.Cohort.Species, date] + delim +
                              StaticVariables.FTempResp[ecoregion, cohortbiomass.Cohort.Species, date] + delim +
                              cohortbiomass.Fage + delim +
                              cohortbiomass.FRad + delim;
                               


           
            System.IO.StreamWriter sw = System.IO.File.AppendText(FileName);
            sw.WriteLine(contents);

            sw.Close();
        }
         
    }
}
