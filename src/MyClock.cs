using System;
using System.Diagnostics;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class MyClock
    {
        Stopwatch sw = null;
        public int SumUnits { get; private set; }
        int unitsCount = 0;
        string update = "";

        public int Progress()
        {
            int Percentage = (int)Math.Round(100.0f * (unitsCount / SumUnits), 0);
            return Percentage;
        }

        long ElapsedTime
        {
            get
            {
                return sw.ElapsedMilliseconds;
            }
        }

        string Update
        {
            get
            {
                int length = update.Length;
                update = "Progress = " + Progress() + "%   Elapsed time " + MsToSec((int)sw.ElapsedMilliseconds) + "s   EstimatedTotalTime " + EstimatedTotalTime + "s";
                return update.PadRight(length, ' ');
            }
        }
        int MsToSec(int ProgressinMs)
        {
            return (int)(ProgressinMs / 1000.0);
        }

        int EstimatedTotalTime
        {
            get
            {
                // MG20260827 -- avoid divide-by-zero causing potential overflow error 
                int progress = Progress();
                int EstimatedTotalTime = 0;
                if (progress > 0)
                    EstimatedTotalTime = (int)Math.Round(100.0 / progress * MsToSec((int)sw.ElapsedMilliseconds), 0);
                return EstimatedTotalTime;
            }
        }

        public void WriteUpdate()
        {
            Console.Write("\r\t" + Update);
        }

        public void Next()
        {
            unitsCount++;
        }

        public MyClock(int SumUnits)
        {
            this.SumUnits = SumUnits;
            if (sw == null)
            {
                sw = new Stopwatch();
                sw.Start();
            }
        }
    }
}
