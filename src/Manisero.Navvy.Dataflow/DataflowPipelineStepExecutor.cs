using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Manisero.Navvy.Core.Events;
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
            CancellationToken cancellation)
        {
            var items = step.Input.ItemsFactory();
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            var dataflowExecutionContext = new DataflowExecutionContext
            {
                StepContext = context,
                ExpectedItemsCount = items.ExpectedCount,
                TaskEvents = context.EventsBag.TryGetEvents<TaskExecutionEvents>(),
                Events = events,
                TotalInputMaterializationDuration = TimeSpan.Zero,
                TotalBlockDurations = new ConcurrentDictionary<string, TimeSpan>(
                    step.Blocks.Select(x => x.Name).Distinct().Select(
                        x => new KeyValuePair<string, TimeSpan>(x, TimeSpan.Zero)))
            };
            
            using (var inputEnumerator = items.Items.GetEnumerator())
            {
                var pipeline = _dataflowPipelineBuilder.Build(
                    inputEnumerator,
                    step,
                    dataflowExecutionContext,
                    cancellation);
                
                try
                {
                    pipeline.Execute().Wait();
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

            events?.Raise(x => x.OnPipelineEnded(dataflowExecutionContext.TotalInputMaterializationDuration, dataflowExecutionContext.TotalBlockDurations, step, context.Task));
        }
    }
}
