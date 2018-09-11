using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            var dataflowExecutionContext = new DataflowExecutionContext
            {
                StepContext = context,
                Events = events,
                TotalBlockDurations = new ConcurrentDictionary<string, TimeSpan>(
                    step.Blocks.Select(x => x.Name).Distinct().Select(
                        x => new KeyValuePair<string, TimeSpan>(x, TimeSpan.Zero)))
            };
            
            var pipeline = _dataflowPipelineBuilder.Build(step, dataflowExecutionContext, progress, cancellation);
            var itemNumber = 0;
            var totalInputMaterializationDuration = TimeSpan.Zero;

            using (var inputEnumerator = step.Input.Input.GetEnumerator())
            {
                while (true)
                {
                    var sw = Stopwatch.StartNew();

                    if (!inputEnumerator.MoveNext())
                    {
                        sw.Stop();
                        break;
                    }

                    var pipelineItem = new PipelineItem<TItem>
                    {
                        Number = ++itemNumber,
                        Item = inputEnumerator.Current,
                        ProcessingStopwatch = sw
                    };

                    var materializationDuration = sw.Elapsed;
                    totalInputMaterializationDuration += materializationDuration;
                    events?.OnItemStarted(pipelineItem.Number, pipelineItem.Item, materializationDuration, step, context.Task);

                    pipeline.InputBlock.SendAsync(pipelineItem).Wait();

                    cancellation.ThrowIfCancellationRequested();
                }
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

            events?.OnPipelineEnded(totalInputMaterializationDuration, dataflowExecutionContext.TotalBlockDurations, step, context.Task);
        }
    }
}
