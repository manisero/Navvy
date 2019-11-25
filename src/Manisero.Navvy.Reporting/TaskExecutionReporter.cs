using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manisero.Navvy.Core;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Logging;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Reporting.PipelineReporting;
using Manisero.Navvy.Reporting.Shared;
using Manisero.Navvy.Reporting.TaskReporting;

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

        private readonly ITaskReportsGenerator _taskReportsGenerator;
        private readonly IPipelineReportsGenerator _pipelineReportsGenerator;

        public TaskExecutionReporter()
        {
            var reportsFormatter = new ReportsFormatter();

            _taskReportsGenerator = new TaskReportsGenerator(
                new TaskReportDataExtractor(),
                reportsFormatter);

            _pipelineReportsGenerator = new PipelineReportsGenerator(
                new PipelineReportDataExtractor(),
                reportsFormatter);
        }

        public IEnumerable<TaskExecutionReport> GenerateReports(
            TaskDefinition successfulTask)
        {
            var log = successfulTask.GetExecutionLog();

            var taskReports = _taskReportsGenerator.Generate(successfulTask, log);

            foreach (var report in taskReports)
            {
                yield return report;
            }

            foreach (var pipeline in successfulTask.Steps
                .Where(x => log.StepLogs.ContainsKey(x.Name) && x is IPipelineTaskStep)
                .Cast<IPipelineTaskStep>())
            {
                var pipelineReports = _pipelineReportsGenerator.Generate(pipeline, log);

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
