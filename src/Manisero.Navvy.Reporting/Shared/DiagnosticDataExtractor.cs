using System;
using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Logging;
using Manisero.Navvy.Logging.Diagnostics;
using Manisero.Navvy.Reporting.Utils;

namespace Manisero.Navvy.Reporting.Shared
{
    internal class DiagnosticChartsData
    {
        public ICollection<ICollection<object>> MemoryData { get; set; }

        public ICollection<ICollection<object>> CpuUsageData { get; set; }
    }

    internal static class DiagnosticDataExtractor
    {
        public static DiagnosticChartsData Extract(
            IEnumerable<Diagnostic> allDiagnostics,
            DurationLog forPeriod)
        {
            var memoryData = new List<ICollection<object>>
            {
                new[]
                {
                    "Time" + PipelineReportingUtils.MsUnit,
                    "Process working set" + PipelineReportingUtils.MbUnit,
                    "GC allocated set" + PipelineReportingUtils.MbUnit
                }
            };

            var cpuUsageData = new List<ICollection<object>>
            {
                new[]
                {
                    "Time" + PipelineReportingUtils.MsUnit,
                    "Avg CPU usage" + PipelineReportingUtils.PercentUnit
                }
            };
            
            FillData(
                allDiagnostics,
                forPeriod.StartTs,
                forPeriod.EndTs,
                memoryData,
                cpuUsageData);

            return new DiagnosticChartsData
            {
                MemoryData = memoryData,
                CpuUsageData = cpuUsageData
            };
        }

        private static void FillData(
            IEnumerable<Diagnostic> allDiagnostics,
            DateTime fromTs,
            DateTime toTs,
            ICollection<ICollection<object>> memoryDataToFill,
            ICollection<ICollection<object>> cpuUsageDataToFill)
        {
            var relevantDiagnostics = allDiagnostics
                .Where(x => x.Timestamp.IsBetween(fromTs, toTs))
                .OrderBy(x => x.Timestamp);

            double? prevCpuDiagnosticTime = null;

            foreach (var diagnostic in relevantDiagnostics)
            {
                var time = (diagnostic.Timestamp - fromTs).GetLogValue();

                memoryDataToFill.Add(
                    new object[] { time, diagnostic.ProcessWorkingSet.ToMb(), diagnostic.GcAllocatedSet.ToMb() });

                if (prevCpuDiagnosticTime == null)
                {
                    prevCpuDiagnosticTime = time;
                    continue;
                }

                if (diagnostic.CpuUsage.HasValue)
                {
                    var cpuUsage = diagnostic.CpuUsage.Value.ToPercentage();
                    cpuUsageDataToFill.Add(new object[] { prevCpuDiagnosticTime, cpuUsage });
                    cpuUsageDataToFill.Add(new object[] { time, cpuUsage });

                    prevCpuDiagnosticTime = time;
                }
            }
        }
    }
}
