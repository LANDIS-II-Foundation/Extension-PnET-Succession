/*// JSF - FIX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;
using Landis.Core;
using Landis.Utilities;
using System.IO;
using Flel = Landis.Utilities;

namespace Landis.Library.DensityCohorts
{
    public static class MetadataHandler
    {
        public static ExtensionMetadata Extension { get; set; }

        public static void InitializeMetadata(int Timestep, string summaryLogName, bool makeTable)
        {

            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata()
            {
                RasterOutCellArea = EcoregionData.ModelCore.CellArea,
                TimeMin = EcoregionData.ModelCore.StartTime,
                TimeMax = EcoregionData.ModelCore.EndTime,
            };

            Extension = new ExtensionMetadata(EcoregionData.ModelCore)
            {
                Name = Names.ExtensionName,
                TimeInterval = Timestep,
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            if (makeTable)
            {
                CreateDirectory(summaryLogName);

                SiteVars.summaryLogMortality = new MetadataTable<SummaryLogMortality>(summaryLogName);

                EcoregionData.ModelCore.UI.WriteLine("   Generating summary table...");
                OutputMetadata tblOut_summary = new OutputMetadata()
                {
                    Type = OutputType.Table,
                    Name = "SummaryLog",
                    FilePath = PlugIn.summaryLogMortality.FilePath,
                    Visualize = true,
                };
                tblOut_summary.RetriveFields(typeof(SummaryLogMortality));
                Extension.OutputMetadatas.Add(tblOut_summary);
            }


        }

        //---------------------------------------
        //          create directory:   
        //---------------------------------------

        public static void CreateDirectory(string path)
        {
            path = path.Trim(null);
            if (path.Length == 0)
                throw new ArgumentException("path is empty or just whitespace");

            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Flel.Directory.EnsureExists(dir);
            }

            return;
        }
    }

}*/