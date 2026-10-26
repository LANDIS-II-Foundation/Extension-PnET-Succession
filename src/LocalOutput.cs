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
        public string Path { get; private set; }

        public LocalOutput(string SiteName, string FileName, string Header)
        {
            this.SiteName = SiteName;
            this.Path = "Output" + System.IO.Path.DirectorySeparatorChar + PNEToutputsites + System.IO.Path.DirectorySeparatorChar + SiteName + +System.IO.Path.DirectorySeparatorChar;
            this.FileName = FileName;
            if (File.Exists(Path + FileName))
                File.Delete(Path + FileName);
            if (Directory.Exists(Path) == false)
                Directory.CreateDirectory(Path);
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
                    StreamWriter sw = new StreamWriter(System.IO.Path.Combine(Path, FileName), true);
                    foreach (string line in FileContent)
                        sw.WriteLine(line);
                    sw.Close();
                    FileContent.Clear();
                    return;
                }
                catch (IOException e)
                {
                    PlugIn.ModelCore.UI.WriteLine("Cannot write to " + System.IO.Path.Combine(Path, FileName) + " " + e.Message);
                }
            }
        }
    }
}
