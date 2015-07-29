using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.SpatialModeling;
using Landis.Library.BiomassCohortsPnET;
using System.IO;

namespace Landis.Extension.Succession.BiomassPnET
{
    static class SiteOutput
    {
        static string delim = Constants.delim;
        private static ISiteVar<bool> HasSiteOutput;
        private static string OutputSiteFileName(ActiveSite site)
        {
            return "output/SiteData_" + site + Constants.ext;
        }

        public static void WriteHeader(ActiveSite site)
        {
            if (HasSiteOutput[site] == false) return;
            string FileName = OutputSiteFileName(site);
            StreamWriter sw = new StreamWriter(FileName);

            string s = "Year" + delim +
                        "Month" + delim +
                        "date" + delim +
                        "CanopyLayers" + delim +
                        "Tday(C)" + delim +
                        "Precip(mm_mo)" + delim +
                        "RunOff(mm_mo)" + delim +
                        "WaterLeakage(mm_mo)" + delim +
                        "Transpiration(mm)" + delim +
                        "Evaporation(mm_mo)" + delim +
                        "water(mm)" + delim +
                        "LAI(m2)" + delim +
                        "VPD(kPa)" + delim +
                        "GrossPsn(gC_mo)" + delim +
                        "NetPsn(gC_mo)" + delim +
                        "AutoResp(gC_mo)" + delim +
                        "HeteroResp(gC_mo)" + delim +
                        "TotalBiomass (gDW)" + delim +
                        "TotalRoot (gDW)" + delim +
                        "TotalFol (gDW)" + delim +
                        "TotalNSC (gC)" + delim +
                        "Litter(gDW_m2)" + delim +
                        "CWD(gDW_m2)";

            sw.WriteLine(s);
            sw.Close();
        }

        
        static List<string> FileContent = new List<string>();
        public static void UpdateSiteData(DateTime date, ActiveSite site)
        { 
            if (SiteOutput.HasSiteOutput[site] ==false) return;

             
            string s = date.Year.ToString() + delim +
                       date.Month + delim +
                       date.ToString("MM/yyyy") + delim +
                       CanopyBiomass.NumberOfCanopyLayers[site] + delim +
                       Static.Tday[date] + delim +
                       Static.Prec[date] + delim +
                       Hydrology.RunOff[site] + delim +
                       Hydrology.WaterLeakage[site] + delim +
                       Hydrology.Transpiration[site] + delim +
                       Hydrology.Evaporation[site] + delim +
                       Hydrology.Water[site] + delim +
                       CanopyBiomass.CanopyLAI[site] + delim +
                       Static.VPD[date] + delim +
                       CanopyBiomass.GrossPsn[site] + delim +
                       CanopyBiomass.NetPsn[site] + delim +
                       CanopyBiomass.AutotrophicRespiration[site] + delim +
                       ForestFloor.HeterotrophicRespiration[site] + delim +
                       CanopyBiomass.TotalBiomass[site] + delim +
                       CanopyBiomass.TotalRoot[site] + delim +
                       CanopyBiomass.TotalFoliage[site] + delim +
                       CanopyBiomass.TotalNSC[site] + delim +
                       ForestFloor.Litter[site].Mass + delim +
                       ForestFloor.WoodyDebris[site].Mass;

            FileContent.Add(s);
        }
        
        public static void WriteSiteData(ActiveSite site)
        {
            if (SiteOutput.HasSiteOutput[site] == false) return;
            string FileName = OutputSiteFileName(site);
            StreamWriter sw = File.AppendText(FileName);
            foreach (string s in FileContent)
            {
                sw.WriteLine(s);
            }
            
            sw.Close();
            FileContent.Clear();
        }
        
        public static void Initialize( IInputParameters parameters)
        {
            HasSiteOutput = parameters.HasSiteOutput;

             

            
        }
    }
}
