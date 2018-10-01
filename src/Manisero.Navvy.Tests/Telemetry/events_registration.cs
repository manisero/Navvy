using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests.Telemetry
{
    public class events_registration
    {
        [Fact]
        public void events_can_be_passed_for_single_task_execution()
        {
            // Arrange
            var eventHandled = false;

            var events = new TaskExecutionEvents();
            events.TaskStarted += _ => eventHandled = true;

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            var executor = TaskExecutorFactory.Create(ResolverType.Sequential);
            executor.Execute(task, events: events);

            // Assert
            eventHandled.Should().BeTrue();
        }

        [Fact]
        public void events_can_be_registered_for_Executor_instance()
        {
            // Arrange
            var eventHandled = false;

            var events = new TaskExecutionEvents();
            events.TaskStarted += _ => eventHandled = true;

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, events);
            executor.Execute(task);

            // Assert
            eventHandled.Should().BeTrue();
        }

        [Fact]
        public void global_and_one_time_events_are_merged()
        {
            // Arrange
            var globalEventHandled = false;
            var globalEvents = new TaskExecutionEvents();
            globalEvents.TaskStarted += _ => globalEventHandled = true;

            var oneTimeEventHandled = false;
            var oneTimeEvents = new TaskExecutionEvents();
            oneTimeEvents.TaskStarted += _ => oneTimeEventHandled = true;

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, globalEvents);
            executor.Execute(task, events: oneTimeEvents);

            // Assert
            globalEventHandled.Should().BeTrue();
            oneTimeEventHandled.Should().BeTrue();
        }
    }
}
