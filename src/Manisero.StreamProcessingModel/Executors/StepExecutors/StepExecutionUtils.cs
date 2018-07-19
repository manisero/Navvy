using System;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors
{
    public static class StepExecutionUtils
    {
        public static void ReportProgress(
            int batchNumber,
            int expectedPipelineInputBatchesCount,
            IProgress<byte> progress)
        {
            progress.Report(
                batchNumber < expectedPipelineInputBatchesCount
                    ? batchNumber.ToPercentageOf(expectedPipelineInputBatchesCount)
                    : (byte)100);
        }
    }
}
