using System;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Logging;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests
{
    public class logging
    {
        [Fact]
        public void logger_creates_and_fills_execution_log()
        {
            // Arrange
            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));

            var events = TaskExecutionLogger.CreateEvents();

            // Act
            var startTs = DateTime.UtcNow;
            task.Execute(events: events);
            var endTs = DateTime.UtcNow;

            // Assert
            var log = task.GetExecutionLog();
            log.Should().NotBeNull();
            log.TaskDuration.StartTs.Should().BeOnOrAfter(startTs).And.BeOnOrBefore(endTs);
            log.DiagnosticsLog.GetDiagnostics().Should().NotBeEmpty();
        }

        [Fact]
        public void task_fails___does_not_break()
        {
            // Arrange
            var task = new TaskDefinition(
                new BasicTaskStep(
                    "Failing step",
                    () => throw new Exception()));

            var events = TaskExecutionLogger.CreateEvents();

            // Act
            task.Execute(events: events);

            // Assert
            var log = task.GetExecutionLog();
            log.Should().NotBeNull();
        }
    }
}
