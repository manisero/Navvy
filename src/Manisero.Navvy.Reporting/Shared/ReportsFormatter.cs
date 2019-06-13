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
        /// <param name="reportNameGetter">Template file name => report name</param>
        IEnumerable<TaskExecutionReport> Format(
            object reportData,
            Type templatesNamespaceMarker,
            string reportDataJsonToken,
            Func<string, string> reportNameGetter);
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
            string reportDataJsonToken,
            Func<string, string> reportNameGetter)
        {
            var reportTemplates = _templates.GetOrAdd(
                templatesNamespaceMarker,
                x => GetReportTemplates(x));

            var reportDataJson = reportData.ToJson();

            foreach (var template in reportTemplates)
            {
                yield return new TaskExecutionReport(
                    reportNameGetter(template.FileName),
                    template.Body.Replace(reportDataJsonToken, reportDataJson));
            }
        }

        private static ICollection<ReportTemplate> GetReportTemplates(
            Type templatesNamespaceMarker)
        {
            var templatesAssembly = templatesNamespaceMarker.Assembly;
            var templateResourceNamePrefix = templatesNamespaceMarker.Namespace + ".";

            var templateResourceNames = templatesAssembly
                .GetManifestResourceNames()
                .Where(x => x.StartsWith(templateResourceNamePrefix));

            var result = new List<ReportTemplate>();

            foreach (var resourceName in templateResourceNames)
            {
                using (var resourceStream = templatesAssembly.GetManifestResourceStream(resourceName))
                using (var reader = new StreamReader(resourceStream))
                {
                    result.Add(new ReportTemplate
                    {
                        FileName = resourceName.Substring(templateResourceNamePrefix.Length),
                        Body = reader.ReadToEnd()
                    });
                }
            }

            return result;
        }
    }
}
