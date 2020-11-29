using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Logging;
using Manisero.Navvy.Reporting.Shared;

namespace Manisero.Navvy.Reporting.TaskReporting
{
    internal interface ITaskReportDataExtractor
    {
        TaskReportData Extract(
            TaskDefinition task,
            TaskExecutionLog log);
    }

    internal class TaskReportDataExtractor : ITaskReportDataExtractor
    {
        public TaskReportData Extract(
            TaskDefinition task,
            TaskExecutionLog log)
        {
            var stepNames = task.Steps
                .Select(x => x.Name)
                .Where(x => log.StepLogs.ContainsKey(x))
                .ToArray();

            var diagnosticChartsData = DiagnosticDataExtractor.Extract(
                log.DiagnosticsLog.Diagnostics,
                log.TaskDuration);

            return new TaskReportData
            {
                StepsTimelineData = GetStepsTimelineData(log, stepNames).ToArray(),
                StepTimesData = GetStepTimesData(log, stepNames).ToArray(),
                MemoryData = diagnosticChartsData.MemoryData,
                CpuUsageData = diagnosticChartsData.CpuUsageData
            };
        }

        private IEnumerable<ICollection<object>> GetStepsTimelineData(
            TaskExecutionLog log,
            IEnumerable<string> stepNames)
        {
            yield return new[] { "Step", "Start" + PipelineReportingUtils.MsUnit, "End" + PipelineReportingUtils.MsUnit };

            var taskStartTs = log.TaskDuration.StartTs;

            foreach (var stepName in stepNames)
            {
                var stepDuration = log.StepLogs[stepName].Duration;

                yield return new object[]
                {
                    stepName,
                    (stepDuration.StartTs - taskStartTs).GetLogValue(),
                    (stepDuration.EndTs - taskStartTs).GetLogValue()
                };
            }
        }

        private IEnumerable<ICollection<object>> GetStepTimesData(
            TaskExecutionLog log,
            IEnumerable<string> stepNames)
        {
            yield return new[] { "Step", "Duration" + PipelineReportingUtils.MsUnit };
            yield return new object[] { "[Total]", log.TaskDuration.Duration.GetLogValue() };

            foreach (var stepName in stepNames)
            {
                yield return new object[] { stepName, log.StepLogs[stepName].Duration.Duration.GetLogValue() };
            }
        }
    }
}
