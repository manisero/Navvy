using System;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.PipelineProcessing
{
    public static class PipelineProcessingUtils
    {
        public static void ReportProgress(
            int itemNumber,
            int expectedItemsCount,
            IProgress<byte> progress)
        {
            progress.Report(
                itemNumber < expectedItemsCount
                    ? itemNumber.ToPercentageOf(expectedItemsCount)
                    : (byte)100);
        }
    }
}
