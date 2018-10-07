using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Logging;
using Manisero.Navvy.Reporting.Shared;
using Manisero.Navvy.Reporting.TaskReporting.Templates;

namespace Manisero.Navvy.Reporting.TaskReporting
{
    internal interface ITaskReportsGenerator
    {
        ICollection<TaskExecutionReport> Generate(
            TaskExecutionLog log);
    }

    internal class TaskReportsGenerator : ITaskReportsGenerator
    {
        public const string ReportDataJsonToken = "@ReportDataJson";

        private readonly ITaskReportDataExtractor _taskReportDataExtractor;
        private readonly IReportsFormatter _reportsFormatter;

        public TaskReportsGenerator(
            ITaskReportDataExtractor taskReportDataExtractor,
            IReportsFormatter reportsFormatter)
        {
            _taskReportDataExtractor = taskReportDataExtractor;
            _reportsFormatter = reportsFormatter;
        }

        public ICollection<TaskExecutionReport> Generate(
            TaskExecutionLog log)
        {
            var data = _taskReportDataExtractor.Extract(log);

            return _reportsFormatter.Format(
                    data,
                    typeof(TaskReportingTemplatesNamespaceMarker),
                    ReportDataJsonToken,
                    x => x)
                .ToArray();
        }
    }
}
