using System;
using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Logging;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Reporting.Utils;

namespace Manisero.Navvy.Reporting.PipelineReporting
{
    internal interface IPipelineReportDataExtractor
    {
        PipelineReportData Extract(
            IPipelineTaskStep pipeline,
            TaskExecutionLog log);
    }

    internal class PipelineReportDataExtractor : IPipelineReportDataExtractor
    {
        private const string MsUnit = " [ms]";
        private const string MbUnit = " [mb]";
        private const string MaterializationBlockName = "Materialization";

        public PipelineReportData Extract(
            IPipelineTaskStep pipeline,
            TaskExecutionLog log)
        {
            var stepLog = log.StepLogs[pipeline.Name];
            var blockNames = pipeline.Blocks.Select(x => x.Name).ToArray();

            return new PipelineReportData
            {
                ItemTimesData = GetItemTimesData(stepLog, blockNames).ToArray(),
                BlockTimesData = GetBlockTimesData(stepLog, blockNames).ToArray(),
                MemoryData = GetMemoryData(stepLog.Duration, log.Diagnostics).ToArray()
            };
        }

        private IEnumerable<ICollection<object>> GetItemTimesData(
            TaskStepLog stepLog,
            ICollection<string> blockNames)
        {
            var headerRow = new[] { "Item", "Step", "StartTs", "EndTs" };

            var dataRows = stepLog
                .ItemLogs
                .Select(x => GetItemTimesItemRows(x.Key, x.Value, blockNames, stepLog.Duration.StartTs))
                .SelectMany(rows => rows);

            return headerRow.ToEnumerable().Concat(dataRows);
        }

        private IEnumerable<ICollection<object>> GetItemTimesItemRows(
            int itemNumber,
            PipelineItemLog itemLog,
            ICollection<string> blockNames,
            DateTime stepStartTs)
        {
            var itemNumberString = $"Item {itemNumber}";

            yield return new[]
            {
                itemNumberString,
                MaterializationBlockName,
                (itemLog.Duration.StartTs - stepStartTs).GetLogValue(),
                (itemLog.Duration.StartTs + itemLog.MaterializationDuration - stepStartTs).GetLogValue()
            };

            foreach (var blockName in blockNames)
            {
                var blockDuration = itemLog.BlockDurations[blockName];

                yield return new[]
                {
                    itemNumberString,
                    blockName,
                    (blockDuration.StartTs - stepStartTs).GetLogValue(),
                    (blockDuration.EndTs - stepStartTs).GetLogValue()
                };
            }
        }

        private IEnumerable<ICollection<object>> GetBlockTimesData(
            TaskStepLog stepLog,
            ICollection<string> blockNames)
        {
            yield return new[] { "Step", "Total duration" + MsUnit };
            yield return new[] { MaterializationBlockName, stepLog.BlockTotals.MaterializationDuration.GetLogValue() };

            foreach (var blockName in blockNames)
            {
                yield return new[] { blockName, stepLog.BlockTotals.BlockDurations[blockName].GetLogValue() };
            }
        }

        private IEnumerable<ICollection<object>> GetMemoryData(
            DurationLog stepDuration,
            IEnumerable<DiagnosticLog> diagnostics)
        {
            yield return new[] { "Time" + MsUnit, "Process working set" + MbUnit, "GC allocated set" + MbUnit };

            var relevantDiagnostics = diagnostics
                .Where(x => x.Timestamp.IsBetween(stepDuration.StartTs, stepDuration.EndTs))
                .OrderBy(x => x.Timestamp);

            foreach (var diagnostic in relevantDiagnostics)
            {
                yield return new[]
                {
                    (diagnostic.Timestamp - stepDuration.StartTs).GetLogValue(),
                    diagnostic.ProcessWorkingSet.ToMb(),
                    diagnostic.GcAllocatedSet.ToMb()
                };
            }
        }
    }
}
