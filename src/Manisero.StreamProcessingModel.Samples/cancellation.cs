using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.BasicProcessing;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.PipelineProcessing;
using Manisero.StreamProcessingModel.Samples.Utils;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples
{
    public class cancellation
    {
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private bool _completed;

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void basic(ResolverType resolverType)
        {
            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Cancel",
                        _cancellationSource.Cancel),
                    new BasicTaskStep(
                        "Complete",
                        () => { _completed = true; })
                }
            };

            test(taskDescription, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___within_single_block_between_batches(ResolverType resolverType)
        {
            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        "Step",
                        new[]
                        {
                            new[] { 0 },
                            new[] { 1 }
                        },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.BatchBody(
                                "Cancel / Complete",
                                x =>
                                {
                                    if (x.Contains(0))
                                    {
                                        _cancellationSource.Cancel();
                                    }
                                    else
                                    {
                                        _completed = true;
                                    }
                                })
                        })
                }
            };

            test(taskDescription, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___between_blocks(ResolverType resolverType)
        {
            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        "Step",
                        new[] { new[] { 0 } },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.BatchBody(
                                "Cancel",
                                x => { _cancellationSource.Cancel(); }),
                            PipelineBlock<int>.BatchBody(
                                "Complete",
                                x => { _completed = true; })
                        })
                }
            };

            test(taskDescription, resolverType);
        }

        private void test(
            TaskDescription taskDescription,
            ResolverType resolverType)
        {
            // Arrange
            var progress = new Progress<TaskProgress>(_ => {});
            var executor = TaskExecutorFactory.Create(resolverType);

            // Act
            var result = executor.Execute(taskDescription, progress, _cancellationSource.Token);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Canceled);
            _completed.Should().Be(false);
        }
    }
}
