using System;
using System.Diagnostics;
using System.Threading;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.PipelineProcessing.Events;

namespace Manisero.Navvy.PipelineProcessing
{
    internal class SequentialPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        public void Execute(
            PipelineTaskStep<TData> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var batchNumber = 0;
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            foreach (var input in step.Input)
            {
                batchNumber++;
                events?.OnBatchStarted(batchNumber, input, step, context.Task);
                var batchSw = Stopwatch.StartNew();

                foreach (var block in step.Blocks)
                {
                    events?.OnBlockStarted(block, batchNumber, input, step, context.Task);
                    var blockSw = Stopwatch.StartNew();

                    try
                    {
                        block.Body(input);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    blockSw.Stop();
                    events?.OnBlockEnded(block, batchNumber, input, step, context.Task, blockSw.Elapsed);
                    cancellation.ThrowIfCancellationRequested();
                }

                batchSw.Stop();
                events?.OnBatchEnded(batchNumber, input, step, context.Task, batchSw.Elapsed);
                PipelineProcessingUtils.ReportProgress(batchNumber, step.ExpectedInputBatchesCount, progress);
            }
        }
    }
}
