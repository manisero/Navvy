using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.PipelineExecutors.Dataflow
{
    public class DataflowPipelineBuilder
    {
        private static readonly MethodInfo AppendTransformBlockMethod =
            typeof(DataflowPipelineBuilder).GetMethod(nameof(AppendTransformBlock),
                                                       BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo IgnoreOutputMethod =
            typeof(DataflowPipelineBuilder).GetMethod(nameof(IgnoreOutput),
                                                       BindingFlags.Instance | BindingFlags.NonPublic);

        private const int DegreeOfParallelism = 4;

        public DataflowPipeline<TInput> Build<TInput>(Pipeline<TInput> pipeline)
        {
            var completions = new List<Task>();

            var firstBlock = pipeline.Blocks.First();

            var firstTransformBlock = AppendTransformBlockMethod
                .InvokeAsGeneric<ITargetBlock<TInput>>(this,
                                                       new[] { firstBlock.InputType, firstBlock.OutputType },
                                                       firstBlock, null);

            var firstCompletion = firstBlock.OnCompleted != null
                ? firstTransformBlock.Completion.ContinueWith(_ => firstBlock.OnCompleted())
                : firstTransformBlock.Completion;

            completions.Add(firstCompletion);

            IDataflowBlock currentBlock = firstTransformBlock;

            foreach (var block in pipeline.Blocks.Skip(1))
            {
                currentBlock = AppendTransformBlockMethod
                    .InvokeAsGeneric<IDataflowBlock>(this,
                                                     new[] { block.InputType, block.OutputType },
                                                     block, currentBlock);

                var completion = block.OnCompleted != null
                    ? currentBlock.Completion.ContinueWith(_ => block.OnCompleted())
                    : currentBlock.Completion;

                completions.Add(completion);
            }

            IgnoreOutputMethod
                .InvokeAsGeneric<IDisposable>(this,
                                              new[] { pipeline.Blocks.Last().OutputType },
                                              currentBlock);

            var globalCompletion = Task.WhenAll(completions);

            return new DataflowPipeline<TInput>(pipeline.Input, firstTransformBlock, globalCompletion);
        }

        private TransformBlock<TInput, TOutput> AppendTransformBlock<TInput, TOutput>(
            Block<TInput, TOutput> block,
            ISourceBlock<TInput> previousBlock)
        {
            var maxDegreeOfParallelism = block.Parallel ? DegreeOfParallelism : 1;

            var transformBlock = new TransformBlock<TInput, TOutput>(block.Body,
                                                                     new ExecutionDataflowBlockOptions
                                                                     {
                                                                         MaxDegreeOfParallelism = maxDegreeOfParallelism,
                                                                         BoundedCapacity = maxDegreeOfParallelism,
                                                                         EnsureOrdered = true
                                                                     });

            previousBlock?.LinkTo(transformBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return transformBlock;
        }

        private IDisposable IgnoreOutput<TOutput>(
            ISourceBlock<TOutput> sourceBlock)
        {
            var terminateBlock = DataflowBlock.NullTarget<TOutput>(); // Never completes

            return sourceBlock.LinkTo(terminateBlock, new DataflowLinkOptions { PropagateCompletion = true });
        }
    }
}
