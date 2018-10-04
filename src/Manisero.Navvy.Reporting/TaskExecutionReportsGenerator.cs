using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Logging;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Reporting.PipelineReporting;

namespace Manisero.Navvy.Reporting
{
    public interface ITaskExecutionReportsGenerator
    {
        IEnumerable<TaskExecutionReport> Generate(
            TaskDefinition task);
    }

    public class TaskExecutionReportsGenerator : ITaskExecutionReportsGenerator
    {
        public static ITaskExecutionReportsGenerator Instance = new TaskExecutionReportsGenerator();

        private readonly IPipelineReportDataExtractor _pipelineReportDataExtractor = new PipelineReportDataExtractor();
        private readonly IPipelineReportsGenerator _pipelineReportsGenerator = new PipelineReportsGenerator();

        public IEnumerable<TaskExecutionReport> Generate(
            TaskDefinition task)
        {
            var log = task.GetExecutionLog();

            foreach (var pipeline in task.Steps.Where(x => x is IPipelineTaskStep).Cast<IPipelineTaskStep>())
            {
                var data = _pipelineReportDataExtractor.Extract(pipeline, log);
                var pipelineReports = _pipelineReportsGenerator.Generate(data);

                foreach (var report in pipelineReports)
                {
                    yield return report;
                }
            }
        }

        public static TaskExecutionEvents CreateEvents(
            Func<TaskDefinition, string> reportsFolderPathFactory = null)
        {
            return new TaskExecutionEvents(
                taskEnded: x =>
                {
                    var reports = Instance.Generate(x.Task).ToArray();
                    x.Task.SetExecutionReports(reports);

                    if (reportsFolderPathFactory != null)
                    {
                        var reportsFolderPath = reportsFolderPathFactory(x.Task);
                        Directory.CreateDirectory(reportsFolderPath);

                        foreach (var report in reports)
                        {
                            var reportFilePath = Path.Combine(reportsFolderPath, report.Name);
                            File.WriteAllText(reportFilePath, report.Content);
                        }

                        x.Task.SetExecutionReportsPath(reportsFolderPath);
                    }
                });
        }
    }
}
