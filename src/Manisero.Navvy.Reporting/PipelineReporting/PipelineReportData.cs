using System.Collections.Generic;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal class PipelineReportData
    {
        public ICollection<ICollection<object>> ItemsTimelineData { get; set; }

        public ICollection<ICollection<object>> BlockTimesData { get; set; }

        public ICollection<ICollection<object>> MemoryData { get; set; }

        public ICollection<ICollection<object>> CpuUsageData { get; set; }
    }
}
