using System;
using System.Collections.Generic;

namespace Manisero.Navvy.Reporting
{
    public static class TaskExecutionReportingUtils
    {
        public const string TaskExecutionReportsExtraKey = "ExecutionReports";
        public const string TaskExecutionReportsFolderPathExtraKey = "ExecutionReportsPath";

        internal static void SetExecutionReports(
            this TaskDefinition task,
            IReadOnlyCollection<TaskExecutionReport> reports)
            => task.Extras.Set(TaskExecutionReportsExtraKey, reports);

        public static IReadOnlyCollection<TaskExecutionReport> GetExecutionReports(
            this TaskDefinition task)
            => task.Extras.Get<IReadOnlyCollection<TaskExecutionReport>>(TaskExecutionReportsExtraKey);

        internal static void SetExecutionReportsPath(
            this TaskDefinition task,
            string path)
            => task.Extras.Set(TaskExecutionReportsFolderPathExtraKey, path);

        public static string GetExecutionReportsPath(
            this TaskDefinition task)
            => task.Extras.Get<string>(TaskExecutionReportsFolderPathExtraKey);

        /// <param name="reportsFolderPathFactory">If specified, generated reports will be saved in given folder.</param>
        public static ITaskExecutorBuilder UseTaskExecutionReporter(
            this ITaskExecutorBuilder builder,
            Func<TaskDefinition, string> reportsFolderPathFactory = null)
            => builder.RegisterEvents(TaskExecutionReporter.CreateEvents(reportsFolderPathFactory));
    }
}
