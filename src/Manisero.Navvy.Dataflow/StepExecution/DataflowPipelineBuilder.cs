﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Models;

namespace Manisero.Navvy.Dataflow.StepExecution
{
    internal class DataflowPipelineBuilder
    {
        public DataflowPipeline<TItem> Build<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            return step.Blocks.Count != 0
                ? BuildNotEmpty(step, context, cancellation)
                : BuildEmpty(step, context, cancellation);
        }

        private DataflowPipeline<TItem> BuildNotEmpty<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            var firstBlock = ToTransformBlock(step, 0, context, cancellation);
            var previousBlock = firstBlock;

            for (var i = 1; i <= step.Blocks.Count - 1; i++)
            {
                var currentBlock = ToTransformBlock(step, i, context, cancellation);
                previousBlock.LinkTo(currentBlock, new DataflowLinkOptions { PropagateCompletion = true });

                previousBlock = currentBlock;
            }

            var progressBlock = CreateProgressBlock(step, context, cancellation);
            previousBlock.LinkTo(progressBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return new DataflowPipeline<TItem>
            {
                InputBlock = firstBlock,
                Completion = progressBlock.Completion
            };
        }

        private DataflowPipeline<TItem> BuildEmpty<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            var progressBlock = CreateProgressBlock(step, context, cancellation);

            return new DataflowPipeline<TItem>
            {
                InputBlock = progressBlock,
                Completion = progressBlock.Completion
            };
        }

        private TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>> ToTransformBlock<TItem>(
            PipelineTaskStep<TItem> step,
            int blockIndex,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            var block = step.Blocks[blockIndex];

            return new TransformBlock<PipelineItem<TItem>, PipelineItem<TItem>>(
                x =>
                {
                    context.Events?.Raise(e => e.OnBlockStarted(block, x.Number, x.Item, step, context.StepContext.Task));
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        block.Body(x.Item);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    sw.Stop();
                    context.TotalBlockDurations.AddOrUpdate(block.Name, sw.Elapsed, (_, duration) => duration + sw.Elapsed);
                    context.Events?.Raise(e => e.OnBlockEnded(block, x.Number, x.Item, step, context.StepContext.Task, sw.Elapsed));

                    return x;
                },
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = block.Name,
                    MaxDegreeOfParallelism = block.MaxDegreeOfParallelism,
                    BoundedCapacity = block.MaxDegreeOfParallelism,
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }

        private ActionBlock<PipelineItem<TItem>> CreateProgressBlock<TItem>(
            PipelineTaskStep<TItem> step,
            DataflowExecutionContext context,
            CancellationToken cancellation)
        {
            return new ActionBlock<PipelineItem<TItem>>(
                x =>
                {
                    x.ProcessingStopwatch.Stop();
                    context.Events?.Raise(e => e.OnItemEnded(x.Number, x.Item, step, context.StepContext.Task, x.ProcessingStopwatch.Elapsed));
                    context.TaskEvents?.Raise(e => e.OnStepProgressed(x.Number, context.ExpectedItemsCount, x.ProcessingStopwatch.Elapsed, step, context.StepContext.Task));
                },
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = "Progress",
                    BoundedCapacity = 1,
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }
    }
}
