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
                                "Errors / Complete",
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
                                "Errors",
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
            var executor = TaskExecutorFactory.Create(resolverType);

            // Act
            var result = executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Failed);
            var error = result.Errors.Should().NotBeNull().And.ContainSingle().Subject;
            error.StepName.ShouldBeEquivalentTo(FailingStepName);
            error.InnerException.Should().BeSameAs(_error);
        }
    }
}
