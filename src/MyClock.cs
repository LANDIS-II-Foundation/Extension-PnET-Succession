using System;

namespace Landis.Extension.Succession.BiomassPnET
{
    public class MyClock
    {
        System.Diagnostics.Stopwatch sw = null;
        public int SumUnits { get; private set; }
        int unitsCount = 0;

        public int Progress()
        {
            int Percentage =(int)Math.Round(100.0F * ((float)unitsCount / (float)SumUnits),0);
            return Percentage;
        }
        long ElapsedTime
        {
            get
            {
                return sw.ElapsedMilliseconds;
            }
        }

        string update = "";

        string Update
        {
            get
            {
                int length = update.Length;

                //Console.Write("\rInitialization progress {0}% Elapsed time {1} Estimated total time {2}   ", System.String.Format("{0:0.00}", Percentage), Math.Round(sw.ElapsedMilliseconds / 1000F, 0), Math.Round(EstimatedTotalTime, 0));
                update = "Progress = " + Progress() + "% Elapsed time " + MsToSec((int)sw.ElapsedMilliseconds) + "s EstimatedTotalTime " + EstimatedTotalTime +"s";

                return update.PadRight(length, ' ');
                //return "Progress = " + Progress() + " Elapsed time " + MsToSec((int)sw.ElapsedMilliseconds) + " EstimatedTotalTime " + EstimatedTotalTime;
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
                int EstimatedTotalTime = (int)Math.Round(100.0 / Progress() * MsToSec((int)sw.ElapsedMilliseconds), 0);
                return EstimatedTotalTime;
            }
        }
        //
        public void WriteUpdate()
        {
            Console.Write("\r\t" + Update);
            //PlugIn.ModelCore.UI.WriteLine("\r\t" + Update);
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
                sw = new System.Diagnostics.Stopwatch();
                sw.Start();
            }
        }
         
    }
}
