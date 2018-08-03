using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.Executors;
using Manisero.StreamProcessingModel.Executors.StepExecutorResolvers;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Models.TaskSteps;
using Manisero.StreamProcessingModel.Samples.Utils;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples
{
    public class error_handling
    {
        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void basic(ResolverType resolverType)
        {
            // Arrange
            var exception = new Exception();

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Step",
                        () => throw exception)
                }
            };

            // Act
            var result = test(taskDescription, TaskExecutorResolvers.Get(resolverType));

            // Assert
            result.Error.Should().BeSameAs(exception);
        }

        private TaskResult test(
            TaskDescription taskDescription,
            ITaskStepExecutorResolver taskStepExecutorResolver)
        {
            // Arrange
            var progress = new Progress<TaskProgress>(_ => {});
            var cancellationSource = new CancellationTokenSource();
            var executor = new TaskExecutor(taskStepExecutorResolver);

            // Act
            var result = executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Failed);

            return result;
        }
    }
}
