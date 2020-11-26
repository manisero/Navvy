using System.Linq;
using Manisero.Navvy.Core;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Logging.Diagnostics;
using Manisero.Navvy.PipelineProcessing.Events;

namespace Manisero.Navvy.Logging
{
    public static class TaskExecutionLogger
    {
        public static IExecutionEvents[] CreateEvents()
        {
            return new IExecutionEvents[]
            {
                new TaskExecutionEvents(
                    taskStarted: HandleTaskStarted,
                    taskEnded: HandleTaskEnded,
                    stepStarted: HandleStepStarted,
                    stepEnded: HandleStepEnded),
                new PipelineExecutionEvents(
                    itemMaterialized: HandleItemMaterialized,
                    itemEnded: HandleItemEnded,
                    blockStarted: HandleBlockStarted,
                    blockEnded: HandleBlockEnded,
                    pipelineEnded: HandlePipelineEnded)
            };
        }

        private static void HandleTaskStarted(
            TaskStartedEvent e)
        {
            var diagnostic = DiagnosticsProvider.GetFirstDiagnostic();

            var log = new TaskExecutionLog(diagnostic);
            log.TaskDuration.SetStart(e.Timestamp);

            e.Task.SetExecutionLog(log);
        }

        private static void HandleTaskEnded(
            TaskEndedEvent e)
        {
            var log = e.Task.GetExecutionLog();
            log.TaskDuration.SetEnd(e.Timestamp, e.Duration);
            AddDiagnostic(log);
        }

        private static void HandleStepStarted(
            StepStartedEvent e)
        {
            var stepLog = new TaskStepLog();
            stepLog.Duration.SetStart(e.Timestamp);

            var log = e.Task.GetExecutionLog();
            log.StepLogs[e.Step.Name] = stepLog;
            AddDiagnostic(log);
        }

        private static void HandleStepEnded(
            StepEndedEvent e)
        {
            var log = e.Task.GetExecutionLog();
            log.StepLogs[e.Step.Name].Duration.SetEnd(e.Timestamp, e.Duration);
            AddDiagnostic(log);
        }

        private static void HandleItemMaterialized(
            ItemMaterializedEvent e)
        {
            var itemLog = new PipelineItemLog();
            itemLog.Duration.SetStart(e.ItemStartTimestamp);
            itemLog.MaterializationDuration = e.MaterializationDuration;

            var log = e.Task.GetExecutionLog();
            log.StepLogs[e.Step.Name].ItemLogs[e.ItemNumber] = itemLog;
            AddDiagnostic(log);
        }

        private static void HandleItemEnded(
            ItemEndedEvent e)
        {
            var log = e.Task.GetExecutionLog();
            log.StepLogs[e.Step.Name].ItemLogs[e.ItemNumber].Duration.SetEnd(e.Timestamp, e.Duration);
            AddDiagnostic(log);
        }

        private static void HandleBlockStarted(
            BlockStartedEvent e)
        {
            var blockLog = new DurationLog();
            blockLog.SetStart(e.Timestamp);

            var log = e.Task.GetExecutionLog();
            log.StepLogs[e.Step.Name].ItemLogs[e.ItemNumber].BlockDurations[e.Block.Name] = blockLog;
            AddDiagnostic(log);
        }

        private static void HandleBlockEnded(
            BlockEndedEvent e)
        {
            var log = e.Task.GetExecutionLog();
            log.StepLogs[e.Step.Name].ItemLogs[e.ItemNumber].BlockDurations[e.Block.Name].SetEnd(e.Timestamp, e.Duration);
            AddDiagnostic(log);
        }

        private static void HandlePipelineEnded(
            PipelineEndedEvent e)
        {
            var log = e.Task.GetExecutionLog();
            log.StepLogs[e.Step.Name].BlockTotals = new PipelineBlockTotalsLog
            {
                MaterializationDuration = e.TotalInputMaterializationDuration,
                BlockDurations = e.TotalBlockDurations.ToDictionary(entry => entry.Key, entry => entry.Value)
            };

            AddDiagnostic(log);
        }

        private static void AddDiagnostic(
            TaskExecutionLog log)
        {
            var diagnostic = DiagnosticsProvider.GetDiagnostic(log.LatestDiagnostic);
            log.AddDiagnostic(diagnostic);
        }
    }
}
