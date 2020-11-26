using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Manisero.Navvy.Logging.Diagnostics;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.Logging
{
    public class TaskExecutionLog
    {
        public DurationLog TaskDuration { get; } = new DurationLog();

        /// <summary>StepName -> Log</summary>
        public ConcurrentDictionary<string, TaskStepLog> StepLogs { get; } = new ConcurrentDictionary<string, TaskStepLog>();

        public ConcurrentBag<Diagnostic> Diagnostics { get; } = new ConcurrentBag<Diagnostic>();

        /// <summary>Note: wrapped in reference type to ensure atomic access to Diagnostic.</summary>
        private Wrapper<Diagnostic> LatestDiagnostic { get; set; }

        public TaskExecutionLog(
            Diagnostic firstDiagnostic)
        {
            AddDiagnostic(firstDiagnostic);
        }

        public void AddDiagnostic(
            Diagnostic diagnostic)
        {
            Diagnostics.Add(diagnostic);
            LatestDiagnostic = new Wrapper<Diagnostic>(diagnostic);
        }

        public Diagnostic GetLatestDiagnostic() => LatestDiagnostic.Wrapped;
    }

    public class TaskStepLog
    {
        public DurationLog Duration { get; } = new DurationLog();

        /// <summary>ItemNumber -> Log</summary>
        public ConcurrentDictionary<int, PipelineItemLog> ItemLogs { get; } = new ConcurrentDictionary<int, PipelineItemLog>();

        public PipelineBlockTotalsLog BlockTotals { get; set; }
    }

    public class PipelineItemLog
    {
        public DurationLog Duration { get; } = new DurationLog();

        public TimeSpan MaterializationDuration { get; set; }

        public ConcurrentDictionary<string, DurationLog> BlockDurations { get; } = new ConcurrentDictionary<string, DurationLog>();
    }

    public class PipelineBlockTotalsLog
    {
        public TimeSpan MaterializationDuration { get; set; }

        public Dictionary<string, TimeSpan> BlockDurations { get; set; }
    }

    public class DurationLog
    {
        public DateTime StartTs { get; private set; }

        public DateTime EndTs { get; private set; }

        public TimeSpan Duration { get; private set; }

        public void SetStart(
            DateTime startTs)
        {
            StartTs = startTs;
        }

        public void SetEnd(
            DateTime endTs,
            TimeSpan duration)
        {
            EndTs = endTs;
            Duration = duration;
        }
    }
}
