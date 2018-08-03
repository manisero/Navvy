using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.Executors;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Models.TaskSteps;
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
        public void pipeline(ResolverType resolverType)
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
                            PipelineBlock<int>.ItemBody(
                                "Cancel",
                                x =>
                                {
                                    if (x == 0)
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

        private void test(
            TaskDescription taskDescription,
            ResolverType resolverType)
        {
            // Arrange
            var progress = new Progress<TaskProgress>(_ => {});
            var executor = new TaskExecutor(TaskExecutorResolvers.Get(resolverType));

            // Act
            var result = executor.Execute(taskDescription, progress, _cancellationSource.Token);

            // Assert
            _completed.Should().Be(false);
            result.Outcome.Should().Be(TaskOutcome.Canceled);
        }
    }
}
