using System;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.PipelineProcessing
{
    public static class PipelineProcessingUtils
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
