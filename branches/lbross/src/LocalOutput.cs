using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Landis.Extension.Succession.BiomassPnET 
{
    public class LocalOutput
    {
        public static string PNEToutputsites;

        public delegate void SendMsg(string Msg);

        private List<string> FileContent;
        public string FileName { get; private set; }
        public string SiteName { get; private set; }
        public string Path { get; private set; }

        SendMsg send_message = null;
        public LocalOutput(string SiteName, string FileName, string Header, SendMsg send_message)
        {
            this.send_message = send_message;

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
                    if (send_message != null)
                    {
                        send_message("Cannot write to " + System.IO.Path.Combine(Path, FileName) + " " + e.Message);
                    }

                    else throw e;

                }
            }
           
        }
    }
}
