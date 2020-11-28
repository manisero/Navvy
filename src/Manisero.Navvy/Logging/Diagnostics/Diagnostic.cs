using System;

namespace Manisero.Navvy.Logging.Diagnostics
{
    public struct Diagnostic
    {
        /// <summary>UTC.</summary>
        public DateTime Timestamp { get; }

        public long ProcessWorkingSet { get; }

        public long GcAllocatedSet { get; }

        public TimeSpan TotalProcessorTime { get; }

        /// <summary>From 0 to 1.</summary>
        public double? CpuUsage { get; }

        public Diagnostic(
            DateTime timestamp,
            long processWorkingSet,
            long gcAllocatedSet,
            TimeSpan totalProcessorTime,
            double? cpuUsage = null)
        {
            Timestamp = timestamp;
            ProcessWorkingSet = processWorkingSet;
            GcAllocatedSet = gcAllocatedSet;
            TotalProcessorTime = totalProcessorTime;
            CpuUsage = cpuUsage;
        }
    }
}
