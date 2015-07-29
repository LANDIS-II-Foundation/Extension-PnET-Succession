using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Table;
using System.IO;
using RadiationTable;

namespace PnETII_RW
{
    public class COutput
    {
        public string OutputFolder;
        public COutput(string OutputFolder)
        {
            this.OutputFolder = OutputFolder;
            try
            {
                System.IO.Directory.CreateDirectory(OutputFolder);
            }
            catch
            { 
                
            }
        }
    }
}
