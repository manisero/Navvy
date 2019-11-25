using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Logging;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Models;
using Manisero.Navvy.Reporting;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests
{
    public class reporting
    {
        [Fact]
        public void reporter_creates_and_fills_execution_reports()
        {
            // Arrange
            var task = new TaskDefinition(
                BasicTaskStep.Empty("Basic"),
                new PipelineTaskStep<int>(
                    "Pipeline1",
                    new[] { 1 },
                    new List<PipelineBlock<int>>()),
                new PipelineTaskStep<int>(
                    "Pipeline2",
                    new[] { 1 },
                    new List<PipelineBlock<int>>()));

            var loggerEvents = TaskExecutionLogger.CreateEvents();
            var reporterEvents = TaskExecutionReporter.CreateEvents();

            // Act
            task.Execute(events: loggerEvents.Concat(reporterEvents).ToArray());

            // Assert
            var reports = task.GetExecutionReports();
            reports.Should().NotBeNull().And.NotBeEmpty();
            reports.Should().Contain(x => x.Name == "charts.html");
            reports.Should().Contain(x => x.Name == "Pipeline1_charts.html");
            reports.Should().Contain(x => x.Name == "Pipeline2_charts.html");
            reports.Should().OnlyContain(x => x.Content.StartsWith("<html>"));
        }

        [Fact]
        public void step_ignored___report_still_generated()
        {
            // Arrange
            var task = new TaskDefinition(
                BasicTaskStep.Empty("Basic"),
                new BasicTaskStep(
                    "Ignored",
                    () => { },
                    x => false));

            var loggerEvents = TaskExecutionLogger.CreateEvents();
            var reporterEvents = TaskExecutionReporter.CreateEvents();

            // Act
            task.Execute(events: loggerEvents.Concat(reporterEvents).ToArray());

            // Assert
            var reports = task.GetExecutionReports();
            reports.Should().NotBeNull().And.NotBeEmpty();
        }

        [Fact]
        public void all_steps_ignored___report_still_generated()
        {
            // Arrange
            var task = new TaskDefinition(
                new BasicTaskStep(
                    "Ignored1",
                    () => { },
                    x => false),
                new BasicTaskStep(
                    "Ignored2",
                    () => { },
                    x => false));

            var loggerEvents = TaskExecutionLogger.CreateEvents();
            var reporterEvents = TaskExecutionReporter.CreateEvents();

            // Act
            task.Execute(events: loggerEvents.Concat(reporterEvents).ToArray());

            // Assert
            var reports = task.GetExecutionReports();
            reports.Should().NotBeNull().And.NotBeEmpty();
        }

        [Fact]
        public void task_fails___no_report()
        {
            // Arrange
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        "Pipeline")
                    .WithInput(new[] { 1 })
                    .WithBlock("Failing block", x => throw new Exception())
                    .Build());

            var loggerEvents = TaskExecutionLogger.CreateEvents();
            var reporterEvents = TaskExecutionReporter.CreateEvents();

            // Act
            task.Execute(events: loggerEvents.Concat(reporterEvents).ToArray());

            // Assert
            var reports = task.Extras.TryGet<IReadOnlyCollection<TaskExecutionReport>>(TaskExecutionReportingUtils.TaskExecutionReportsExtraKey);
            reports.Should().BeNull();
        }
    }
}
