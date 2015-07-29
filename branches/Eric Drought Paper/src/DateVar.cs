using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class DateVar<T> 
    {
        private string label;
        public int FirstYear;
        public int LastYear;
        private T[,] values;

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
                    return values[600, 10];
                    //throw new System.Exception("Cannot get " + label + " "+ d +" range [min,max] = " + FirstYear + " " + LastYear);
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
        public DateVar( string label, int FirstYear, int LastYear)
        {
            this.label = label;
            this.FirstYear = FirstYear;
            this.LastYear = LastYear;
            values = new T[LastYear - FirstYear + 1, 12];
        }
    }
   
}
