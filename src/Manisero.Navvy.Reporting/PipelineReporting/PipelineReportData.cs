using System.Collections.Generic;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal class PipelineReportData
    {
        public string PipelineName { get; set; }

        public ICollection<ICollection<object>> ItemTimesData { get; set; }

        public ICollection<ICollection<object>> BlockTimesData { get; set; }

        public ICollection<ICollection<object>> MemoryData { get; set; }
    }
}
