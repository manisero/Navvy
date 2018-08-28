﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.PipelineProcessing.Dataflow.StepExecution
{
    public class DataflowPipelineBuilder<TData>
    {
        private static readonly int DegreeOfParallelism = Environment.ProcessorCount - 1;

        public DataflowPipeline<TData> Build(
            PipelineTaskStep<TData> step,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            var firstBlock = ToTransformBlock(step, 0, cancellation);
            var previousBlock = firstBlock;

            for (var i = 1; i <= step.Blocks.Count - 1; i++)
            {
                var currentBlock = ToTransformBlock(step, i, cancellation);
                previousBlock.LinkTo(currentBlock, new DataflowLinkOptions { PropagateCompletion = true });

                previousBlock = currentBlock;
            }

            var progressBlock = CreateProgressBlock(step, progress, cancellation);
            previousBlock.LinkTo(progressBlock, new DataflowLinkOptions { PropagateCompletion = true });

            return new DataflowPipeline<TData>
            {
                InputBlock = firstBlock,
                Completion = progressBlock.Completion
            };
        }

        private TransformBlock<DataBatch<TData>, DataBatch<TData>> ToTransformBlock(
            PipelineTaskStep<TData> step,
            int blockIndex,
            CancellationToken cancellation)
        {
            var block = step.Blocks[blockIndex];
            var maxDegreeOfParallelism = block.Parallel ? DegreeOfParallelism : 1;

            return new TransformBlock<DataBatch<TData>, DataBatch<TData>>(
                x =>
                {
                    PipelineExecutionEvents.BlockStarted(
                        new BlockStartedEvent
                        {
                            Timestamp = DateTimeUtils.Now
                        });

                    var sw = Stopwatch.StartNew();

                    try
                    {
                        block.Body(x.Data);
                    }
                    catch (Exception e)
                    {
                        throw new TaskExecutionException(e, step, block.GetExceptionData());
                    }

                    sw.Stop();

                    PipelineExecutionEvents.BlockEnded(
                        new BlockEndedEvent
                        {
                            Timestamp = DateTimeUtils.Now,
                            Duration = sw.Elapsed
                        });

                    return x;
                },
                new ExecutionDataflowBlockOptions
                {
                    NameFormat = block.Name,
                    MaxDegreeOfParallelism = maxDegreeOfParallelism,
                    BoundedCapacity = maxDegreeOfParallelism,
                    EnsureOrdered = true,
                    CancellationToken = cancellation
                });
        }

        private ActionBlock<DataBatch<TData>> CreateProgressBlock(
            PipelineTaskStep<TData> step,
            IProgress<byte> progress,
            CancellationToken cancellation)
        {
            return new ActionBlock<DataBatch<TData>>(
                x => StepExecutionUtils.ReportProgress(x.Number, step.ExpectedInputBatchesCount, progress),
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
