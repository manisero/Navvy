using System;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.PipelineProcessing
{
    internal static class PipelineProcessingUtils
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
