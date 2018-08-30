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
    internal class DataflowPipelineStepExecutor<TData> : ITaskStepExecutor<PipelineTaskStep<TData>>
    {
        private readonly DataflowPipelineBuilder<TData> _dataflowPipelineBuilder = new DataflowPipelineBuilder<TData>();

        public void Execute(
            PipelineTaskStep<TData> step,
            TaskStepExecutionContext context,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var pipeline = _dataflowPipelineBuilder.Build(step, context, progress, cancellation);
            var batchNumber = 0;
            var events = context.EventsBag.TryGetEvents<PipelineExecutionEvents>();

            foreach (var input in step.Input)
            {
                batchNumber++;
                events?.OnBatchStarted(batchNumber, input, step, context.Task);

                var batch = new DataBatch<TData>
                {
                    Number = batchNumber,
                    Data = input,
                    ProcessingStopwatch = Stopwatch.StartNew()
                };

                pipeline.InputBlock.SendAsync(batch).Wait();
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
