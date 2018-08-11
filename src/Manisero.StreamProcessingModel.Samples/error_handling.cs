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
    public class error_handling
    {
        private const string FailingStepName = "Failing Step";
        private readonly Exception _error = new Exception();

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
                        FailingStepName,
                        () => throw _error)
                }
            };
            
            test(taskDescription, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___catches_error(ResolverType resolverType)
        {
            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        FailingStepName,
                        new[] { new[] { 0 } },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.BatchBody(
                                "Block",
                                _ => throw _error
                            )
                        })
                }
            };
            
            test(taskDescription, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___batch_following_invalid_batch_is_not_processed(ResolverType resolverType)
        {
            var completed = false;

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        FailingStepName,
                        new[]
                        {
                            new[] { 0 },
                            new[] { 1 }
                        },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.BatchBody(
                                "Error / Complete",
                                x =>
                                {
                                    if (x.Contains(0))
                                    {
                                        throw _error;
                                    }
                                    else
                                    {
                                        completed = true;
                                    }
                                })
                        })
                }
            };

            test(taskDescription, resolverType);

            completed.Should().Be(false);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___invalid_batch_is_not_further_processed(ResolverType resolverType)
        {
            var completed = false;

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        FailingStepName,
                        new[] { new[] { 0 } },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.BatchBody(
                                "Error",
                                x => throw _error),
                            PipelineBlock<int>.BatchBody(
                                "Complete",
                                x => { completed = true; })
                        })
                }
            };

            test(taskDescription, resolverType);

            completed.Should().Be(false);
        }

        private void test(
            TaskDescription taskDescription,
            ResolverType resolverType)
        {
            // Arrange
            var progress = new Progress<TaskProgress>(_ => {});
            var cancellationSource = new CancellationTokenSource();
            var executor = new TaskExecutor(TaskExecutorResolvers.Get(resolverType));

            // Act
            var result = executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Failed);
            result.Error.Should().NotBeNull();
            result.Error.StepName.ShouldBeEquivalentTo(FailingStepName);
            result.Error.InnerException.Should().BeSameAs(_error);
        }
    }
}
