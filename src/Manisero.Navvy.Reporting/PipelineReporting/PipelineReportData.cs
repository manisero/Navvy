using System.Collections.Generic;
using Manisero.Navvy.Logging.Diagnostics;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal class PipelineReportData
    {
        public GlobalDiagnostic GlobalData { get; set; }

        public ICollection<ICollection<object>> ItemsTimelineData { get; set; }

        public ICollection<ICollection<object>> BlockTimesData { get; set; }

        public ICollection<ICollection<object>> MemoryData { get; set; }

        public ICollection<ICollection<object>> CpuUsageData { get; set; }
    }
}
