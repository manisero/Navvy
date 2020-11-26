using System.Collections.Generic;

namespace Manisero.Navvy.Reporting.TaskReporting
{
    internal class TaskReportData
    {
        public ICollection<ICollection<object>> StepsTimelineData { get; set; }

        public ICollection<ICollection<object>> StepTimesData { get; set; }

        public ICollection<ICollection<object>> MemoryData { get; set; }

        public ICollection<ICollection<object>> CpuUsageData { get; set; }
    }
}
