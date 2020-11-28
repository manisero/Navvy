using System;
using System.Diagnostics;

namespace Manisero.Navvy.Logging.Diagnostics
{
    internal static class DiagnosticsProvider
    {
        public static Diagnostic GetFirstDiagnostic()
        {
            var timestamp = DateTime.UtcNow;
            var process = Process.GetCurrentProcess();

            return new Diagnostic(
                timestamp,
                process.WorkingSet64,
                GC.GetTotalMemory(false),
                process.TotalProcessorTime);
        }

        /// <summary>Caution: for low interval between calls, cpu usage calculation is inaccurate. 50 ms interval should be fine</summary>
        public static Diagnostic GetDiagnostic(
            Diagnostic previousDiagnostic)
        {
            var timestamp = DateTime.UtcNow;
            var process = Process.GetCurrentProcess();

            var totalProcessorTime = process.TotalProcessorTime;
            var cpuUsage = CalculateCpuUsage(
                previousDiagnostic.Timestamp,
                previousDiagnostic.TotalProcessorTime,
                timestamp,
                totalProcessorTime);

            return new Diagnostic(
                timestamp,
                process.WorkingSet64,
                GC.GetTotalMemory(false),
                totalProcessorTime,
                cpuUsage);
        }

        private static double CalculateCpuUsage(
            DateTime prevTimestamp,
            TimeSpan prevTotalCpuUsage,
            DateTime currentTimestamp,
            TimeSpan currentTotalCpuUsage)
        {
            var timeDelta = (currentTimestamp - prevTimestamp).Ticks;
            var processorTimeDelta = (currentTotalCpuUsage - prevTotalCpuUsage).Ticks;

            return (double)processorTimeDelta / timeDelta / Environment.ProcessorCount;
        }
    }
}
