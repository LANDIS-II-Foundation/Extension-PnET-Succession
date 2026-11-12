using System.Collections.Generic;
using System.IO;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public class LocalOutput
    {
        private List<string> FileContent;
        public static string PNEToutputsites;
        public string FileName { get; private set; }
        public string SiteName { get; private set; }
        public string OutputPath { get; private set; }

        public LocalOutput(string SiteName, string FileName, string Header)
        {
            this.SiteName = SiteName;
            this.OutputPath = "Output" + Path.DirectorySeparatorChar + PNEToutputsites + Path.DirectorySeparatorChar + SiteName + Path.DirectorySeparatorChar;
            this.FileName = FileName;
            if (File.Exists(OutputPath + FileName))
                File.Delete(OutputPath + FileName);
            if (!Directory.Exists(OutputPath))
                Directory.CreateDirectory(OutputPath);
            FileContent = new List<string>(new string[] { Header });
            Write();
        }

        public void Add(string s)
        {
            FileContent.Add(s);
        }

        public void Write()
        {
            while (true)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(Path.Combine(OutputPath, FileName), true);
                    foreach (string line in FileContent)
                        sw.WriteLine(line);
                    sw.Close();
                    FileContent.Clear();
                    return;
                }
                catch (IOException e)
                {
                    PlugIn.ModelCore.UI.WriteLine("Cannot write to " + Path.Combine(OutputPath, FileName) + " " + e.Message);
                }
            }
        }
    }
}
