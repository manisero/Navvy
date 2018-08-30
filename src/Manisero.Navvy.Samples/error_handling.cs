using System;
using System.Collections.Generic;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Samples.Utils;
using Xunit;

namespace Manisero.Navvy.Samples
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
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        FailingStepName,
                        () => throw _error)
                }
            };
            
            test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___catches_error(ResolverType resolverType)
        {
            var task = new TaskDefinition
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
            
            test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___batch_following_invalid_batch_is_not_processed(ResolverType resolverType)
        {
            var completed = false;

            var task = new TaskDefinition
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

            test(task, resolverType);

            completed.Should().Be(false);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___invalid_batch_is_not_further_processed(ResolverType resolverType)
        {
            var completed = false;

            var task = new TaskDefinition
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

            test(task, resolverType);

            completed.Should().Be(false);
        }

        private void test(
            TaskDefinition task,
            ResolverType resolverType)
        {
            // Act
            var result = task.Execute(resolverType);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Failed);
            var error = result.Errors.Should().NotBeNull().And.ContainSingle().Subject;
            error.StepName.ShouldBeEquivalentTo(FailingStepName);
            error.InnerException.Should().BeSameAs(_error);
        }
    }
}
