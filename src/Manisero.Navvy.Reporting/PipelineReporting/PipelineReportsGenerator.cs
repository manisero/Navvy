using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Reporting.PipelineReporting.Templates;
using Manisero.Navvy.Reporting.Shared;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal interface IPipelineReportsGenerator
    {
        ICollection<TaskExecutionReport> Generate(
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

        public ICollection<TaskExecutionReport> Generate(
            PipelineReportData data)
        {
            return _reportsFormatter.Format(
                    data,
                    typeof(PipelineReportingTemplatesNamespaceMarker),
                    ReportDataJsonToken,
                    x => $"{data.PipelineName}_{x}")
                .ToArray();
        }
    }
}
