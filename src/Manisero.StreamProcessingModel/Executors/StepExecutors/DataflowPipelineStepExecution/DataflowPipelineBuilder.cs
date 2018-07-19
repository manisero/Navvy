using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Manisero.StreamProcessingModel.Models.TaskSteps;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.Executors.StepExecutors.DataflowPipelineStepExecution
{
    public class DataflowPipeline<TData>
    {
        public ITargetBlock<DataBatch<TData>> InputBlock { get; set; }
        public Task Completion { get; set; }
    }

    public class DataflowPipelineBuilder<TData>
    {
        private static readonly int DegreeOfParallelism = Environment.ProcessorCount - 1;

        public DataflowPipeline<TData> Build(
            PipelineTaskStep<TData> step,
            IProgress<byte> progress)
        {
            var firstBlock = ToTransformBlock(step.Blocks.First());
            var previousBlock = firstBlock;

            for (var i = 1; i <= step.Blocks.Count - 1; i++)
            {
                var currentBlock = ToTransformBlock(step.Blocks[i]);
                previousBlock.LinkTo(currentBlock, new DataflowLinkOptions { PropagateCompletion = true });

                previousBlock = currentBlock;
            }

            var progressBlock = CreateProgressBlock(step, progress);
            previousBlock.LinkTo(progressBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return new DataflowPipeline<TData>
            {
                InputBlock = firstBlock,
                Completion = progressBlock.Completion
            };
        }

        private TransformBlock<DataBatch<TData>, DataBatch<TData>> ToTransformBlock(PipelineBlock<TData> block)
        {
            var maxDegreeOfParallelism = block.Parallel ? DegreeOfParallelism : 1;

            return new TransformBlock<DataBatch<TData>, DataBatch<TData>>(
                x =>
                {
                    foreach (var data in x.Data)
                    {
                        block.Body(data);
                    }

                    return x;
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    BoundedCapacity = maxDegreeOfParallelism,
                    EnsureOrdered = true
                });
        }

        private ActionBlock<DataBatch<TData>> CreateProgressBlock(
            PipelineTaskStep<TData> step,
            IProgress<byte> progress)
        {
            var batchNumber = 0;

            return new ActionBlock<DataBatch<TData>>(
                x =>
                {
                    batchNumber++;
                    StepExecutionUtils.ReportProgress(batchNumber, step.ExpectedInputBatchesCount, progress);
                },
                new ExecutionDataflowBlockOptions
                {
                    BoundedCapacity = 1,
                    EnsureOrdered = true
                });
        }
    }
}
