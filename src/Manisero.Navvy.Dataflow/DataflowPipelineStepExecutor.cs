using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.Dataflow.StepExecution;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Events;

namespace Manisero.Navvy.Dataflow
{
    internal class DataflowPipelineStepExecutor<TItem> : ITaskStepExecutor<PipelineTaskStep<TItem>>
    {
        private readonly DataflowPipelineBuilder _dataflowPipelineBuilder = new DataflowPipelineBuilder();

        public void Execute(
            PipelineTaskStep<TItem> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var pipeline = _dataflowPipelineBuilder.Build(step, context, progress, cancellation);
            var itemNumber = 0;
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            foreach (var item in step.Input)
            {
                itemNumber++;
                events?.OnItemStarted(itemNumber, item, step, context.Task);

                var pipelineItem = new PipelineItem<TItem>
                {
                    Number = itemNumber,
                    Item = item,
                    ProcessingStopwatch = Stopwatch.StartNew()
                };

                pipeline.InputBlock.SendAsync(pipelineItem).Wait();
            }

            pipeline.InputBlock.Complete();

            try
            {
                pipeline.Completion.Wait();
            }
            catch (AggregateException e)
            {
                var flat = e.Flatten();

                if (flat.InnerExceptions.Count == 1)
                {
                    throw flat.InnerExceptions[0];
                }

                throw flat;
            }
        }
    }
}
