using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manisero.Navvy.Core;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Logging;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Reporting.PipelineReporting;

namespace Manisero.Navvy.Reporting
{
    public interface ITaskExecutionReporter
    {
        IEnumerable<TaskExecutionReport> GenerateReports(
            TaskDefinition task);
    }

    public class TaskExecutionReporter : ITaskExecutionReporter
    {
        public static ITaskExecutionReporter Instance = new TaskExecutionReporter();

        private readonly IPipelineReportDataExtractor _pipelineReportDataExtractor = new PipelineReportDataExtractor();
        private readonly IPipelineReportsGenerator _pipelineReportsGenerator = new PipelineReportsGenerator();

        public IEnumerable<TaskExecutionReport> GenerateReports(
            TaskDefinition successfulTask)
        {
            var log = successfulTask.GetExecutionLog();

            foreach (var pipeline in successfulTask.Steps.Where(x => x is IPipelineTaskStep).Cast<IPipelineTaskStep>())
            {
                var data = _pipelineReportDataExtractor.Extract(pipeline, log);
                var pipelineReports = _pipelineReportsGenerator.Generate(data);

                foreach (var report in pipelineReports)
                {
                    yield return report;
                }
            }
        }

        /// <param name="reportsFolderPathFactory">If specified, generated reports will be saved in given folder.</param>
        public static IExecutionEvents[] CreateEvents(
            Func<TaskDefinition, string> reportsFolderPathFactory = null)
        {
            return new IExecutionEvents[]
            {
                new TaskExecutionEvents(
                    taskEnded: x =>
                    {
                        if (x.Result.Outcome != TaskOutcome.Successful)
                        {
                            return;
                        }

                        var reports = Instance.GenerateReports(x.Task).ToArray();
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
                    })
            };
        }
    }
}
