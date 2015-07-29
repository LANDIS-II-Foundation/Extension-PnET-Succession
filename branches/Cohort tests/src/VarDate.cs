using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class VarDate<T> 
    {
        private string label;
        public int FirstYear;
        public int LastYear;
        private T[,] values;

        public string Label
        {
            get
            {
                return label;
            }
        }

        public T this[DateTime d]
        {
            get
            {
                try
                {
                    return values[d.Year - FirstYear, d.Month - 1];
                }
                catch 
                {
                    throw new System.Exception("Cannot get " + label + " "+ d +" range [min,max] = " + FirstYear + " " + LastYear);
                }
            }
            set
            {
                try
                {
                    values[d.Year - FirstYear, d.Month - 1] = value;
                }
                catch  
                {
                    throw new System.Exception("Cannot set " + label + " " + d + " range [min,max] = " + FirstYear + " " + LastYear);
                }
            }
        }
        
        public VarDate(string label, DateTime[] daterange)
        {
            this.label = label;
            this.FirstYear = daterange[0].Year;
            this.LastYear = daterange[1].Year;
            values = new T[LastYear - FirstYear + 1, 12];
        }
        /*
        public VarDate(string label, int FirstYear, int LastYear)
        {
            this.label = label;
            this.FirstYear = FirstYear;
            this.LastYear = LastYear;
            values = new T[LastYear - FirstYear + 1, 12];
        }
         */
    }
   
}
