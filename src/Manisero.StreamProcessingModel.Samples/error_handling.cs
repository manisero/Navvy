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
        private readonly Exception _exception = new Exception();

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
                        "Step",
                        () => throw _exception)
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
                        new[] { new[] { 0 } },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.BatchBody(
                                "Block",
                                _ => throw _exception
                            )
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
            var cancellationSource = new CancellationTokenSource();
            var executor = new TaskExecutor(TaskExecutorResolvers.Get(resolverType));

            // Act
            var result = executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Failed);
            result.Error.Should().BeSameAs(_exception);
        }
    }
}
