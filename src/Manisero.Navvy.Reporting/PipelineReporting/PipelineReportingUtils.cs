using System;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal static class PipelineReportingUtils
    {
        public static object GetLogValue(this TimeSpan timeSpan) => timeSpan.TotalMilliseconds;

        public static double ToMb(this long bytes) => bytes / 1024d / 1024d;
    }
}
