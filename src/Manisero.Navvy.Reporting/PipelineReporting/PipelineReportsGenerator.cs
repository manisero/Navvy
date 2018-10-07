using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Logging;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Reporting.PipelineReporting.Templates;
using Manisero.Navvy.Reporting.Shared;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal interface IPipelineReportsGenerator
    {
        ICollection<TaskExecutionReport> Generate(
            IPipelineTaskStep pipeline,
            TaskExecutionLog log);
    }

    internal class PipelineReportsGenerator : IPipelineReportsGenerator
    {
        public const string ReportDataJsonToken = "@ReportDataJson";

        private readonly IPipelineReportDataExtractor _pipelineReportDataExtractor;
        private readonly IReportsFormatter _reportsFormatter;

        public PipelineReportsGenerator(
            IPipelineReportDataExtractor pipelineReportDataExtractor,
            IReportsFormatter reportsFormatter)
        {
            _pipelineReportDataExtractor = pipelineReportDataExtractor;
            _reportsFormatter = reportsFormatter;
        }

        public ICollection<TaskExecutionReport> Generate(
            IPipelineTaskStep pipeline,
            TaskExecutionLog log)
        {
            var data = _pipelineReportDataExtractor.Extract(pipeline, log);

            return _reportsFormatter.Format(
                    data,
                    typeof(PipelineReportingTemplatesNamespaceMarker),
                    ReportDataJsonToken,
                    x => $"{pipeline.Name}_{x}")
                .ToArray();
        }
    }
}
