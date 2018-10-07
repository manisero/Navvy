using System;

namespace Manisero.Navvy.Reporting.Shared
{
    internal static class PipelineReportingUtils
    {
        public const string MsUnit = " [ms]";
        public const string MbUnit = " [mb]";

        public static object GetLogValue(this TimeSpan timeSpan) => timeSpan.TotalMilliseconds;

        public static double ToMb(this long bytes) => bytes / 1024d / 1024d;
    }
}
