using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Manisero.Navvy.Reporting.Utils;

namespace Manisero.Navvy.Reporting.Shared
{
    internal interface IReportsFormatter
    {
        IEnumerable<TaskExecutionReport> Format(
            object reportData,
            Type templatesNamespaceMarker,
            Func<string, string> reportNameGetter,
            string reportDataJsonToken);
    }

    internal class ReportsFormatter : IReportsFormatter
    {
        private struct ReportTemplate
        {
            public string FileName { get; set; }
            public string Body { get; set; }
        }

        /// <summary>Templates namespace marker -> templates</summary>
        private readonly ConcurrentDictionary<Type, ICollection<ReportTemplate>> _templates =
            new ConcurrentDictionary<Type, ICollection<ReportTemplate>>();

        public IEnumerable<TaskExecutionReport> Format(
            object reportData,
            Type templatesNamespaceMarker,
            Func<string, string> reportNameGetter,
            string reportDataJsonToken)
        {
            var reportTemplates = _templates.GetOrAdd(
                templatesNamespaceMarker,
                x => GetReportTemplates(x).ToArray());

            var reportDataJson = reportData.ToJson();

            foreach (var template in reportTemplates)
            {
                yield return new TaskExecutionReport(
                    reportNameGetter(template.FileName),
                    template.Body.Replace(reportDataJsonToken, reportDataJson));
            }
        }

        private static IEnumerable<ReportTemplate> GetReportTemplates(
            Type templatesNamespaceMarker)
        {
            var templatesAssembly = templatesNamespaceMarker.Assembly;
            var templateResourceNamePrefix = templatesNamespaceMarker.Namespace + ".";

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
