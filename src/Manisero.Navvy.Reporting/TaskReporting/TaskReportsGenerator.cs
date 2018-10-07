using System.Collections.Generic;
using Manisero.Navvy.Logging;

namespace Manisero.Navvy.Reporting.TaskReporting
{
    internal interface ITaskReportsGenerator
    {
        IEnumerable<TaskExecutionReport> Generate(
            TaskExecutionLog log);
    }

    internal class TaskReportsGenerator : ITaskReportsGenerator
    {
        public IEnumerable<TaskExecutionReport> Generate(
            TaskExecutionLog log)
        {
            var data = ExtractReportData(log);

            yield break;
        }

        private TaskReportData ExtractReportData(
            TaskExecutionLog log)
        {
            return new TaskReportData();
        }
    }
}
