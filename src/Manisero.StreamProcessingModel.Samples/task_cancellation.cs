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
        private bool _completed;

        [Fact]
        public void sequential_basic()
        {
            test(GetBasicTask, new SequentialTaskExecutorResolver());
        }

        [Fact]
        public void sequential_pipeline()
        {
            test(GetPipelineTask, new SequentialTaskExecutorResolver());
        }

        [Fact]
        public void streaming_basic()
        {
            test(GetBasicTask, new StreamingTaskExecutorResolver());
        }

        [Fact]
        public void streaming_pipeline()
        {
            test(GetPipelineTask, new StreamingTaskExecutorResolver());
        }

        private void test(
            Func<CancellationTokenSource, TaskDescription> taskFactory,
            ITaskStepExecutorResolver taskStepExecutorResolver)
        {
            // Arrange
            var cancellationSource = new CancellationTokenSource();
            var taskDescription = taskFactory(cancellationSource);

            var progress = new Progress<TaskProgress>(_ => {});
            var executor = new TaskExecutor(taskStepExecutorResolver);

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
                    new BasicTaskStep
                    {
                        Name = "Cancel",
                        Body = () => cancellationSource.Cancel()
                    },
                    new BasicTaskStep
                    {
                        Name = "Complete",
                        Body = () => { _completed = true; }
                    }
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
