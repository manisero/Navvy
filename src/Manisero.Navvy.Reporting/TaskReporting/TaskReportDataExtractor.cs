using System;
using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Logging;
using Manisero.Navvy.Logging.Diagnostics;
using Manisero.Navvy.Reporting.Shared;
using Manisero.Navvy.Reporting.Utils;

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

            var diagnosticChartsData = GetDiagnosticChartsData(log.TaskDuration, log.Diagnostics);

            return new TaskReportData
            {
                StepsTimelineData = GetStepsTimelineData(log, stepNames).ToArray(),
                StepTimesData = GetStepTimesData(log, stepNames).ToArray(),
                MemoryData = diagnosticChartsData.Item1,
                CpuUsageData = diagnosticChartsData.Item2
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

            foreach (var stepName in stepNames)
            {
                yield return new object[] { stepName, log.StepLogs[stepName].Duration.Duration.GetLogValue() };
            }
        }

        private Tuple<ICollection<ICollection<object>>, ICollection<ICollection<object>>> GetDiagnosticChartsData(
            DurationLog taskDuration,
            IEnumerable<Diagnostic> diagnostics)
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
                    "Cpu usage" + PipelineReportingUtils.PercentUnit
                }
            };

            var relevantDiagnostics = diagnostics
                .Where(x => x.Timestamp.IsBetween(taskDuration.StartTs, taskDuration.EndTs))
                .OrderBy(x => x.Timestamp);
            
            double? prevCpuDiagnosticTime = null;

            foreach (var diagnostic in relevantDiagnostics)
            {
                var time = (diagnostic.Timestamp - taskDuration.StartTs).GetLogValue();

                memoryData.Add(new object[]
                {
                    time,
                    diagnostic.ProcessWorkingSet.ToMb(),
                    diagnostic.GcAllocatedSet.ToMb()
                });

                if (prevCpuDiagnosticTime == null)
                {
                    prevCpuDiagnosticTime = time;
                    continue;
                }

                if (diagnostic.CpuUsage.HasValue)
                {
                    var cpuUsage = diagnostic.CpuUsage.Value.ToPercentage();

                    cpuUsageData.Add(new object[]
                    {
                        prevCpuDiagnosticTime,
                        cpuUsage
                    });

                    cpuUsageData.Add(new object[]
                    {
                        time,
                        cpuUsage
                    });

                    prevCpuDiagnosticTime = time;
                }
            }

            return new Tuple<ICollection<ICollection<object>>, ICollection<ICollection<object>>>(
                memoryData, cpuUsageData);
        }
    }
}
