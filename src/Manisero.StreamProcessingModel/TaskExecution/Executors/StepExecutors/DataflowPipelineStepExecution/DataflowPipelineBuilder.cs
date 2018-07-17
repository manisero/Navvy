using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Manisero.StreamProcessingModel.TaskExecution.Models.TaskSteps;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors.StepExecutors.DataflowPipelineStepExecution
{
    public class DataflowPipelineBuilder<TData>
    {
        private static readonly int DegreeOfParallelism = Environment.ProcessorCount - 1;

        public Tuple<ITargetBlock<DataBatch<TData>>, Task> Build(PipelineTaskStep<TData> step)
        {
            var firstBlock = ToTransformBlock(step.Blocks.First());

            if (step.Blocks.Count == 1)
            {
                return new Tuple<ITargetBlock<DataBatch<TData>>, Task>(firstBlock, firstBlock.Completion);
            }

            var previousBlock = firstBlock;

            for (var i = 1; i < step.Blocks.Count - 1; i++)
            {
                var currentBlock = ToTransformBlock(step.Blocks[i]);
                previousBlock.LinkTo(currentBlock, new DataflowLinkOptions { PropagateCompletion = true });

                previousBlock = currentBlock;
            }

            var lastBlock = ToActionBlock(step.Blocks.Last());
            previousBlock.LinkTo(lastBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return new Tuple<ITargetBlock<DataBatch<TData>>, Task>(firstBlock, lastBlock.Completion);
        }

        private TransformBlock<DataBatch<TData>, DataBatch<TData>> ToTransformBlock(PipelineBlock<TData> block)
        {
            var options = GetDataflowOptions(block);

            return new TransformBlock<DataBatch<TData>, DataBatch<TData>>(
                x =>
                {
                    foreach (var data in x.Data)
                    {
                        block.Body(data);
                    }

                    return x;
                },
                options);
        }

        private ActionBlock<DataBatch<TData>> ToActionBlock(PipelineBlock<TData> block)
        {
            var options = GetDataflowOptions(block);

            return new ActionBlock<DataBatch<TData>>(
                x =>
                {
                    foreach (var data in x.Data)
                    {
                        block.Body(data);
                    }
                },
                options);
        }

        private ExecutionDataflowBlockOptions GetDataflowOptions(PipelineBlock<TData> block)
        {
            var maxDegreeOfParallelism = block.Parallel ? DegreeOfParallelism : 1;

            return new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism,
                BoundedCapacity = maxDegreeOfParallelism,
                EnsureOrdered = true
            };
        }
    }
}
