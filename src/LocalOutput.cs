using System.Collections.Generic;
using System.IO;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public class LocalOutput
    {
        public static string PNEToutputsites;

         

        private List<string> FileContent;
        public string FileName { get; private set; }
        public string SiteName { get; private set; }
        public string Path { get; private set; }

        
        public LocalOutput(string SiteName, string FileName, string Header)
        {
            

            this.SiteName = SiteName;
            this.Path = "Output/" + PNEToutputsites + "/" + SiteName + "/";
            this.FileName = FileName;

            if (System.IO.File.Exists(Path + FileName))
            {
                System.IO.File.Delete(Path + FileName);
            }

            if (System.IO.Directory.Exists(Path) == false)
            {
                System.IO.Directory.CreateDirectory(Path);
            }
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
                    {
                        sw.WriteLine(line);
                    }
                    sw.Close();
                    FileContent.Clear();
                    return;
                }
                catch (System.IO.IOException e)
                {
                    PlugIn.ModelCore.UI.WriteLine("Cannot write to " + System.IO.Path.Combine(Path, FileName) + " " + e.Message);
                    
                }
            }
           
        }
    }
}
