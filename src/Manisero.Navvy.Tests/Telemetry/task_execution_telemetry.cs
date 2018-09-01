using System;
using System.Threading;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests.Telemetry
{
    public class task_execution_telemetry
    {
        [Fact]
        public void task_start_and_end_is_reported()
        {
            // Arrange
            TaskStartedEvent? startedEvent = null;
            TaskEndedEvent? endedEvent = null;

            var events = new TaskExecutionEvents();
            events.TaskStarted += x => startedEvent = x;
            events.TaskEnded += x => endedEvent = x;

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            task.Execute(events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Task.ShouldBeEquivalentTo(task);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Task.ShouldBeEquivalentTo(task);
            endedEvent.Value.Duration.Ticks.Should().BePositive();
        }

        [Fact]
        public void step_start_and_end_is_reported()
        {
            // Arrange
            StepStartedEvent? startedEvent = null;
            StepEndedEvent? endedEvent = null;

            var events = new TaskExecutionEvents();
            events.StepStarted += x => startedEvent = x;
            events.StepEnded += x => endedEvent = x;

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            task.Execute(events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Step.ShouldBeEquivalentTo(task.Steps[0]);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Step.ShouldBeEquivalentTo(task.Steps[0]);
            endedEvent.Value.Duration.Ticks.Should().BePositive();
        }

        [Fact]
        public void step_skip_is_reported()
        {
            // Arrange
            StepSkippedEvent? skippedEvent = null;

            var events = new TaskExecutionEvents();
            events.StepSkipped += x => skippedEvent = x;

            var task = new TaskDefinition(
                new BasicTaskStep(
                    "Step",
                    () => { },
                    _ => false));

            // Act
            task.Execute(events: events);

            // Assert
            skippedEvent.Should().NotBeNull();
            skippedEvent.Value.Step.ShouldBeEquivalentTo(task.Steps[0]);
        }

        [Fact]
        public void step_cancellation_is_reported()
        {
            // Arrange
            StepCanceledEvent? canceledEvent = null;

            var events = new TaskExecutionEvents();
            events.StepCanceled += x => canceledEvent = x;

            var cancellationSource = new CancellationTokenSource();

            var task = new TaskDefinition(
                new BasicTaskStep(
                    "Step",
                    () => cancellationSource.Cancel()));

            // Act
            task.Execute(cancellation: cancellationSource, events: events);

            // Assert
            canceledEvent.Should().NotBeNull();
            canceledEvent.Value.Step.ShouldBeEquivalentTo(task.Steps[0]);
        }

        [Fact]
        public void step_failure_is_reported()
        {
            // Arrange
            StepFailedEvent? failedEvent = null;

            var events = new TaskExecutionEvents();
            events.StepFailed += x => failedEvent = x;

            var exception = new Exception();

            var task = new TaskDefinition(
                new BasicTaskStep(
                    "Step",
                    () => throw exception));

            // Act
            task.Execute(events: events);

            // Assert
            failedEvent.Should().NotBeNull();
            failedEvent.Value.Exception.InnerException.ShouldBeEquivalentTo(exception);
            failedEvent.Value.Step.ShouldBeEquivalentTo(task.Steps[0]);
        }
    }
}
