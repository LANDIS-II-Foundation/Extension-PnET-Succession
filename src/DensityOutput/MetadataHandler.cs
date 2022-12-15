using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Landis.Library.Metadata;
using Landis.Core;
using Landis.Utilities;
using System.IO;
using Flel = Landis.Utilities;

namespace Landis.Extension.Output.Density
{
    public static class MetadataHandler
    {
        public static ExtensionMetadata Extension { get; set; }
        
        public static void InitializeMetadata(int Timestep, string summaryLogName, bool makeTable)
        {

            ScenarioReplicationMetadata scenRep = new ScenarioReplicationMetadata()
            {
                RasterOutCellArea = PlugIn.ModelCore.CellArea,
                TimeMin = PlugIn.ModelCore.StartTime,
                TimeMax = PlugIn.ModelCore.EndTime,
            };

            Extension = new ExtensionMetadata(PlugIn.ModelCore)
            {
                Name = PlugIn.ExtensionName,
                TimeInterval = Timestep, 
                ScenarioReplicationMetadata = scenRep
            };

            //---------------------------------------
            //          table outputs:   
            //---------------------------------------

            if (makeTable)
            {
                CreateDirectory(summaryLogName);
                CreateDirectory("Density_cohort_log.csv");
                PlugIn.summaryLog = new MetadataTable<SummaryLog>(summaryLogName);
                PlugIn.summaryLogCohort = new MetadataTable<SummaryLogCohort>("Density_cohort_log.csv");

                PlugIn.ModelCore.UI.WriteLine("   Generating summary table...");
                OutputMetadata tblOut_summary = new OutputMetadata()
                {
                    Type = OutputType.Table,
                    Name = "SummaryLog",
                    FilePath = PlugIn.summaryLog.FilePath,
                    Visualize = true,
                };
                tblOut_summary.RetriveFields(typeof(SummaryLog));
                Extension.OutputMetadatas.Add(tblOut_summary);
            }

            //2 kinds of maps: species and pool maps, maybe multiples of each?
            //---------------------------------------            
            //          map outputs:         
            //---------------------------------------


            foreach(ISpecies species in PlugIn.speciesToMap)
            {
                OutputMetadata treeOut_Species = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = species.Name,
                    FilePath = SpeciesMapNames.ReplaceTemplateVars("outputs/density/{species}-TreeNumber-{timestep}.img",
                                                       species.Name,
                                                       PlugIn.ModelCore.CurrentTime),
                    Map_DataType = MapDataType.Continuous,
                    Visualize = true,
                    //Map_Unit = "categorical",
                };
                Extension.OutputMetadatas.Add(treeOut_Species);

                OutputMetadata basalOut_Species = new OutputMetadata()
                {
                    Type = OutputType.Map,
                    Name = species.Name,
                    FilePath = SpeciesMapNames.ReplaceTemplateVars("outputs/density/{species}-BasalArea-{timestep}.img",
                                       species.Name,
                                       PlugIn.ModelCore.CurrentTime),
                    Map_DataType = MapDataType.Continuous,
                    Visualize = true,
                    //Map_Unit = "categorical",
                };
                Extension.OutputMetadatas.Add(basalOut_Species);
            }

            OutputMetadata mapOut_TotalTrees = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "AllSpecies",
                FilePath = SpeciesMapNames.ReplaceTemplateVars("outputs/density/{species}-TreeNumber-{timestep}.img",
                                       "AllSpecies",
                                       PlugIn.ModelCore.CurrentTime),
                Map_DataType = MapDataType.Continuous,
                Visualize = true,
                //Map_Unit = "categorical",
            };
            Extension.OutputMetadatas.Add(mapOut_TotalTrees);

            OutputMetadata mapOut_TotalBasal = new OutputMetadata()
            {
                Type = OutputType.Map,
                Name = "AllSpecies",
                FilePath = SpeciesMapNames.ReplaceTemplateVars("outputs/density/{species}-BasalArea-{timestep}.img",
                           "AllSpecies",
                           PlugIn.ModelCore.CurrentTime),
                Map_DataType = MapDataType.Continuous,
                Visualize = true,
                //Map_Unit = "categorical",
            };
            Extension.OutputMetadatas.Add(mapOut_TotalTrees);


            //---------------------------------------
            MetadataProvider mp = new MetadataProvider(Extension);
            mp.WriteMetadataToXMLFile("Metadata", Extension.Name, Extension.Name);




        }
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
}