using Landis.Core;
using Landis.SpatialModeling;
using Landis.Library.Succession;
using Landis.Library.InitialCommunities;
using Landis.Library.BiomassCohortsPnET;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;
using System.IO;
using System;


namespace Landis.Extension.Succession.BiomassPnET
{
    public class FileProps
    {
        private char delim;
        private string ext;

        public char Delim
        {
            get
            {
                return delim;
            }
        }
        public string Ext
        {
            get
            {
                return ext;
            }
        }
        public enum FileDelimiters
        {
            comma,
            tab
        }
        private void SetDelimiter(FileDelimiters f)
        {
            if (f == FileDelimiters.comma) delim = ',';
            else if (f == FileDelimiters.tab) delim = ',';
        }
        private void SetExtension(FileDelimiters f)
        {
            if (f == FileDelimiters.comma) ext = ".csv";
            else if (f == FileDelimiters.tab) ext = ".txt";
        }

        public FileProps(FileDelimiters f)
        {
            SetDelimiter(f);
            SetExtension(f);
        }
    }
    
}
