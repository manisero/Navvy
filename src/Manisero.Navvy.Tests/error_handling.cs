using System;
using System.Collections.Generic;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests
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
            var task = new TaskDefinition(
                new BasicTaskStep(
                    FailingStepName,
                    () => throw _error));
            
            test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___catches_error(ResolverType resolverType)
        {
            var task = new TaskDefinition(
                new PipelineTaskStep<int>(
                    FailingStepName,
                    new[] { 0 },
                    new List<PipelineBlock<int>>
                    {
                        new PipelineBlock<int>(
                            "Block",
                            _ => throw _error)
                    }));
            
            test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___item_following_invalid_item_is_not_processed(ResolverType resolverType)
        {
            var completed = false;

            var task = new TaskDefinition(
                new PipelineTaskStep<int>(
                    FailingStepName,
                    new[] { 0, 1 },
                    new List<PipelineBlock<int>>
                    {
                        new PipelineBlock<int>(
                            "Errors / Complete",
                            x =>
                            {
                                if (x == 0)
                                {
                                    throw _error;
                                }
                                else
                                {
                                    completed = true;
                                }
                            })
                    }));

            test(task, resolverType);

            completed.Should().Be(false);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___invalid_item_is_not_further_processed(ResolverType resolverType)
        {
            var completed = false;

            var task = new TaskDefinition(
                new PipelineTaskStep<int>(
                    FailingStepName,
                    new[] { 0 },
                    new List<PipelineBlock<int>>
                    {
                        new PipelineBlock<int>(
                            "Errors",
                            x => throw _error),
                        new PipelineBlock<int>(
                            "Complete",
                            x => { completed = true; })
                    }));

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
