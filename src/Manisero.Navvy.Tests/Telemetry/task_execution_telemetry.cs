﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests.Telemetry
{
    public class task_execution_telemetry
    {
        [Fact]
        public async Task task_start_and_end_is_reported()
        {
            // Arrange
            TaskStartedEvent? startedEvent = null;
            TaskEndedEvent? endedEvent = null;

            var events = new TaskExecutionEvents(
                taskStarted: x => startedEvent = x,
                taskEnded: x => endedEvent = x);

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            await task.Execute(events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Task.Should().BeSameAs(task);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Task.Should().BeSameAs(task);
            endedEvent.Value.Duration.Should().BePositive();
        }

        [Fact]
        public async Task step_start_and_end_is_reported()
        {
            // Arrange
            StepStartedEvent? startedEvent = null;
            StepEndedEvent? endedEvent = null;

            var events = new TaskExecutionEvents(
                stepStarted: x => startedEvent = x,
                stepEnded: x => endedEvent = x);

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            await task.Execute(events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Step.Should().BeSameAs(task.Steps[0]);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Step.Should().BeSameAs(task.Steps[0]);
            endedEvent.Value.Duration.Should().BePositive();
        }

        [Fact]
        public async Task step_skip_is_reported()
        {
            // Arrange
            StepSkippedEvent? skippedEvent = null;

            var events = new TaskExecutionEvents(
                stepSkipped: x => skippedEvent = x);

            var task = new TaskDefinition(
                TaskStepBuilder.Build.Basic(
                    "Step",
                    () => { },
                    _ => false));

            // Act
            await task.Execute(events: events);

            // Assert
            skippedEvent.Should().NotBeNull();
            skippedEvent.Value.Step.Should().BeSameAs(task.Steps[0]);
        }

        [Fact]
        public async Task step_cancellation_is_reported()
        {
            // Arrange
            StepCanceledEvent? canceledEvent = null;

            var events = new TaskExecutionEvents(
                stepCanceled: x => canceledEvent = x);

            var cancellationSource = new CancellationTokenSource();

            var task = new TaskDefinition(
                TaskStepBuilder.Build.Basic(
                    "Step",
                    () => cancellationSource.Cancel()));

            // Act
            try
            {
                await task.Execute(cancellation: cancellationSource, events: events);
            }
            catch (TaskExecutionException)
            {
            }
            catch (OperationCanceledException)
            {
            }

            // Assert
            canceledEvent.Should().NotBeNull();
            canceledEvent.Value.Step.Should().BeSameAs(task.Steps[0]);
        }

        [Fact]
        public async Task step_failure_is_reported()
        {
            // Arrange
            StepFailedEvent? failedEvent = null;

            var events = new TaskExecutionEvents(
                stepFailed: x => failedEvent = x);

            var exception = new Exception();

            var task = new TaskDefinition(
                TaskStepBuilder.Build.Basic(
                    "Step",
                    () => throw exception));

            // Act
            try
            {
                await task.Execute(events: events);
            }
            catch (TaskExecutionException)
            {
            }
            catch (OperationCanceledException)
            {
            }

            // Assert
            failedEvent.Should().NotBeNull();
            failedEvent.Value.Exception.InnerException.Should().BeSameAs(exception);
            failedEvent.Value.Step.Should().BeSameAs(task.Steps[0]);
        }
    }
}
