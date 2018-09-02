using System;
using System.Diagnostics;
using System.Threading;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.PipelineProcessing.Events;

namespace Manisero.Navvy.PipelineProcessing
{
    internal class SequentialPipelineStepExecutor<TItem> : ITaskStepExecutor<PipelineTaskStep<TItem>>
    {
        public void Execute(
            PipelineTaskStep<TItem> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var itemNumber = 0;
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            var itemSw = new Stopwatch();
            var blockSw = new Stopwatch();

            foreach (var item in step.Input)
            {
                itemNumber++;
                events?.OnItemStarted(itemNumber, item, step, context.Task);
                itemSw.Restart();

                foreach (var block in step.Blocks)
                {
                    events?.OnBlockStarted(block, itemNumber, item, step, context.Task);
                    blockSw.Restart();

                    try
                    {
                        block.Body(item);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    blockSw.Stop();
                    events?.OnBlockEnded(block, itemNumber, item, step, context.Task, blockSw.Elapsed);
                    cancellation.ThrowIfCancellationRequested();
                }

                itemSw.Stop();
                events?.OnItemEnded(itemNumber, item, step, context.Task, itemSw.Elapsed);
                PipelineProcessingUtils.ReportProgress(itemNumber, step.ExpectedItemsCount, progress);
            }
        }
    }
}
