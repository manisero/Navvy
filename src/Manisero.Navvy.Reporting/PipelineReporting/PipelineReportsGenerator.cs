using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manisero.Navvy.Reporting.PipelineReporting.Templates;
using Manisero.Navvy.Reporting.Utils;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal interface IPipelineReportsGenerator
    {
        IEnumerable<TaskExecutionReport> Generate(
            PipelineReportData data);
    }

    internal class PipelineReportsGenerator : IPipelineReportsGenerator
    {
        private struct ReportTemplate
        {
            public string FileName { get; set; }
            public string Body { get; set; }
        }

        public const string ReportDataJsonToken = "@ReportDataJson";

        private static readonly Lazy<ICollection<ReportTemplate>> ReportTemplates =
            new Lazy<ICollection<ReportTemplate>>(() => GetReportTemplates().ToArray());

        public IEnumerable<TaskExecutionReport> Generate(
            PipelineReportData data)
        {
            var reportDataJson = data.ToJson();

            foreach (var template in ReportTemplates.Value)
            {
                yield return new TaskExecutionReport(
                    $"{data.PipelineName}_{template.FileName}",
                    template.Body.Replace(ReportDataJsonToken, reportDataJson));
            }
        }

        private static IEnumerable<ReportTemplate> GetReportTemplates()
        {
            var templatesAssembly = typeof(TemplatesNamespaceMarker).Assembly;
            var templateResourceNamePrefix = typeof(TemplatesNamespaceMarker).Namespace + ".";

            var templateResourceNames = templatesAssembly
                .GetManifestResourceNames()
                .Where(x => x.StartsWith(templateResourceNamePrefix));

            foreach (var resourceName in templateResourceNames)
            {
                using (var resourceStream = templatesAssembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(resourceStream))
                {
                    yield return new ReportTemplate
                    {
                        FileName = resourceName.Substring(templateResourceNamePrefix.Length),
                        Body = reader.ReadToEnd()
                    };
                }
            }
        }
    }
}
