using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Logging;
using Manisero.Navvy.Reporting.Shared;
using Manisero.Navvy.Reporting.Utils;

namespace Manisero.Navvy.Reporting.TaskReporting
{
    internal interface ITaskReportDataExtractor
    {
        TaskReportData Extract(
            TaskExecutionLog log);
    }

    internal class TaskReportDataExtractor : ITaskReportDataExtractor
    {
        public TaskReportData Extract(
            TaskExecutionLog log)
        {
            return new TaskReportData
            {
                StepTimesData = GetStepTimesData(log).ToArray(),
                MemoryData = GetMemoryData(log.TaskDuration, log.Diagnostics).ToArray()
            };
        }

        private IEnumerable<ICollection<object>> GetStepTimesData(
            TaskExecutionLog log)
        {
            yield return new[] { "Step", "Start" + PipelineReportingUtils.MsUnit, "End" + PipelineReportingUtils.MsUnit };

            var taskStartTs = log.TaskDuration.StartTs;

            foreach (var nameToLog in log.StepLogs)
            {
                var stepDuration = nameToLog.Value.Duration;

                yield return new[]
                {
                    nameToLog.Key,
                    (stepDuration.StartTs - taskStartTs).GetLogValue(),
                    (stepDuration.EndTs - taskStartTs).GetLogValue()
                };
            }
        }

        private IEnumerable<ICollection<object>> GetMemoryData(
            DurationLog taskDuration,
            IEnumerable<DiagnosticLog> diagnostics)
        {
            yield return new[]
            {
                "Time" + PipelineReportingUtils.MsUnit,
                "Process working set" + PipelineReportingUtils.MbUnit,
                "GC allocated set" + PipelineReportingUtils.MbUnit
            };

            var relevantDiagnostics = diagnostics
                .Where(x => x.Timestamp.IsBetween(taskDuration.StartTs, taskDuration.EndTs))
                .OrderBy(x => x.Timestamp);

            foreach (var diagnostic in relevantDiagnostics)
            {
                yield return new[]
                {
                    (diagnostic.Timestamp - taskDuration.StartTs).GetLogValue(),
                    diagnostic.ProcessWorkingSet.ToMb(),
                    diagnostic.GcAllocatedSet.ToMb()
                };
            }
        }
    }
}
