using System;
using System.Diagnostics;

namespace Industry.Utilities
{
    public class Timer
    {
        public enum Units
        {
            Seconds, Milliseconds, NanoSeconds
        }

        public Timer()
        {
            stopwatch = new Stopwatch();
        }

        private Stopwatch stopwatch;

        public Timer Start()
        {
            if (!stopwatch.IsRunning)
                stopwatch.Start();

            return this;
        }
        
        public void Stop(bool debugMsg = true)
        {
            double ms = ElapsedTime(Units.Milliseconds);
            
            if (debugMsg)
                UnityEngine.Debug.Log($"Elapsed: <color=yellow> {ms} ms.</color>");
        }

        public double ElapsedTime(Units time = Units.Milliseconds)
        {
            if (stopwatch.IsRunning)
                stopwatch.Stop();
            else return 0;

            double ticks = stopwatch.ElapsedTicks;
            ticks /= Stopwatch.Frequency;
            stopwatch.Reset();

            if (time == Units.NanoSeconds)
                return 1000000000.0 * ticks;
            else if (time == Units.Milliseconds)
                return 1000.0 * ticks;
            else
                return ticks;
        }        
    
        public void LogElapsedTime(string prefixText = "", Units time = Units.Milliseconds)
        {
            double elapsed = ElapsedTime(time);

            Logger.Log($"{prefixText} {elapsed} ms.");
        }
    }
}
