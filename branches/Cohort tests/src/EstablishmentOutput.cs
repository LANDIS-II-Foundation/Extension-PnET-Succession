using Landis.Core;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Library.BiomassCohortsPnET;
using Landis.SpatialModeling;
using Landis.Extension.Succession.BiomassPnET;

using System;
using System.Collections.Generic;
using System.IO;

namespace Landis.Extension.Succession.BiomassPnET
{
    class EstablishmentOutput
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
            return "output/" + dir + "/SiteEstData_" + site + Constants.ext;
        }
        public EstablishmentOutput(ActiveSite site)
        {
            this.site = site;

            WriteHeader(site);
        }
        public void WriteHeader(ActiveSite site)
        {
            string s = "Year" + delim +
                        "Month" + delim +
                        "date" + delim +
                        "water(mm)" + delim +
                        "SubCanopyPAR(-)" + delim;
            
            foreach(ISpecies spc in PlugIn.modelCore.Species)s += "Pest_" + spc.Name + "(-)"+delim ; 
           
            FileContent.Add(s);            

        }

        public void UpdateEstData(DateTime date, SiteConditions sc)
        {
            //System.Console.WriteLine("UpdateEstData");

            string s = date.Year.ToString() + delim +
                     date.Month + delim +
                     date.ToString("MM/yyyy") + delim +
                     sc.hydrology.Water + delim +
                     sc.SubCanopyPAR + delim;

            foreach (ISpecies spc in PlugIn.modelCore.Species)
            {
                s += sc.Establishment.Pest[spc] + delim ;
            }
            /*
            foreach (ISpecies spc in PlugIn.modelCore.Species)
            {
                s +=   sc.Establishment.PotEstablishments[spc] + delim;
            }
            foreach (ISpecies spc in PlugIn.modelCore.Species)
            {
                s += sc.Establishment.Establishments[spc] + delim;
            }
             */
            FileContent.Add(s);

            System.IO.File.WriteAllLines(OutputSiteFileName(site), FileContent.ToArray());
        }

         


    }
}
