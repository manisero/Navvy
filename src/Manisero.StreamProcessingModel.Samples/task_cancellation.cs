using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.Executors;
using Manisero.StreamProcessingModel.Executors.StepExecutorResolvers;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Models.TaskSteps;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples
{
    public class task_cancellation
    {
        [Fact]
        public void sequential_basic()
        {
            basic(new SequentialTaskExecutorResolver());
        }

        [Fact]
        public void sequential_pipeline()
        {
            pipeline(new SequentialTaskExecutorResolver());
        }

        [Fact]
        public void streaming_basic()
        {
            basic(new StreamingTaskExecutorResolver());
        }

        [Fact]
        public void streaming_pipeline()
        {
            pipeline(new StreamingTaskExecutorResolver());
        }

        private void basic(ITaskStepExecutorResolver taskStepExecutorResolver)
        {
            // Arrange
            var completed = false;
            var cancellationSource = new CancellationTokenSource();

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep
                    {
                        Name = "Cancel",
                        Body = () => cancellationSource.Cancel()
                    },
                    new BasicTaskStep
                    {
                        Name = "Complete",
                        Body = () => { completed = true; }
                    }
                }
            };

            var progress = new Progress<TaskProgress>(_ => {});
            var executor = new TaskExecutor(taskStepExecutorResolver);

            // Act
            var result = executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            completed.Should().Be(false);
            result.Outcome.Should().Be(TaskOutcome.Canceled);
        }

        private void pipeline(ITaskStepExecutorResolver taskStepExecutorResolver)
        {
            var completed = false;
            var cancellationSource = new CancellationTokenSource();

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
                            new PipelineBlock<int>(
                                "Cancel",
                                x =>
                                {
                                    if (x == 0)
                                    {
                                        cancellationSource.Cancel();
                                    }
                                    else
                                    {
                                        completed = true;
                                    }
                                })
                        })
                }
            };

            var progress = new Progress<TaskProgress>(_ => { });
            var executor = new TaskExecutor(taskStepExecutorResolver);

            // Act
            var result = executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            completed.Should().Be(false);
            result.Outcome.Should().Be(TaskOutcome.Canceled);
        }
    }
}
