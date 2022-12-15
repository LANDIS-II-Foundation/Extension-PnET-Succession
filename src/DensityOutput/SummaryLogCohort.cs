using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;

namespace Landis.Extension.Output.Density
{
    public class SummaryLogCohort 
    {

        [DataFieldAttribute(Unit = FieldUnits.Year, Desc = "Simulation Year")]
        public int Time { set; get; }

        [DataFieldAttribute(Desc = "Site Index")]
        public uint SiteIndex { set; get; }

        [DataFieldAttribute(Desc = "Ecoregion Name")]
        public string EcoName { set; get; }

        [DataFieldAttribute(Desc = "Species")]
        public string Species { set; get; }

        [DataFieldAttribute(Desc = "Age")]
        public ushort Age { set; get; }

        [DataFieldAttribute(Desc = "Number of Trees")]
        public double TreeNumber { set; get; }

        [DataFieldAttribute(Desc = "Diameter")]
        public double Diameter { set; get; }

        [DataFieldAttribute(Desc = "Biomass")]
        public double Biomass { set; get; }
    }
}
