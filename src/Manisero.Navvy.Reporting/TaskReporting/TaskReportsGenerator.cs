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

        private readonly IReportsFormatter _reportsFormatter;

        public TaskReportsGenerator(
            IReportsFormatter reportsFormatter)
        {
            _reportsFormatter = reportsFormatter;
        }

        public ICollection<TaskExecutionReport> Generate(
            TaskExecutionLog log)
        {
            var data = ExtractReportData(log);

            return _reportsFormatter.Format(
                    data,
                    typeof(TaskReportingTemplatesNamespaceMarker),
                    ReportDataJsonToken,
                    x => x)
                .ToArray();
        }

        private TaskReportData ExtractReportData(
            TaskExecutionLog log)
        {
            return new TaskReportData();
        }
    }
}
