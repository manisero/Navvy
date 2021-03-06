﻿using System;
using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Logging;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Models;
using Manisero.Navvy.Reporting.Shared;
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
        public PipelineReportData Extract(
            IPipelineTaskStep pipeline,
            TaskExecutionLog log)
        {
            var stepLog = log.StepLogs[pipeline.Name];
            var materializationBlockName = pipeline.Input.Name ?? PipelineInput.DefaultName;
            var blockNames = pipeline.Blocks.Select(x => x.Name).ToArray();

            var diagnosticChartsData = DiagnosticDataExtractor.Extract(
                log.DiagnosticsLog.Diagnostics,
                stepLog.Duration);

            return new PipelineReportData
            {
                GlobalData = log.DiagnosticsLog.GlobalDiagnostic,
                ItemsTimelineData = GetItemsTimelineData(stepLog, materializationBlockName, blockNames).ToArray(),
                BlockTimesData = GetBlockTimesData(stepLog, materializationBlockName, blockNames).ToArray(),
                MemoryData = diagnosticChartsData.MemoryData,
                CpuUsageData = diagnosticChartsData.CpuUsageData
            };
        }

        private IEnumerable<ICollection<object>> GetItemsTimelineData(
            TaskStepLog stepLog,
            string materializationBlockName,
            IEnumerable<string> blockNames)
        {
            var headerRow = new[] { "Item", "Step", "Start" + PipelineReportingUtils.MsUnit, "End" + PipelineReportingUtils.MsUnit };

            var dataRows = stepLog
                .ItemLogs
                .Select(x => GetItemsTimelineItemRows(x.Key, x.Value, materializationBlockName, blockNames, stepLog.Duration.StartTs))
                .SelectMany(rows => rows);

            return headerRow.ToEnumerable().Concat(dataRows);
        }

        private IEnumerable<ICollection<object>> GetItemsTimelineItemRows(
            int itemNumber,
            PipelineItemLog itemLog,
            string materializationBlockName,
            IEnumerable<string> blockNames,
            DateTime stepStartTs)
        {
            var itemNumberString = $"Item {itemNumber}";

            yield return new object[]
            {
                itemNumberString,
                materializationBlockName,
                (itemLog.Duration.StartTs - stepStartTs).GetLogValue(),
                (itemLog.Duration.StartTs + itemLog.MaterializationDuration - stepStartTs).GetLogValue()
            };

            foreach (var blockName in blockNames)
            {
                var blockDuration = itemLog.BlockDurations[blockName];

                yield return new object[]
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
            string materializationBlockName,
            IEnumerable<string> blockNames)
        {
            yield return new object[] { "Block", "Total duration" + PipelineReportingUtils.MsUnit };
            yield return new object[] { "[Total]", stepLog.Duration.Duration.GetLogValue() };
            yield return new object[] { materializationBlockName, stepLog.BlockTotals.MaterializationDuration.GetLogValue() };

            foreach (var blockName in blockNames)
            {
                yield return new object[] { blockName, stepLog.BlockTotals.BlockDurations[blockName].GetLogValue() };
            }
        }
    }
}
