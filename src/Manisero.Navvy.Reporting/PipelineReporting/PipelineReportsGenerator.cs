using System.Collections.Generic;
using Manisero.Navvy.Reporting.PipelineReporting.Templates;
using Manisero.Navvy.Reporting.Shared;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal interface IPipelineReportsGenerator
    {
        IEnumerable<TaskExecutionReport> Generate(
            PipelineReportData data);
    }

    internal class PipelineReportsGenerator : IPipelineReportsGenerator
    {
        public const string ReportDataJsonToken = "@ReportDataJson";

        private readonly IReportsFormatter _reportsFormatter;

        public PipelineReportsGenerator(
            IReportsFormatter reportsFormatter)
        {
            _reportsFormatter = reportsFormatter;
        }

        public IEnumerable<TaskExecutionReport> Generate(
            PipelineReportData data)
        {
            return _reportsFormatter.Format(
                data,
                typeof(PipelineReportingTemplatesNamespaceMarker),
                x => $"{data.PipelineName}_{x}",
                ReportDataJsonToken);
        }
    }
}
