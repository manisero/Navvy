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
        private bool _completed;

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void basic(ResolverType resolverType)
        {
            test(GetBasicTask, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline(ResolverType resolverType)
        {
            test(GetPipelineTask, resolverType);
        }

        private void test(
            Func<CancellationTokenSource, TaskDescription> taskFactory,
            ResolverType resolverType)
        {
            // Arrange
            var cancellationSource = new CancellationTokenSource();
            var taskDescription = taskFactory(cancellationSource);

            var progress = new Progress<TaskProgress>(_ => {});
            var executor = new TaskExecutor(TaskExecutorResolvers.Get(resolverType));

            // Act
            var result = executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            _completed.Should().Be(false);
            result.Outcome.Should().Be(TaskOutcome.Canceled);
        }

        private TaskDescription GetBasicTask(CancellationTokenSource cancellationSource)
        {
            return new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Cancel",
                        cancellationSource.Cancel),
                    new BasicTaskStep(
                        "Complete",
                        () => { _completed = true; })
                }
            };
        }

        private TaskDescription GetPipelineTask(CancellationTokenSource cancellationSource)
        {
            return new TaskDescription
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
                                        cancellationSource.Cancel();
                                    }
                                    else
                                    {
                                        _completed = true;
                                    }
                                })
                        })
                }
            };
        }
    }
}
