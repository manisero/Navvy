using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.BasicProcessing;
using Manisero.StreamProcessingModel.Core.Events;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Samples.Utils;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples
{
    public class telemetry
    {
        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void test(ResolverType resolverType)
        {
            // Arrange
            var startEventRaised = false;

            var events = new TaskExecutionEvents();
            events.TaskStarted += x => startEventRaised = true;

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Step",
                        () => { })
                }
            };
            
            var progress = new Progress<TaskProgress>(_ => { });
            var cancellationSource = new CancellationTokenSource();
            var executor = TaskExecutorFactory.Create(resolverType, events);

            // Act
            executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            startEventRaised.Should().BeTrue();
        }
    }
}
