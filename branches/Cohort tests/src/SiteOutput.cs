using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Succession;
using Landis.Library.InitialCommunities;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System.IO;
using System;
using Landis.Library.Biomass;

namespace Landis.Extension.Succession.BiomassPnET
{
    class SiteOutput
    {

        static string delim = Constants.delim;
        List<string> FileContent = new List<string>();
        ActiveSite site;
        
        private static string OutputSiteFileName(ActiveSite site)
        {
            string dir = Landis.Extension.Succession.BiomassPnET.InputParametersParser.Names.PNEToutputsites;
            if (System.IO.Directory.Exists("output/" + dir) == false)
            {
                System.IO.Directory.CreateDirectory("output/" + dir);
            }
            return "output/"+dir +"/SiteData_" + site + Constants.ext;
        }
        public SiteOutput(ActiveSite site)
        {
            this.site = site;
             
            WriteHeader(site);
        }
        public void WriteHeader(ActiveSite site)
        {
            
            StreamWriter sw = new StreamWriter(OutputSiteFileName(site));

            string s = "Year" + delim +
                        "Month" + delim +
                        "date" + delim +
                        "NrOfCohorts" + delim +
                        "Tday(C)" + delim +
                        "Precip(mm_mo)" + delim +
                        "RunOff(mm_mo)" + delim +
                        "Transpiration(mm_mo)" + delim +
                        "PrecipLoss(mm_mo)" + delim +
                        "water(mm)" + delim +
                        "SnowPack (mm)" + delim +
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
        public void UpdateSiteData(DateTime date, SiteConditions sc)
        {
            IEcoregion ecoregion = PlugIn.modelCore.Ecoregion[sc.Site];

            string s = date.Year.ToString() + delim +
                     date.Month + delim +
                     date.ToString("MM/yyyy") + delim +
                     sc.NrOfCohorts + delim +
                     StaticVariables.Tday[ecoregion, date] + delim +
                     StaticVariables.Prec[ecoregion, date] + delim +
                     sc.hydrology.RunOff + delim +
                     sc.hydrology.Transpiration + delim +
                     sc.hydrology.PrecipLoss + delim +
                     sc.hydrology.Water + delim +
                     sc.hydrology.SnowPack + delim +
                     sc.CanopyLAImax + delim +
                     StaticVariables.VPD[ecoregion, date] + delim +
                     sc.GrossPsn + delim +
                     sc.NetPsn + delim +
                     sc.AutotrophicRespiration + delim +
                     sc.forestfloor.HeterotrophicRespiration + delim +
                     sc.TotalBiomass + delim +
                     sc.TotalRoot + delim +
                     sc.TotalFoliage + delim +
                     sc.TotalNSC + delim +
                     sc.Litter.Mass + delim +
                     sc.WoodyDebris.Mass;

            FileContent.Add(s);
        }
        
        public void WriteSiteData()
        {
             
            StreamWriter sw = File.AppendText(OutputSiteFileName(site));
            foreach (string s in FileContent)
            {
                sw.WriteLine(s);
            }
            
            sw.Close();
            FileContent.Clear();
        }
        
       
    }
}
