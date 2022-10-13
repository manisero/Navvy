using System.Threading.Tasks;
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
        public async Task events_can_be_passed_for_single_task_execution()
        {
            // Arrange
            var eventHandled = false;

            var events = new TaskExecutionEvents(
                taskStarted: _ => eventHandled = true);

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            var executor = TaskExecutorFactory.Create(ResolverType.Sequential);
            await executor.Execute(task, events: events);

            // Assert
            eventHandled.Should().BeTrue();
        }

        [Fact]
        public async Task events_can_be_registered_for_Executor_instance()
        {
            // Arrange
            var eventHandled = false;

            var events = new TaskExecutionEvents(
                taskStarted: _ => eventHandled = true);

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, events);
            await executor.Execute(task);

            // Assert
            eventHandled.Should().BeTrue();
        }

        [Fact]
        public async Task global_and_one_time_events_are_merged()
        {
            // Arrange
            var globalEventHandled = false;
            var globalEvents = new TaskExecutionEvents(
                taskStarted: _ => globalEventHandled = true);

            var oneTimeEventHandled = false;
            var oneTimeEvents = new TaskExecutionEvents(
                taskStarted: _ => oneTimeEventHandled = true);

            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            // Act
            var executor = TaskExecutorFactory.Create(ResolverType.Sequential, globalEvents);
            await executor.Execute(task, events: oneTimeEvents);

            // Assert
            globalEventHandled.Should().BeTrue();
            oneTimeEventHandled.Should().BeTrue();
        }
    }
}
