﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Logging;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Reporting;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests
{
    public class reporting
    {
        [Fact]
        public async Task reporter_creates_and_fills_execution_reports()
        {
            // Arrange
            var task = new TaskDefinition(
                BasicTaskStep.Empty("Basic"),
                TaskStepBuilder.Build.Pipeline<int>(
                        "Pipeline1")
                    .WithInput(new[] { 1 })
                    .Build(),
                TaskStepBuilder.Build.Pipeline<int>(
                        "Pipeline2")
                    .WithInput(new[] { 1 })
                    .Build());

            var loggerEvents = TaskExecutionLogger.CreateEvents();
            var reporterEvents = TaskExecutionReporter.CreateEvents();

            // Act
            await task.Execute(events: loggerEvents.Concat(reporterEvents).ToArray());

            // Assert
            var reports = task.GetExecutionReports();
            reports.Should().NotBeNull().And.NotBeEmpty();
            reports.Should().Contain(x => x.Name == "charts.html");
            reports.Should().Contain(x => x.Name == "Pipeline1_charts.html");
            reports.Should().Contain(x => x.Name == "Pipeline2_charts.html");
            reports.Should().OnlyContain(x => x.Content.StartsWith("<html>"));
        }

        [Fact]
        public async Task step_ignored___report_still_generated()
        {
            // Arrange
            var task = new TaskDefinition(
                BasicTaskStep.Empty("Basic"),
                TaskStepBuilder.Build.Basic(
                    "Ignored",
                    () => { },
                    x => false));

            var loggerEvents = TaskExecutionLogger.CreateEvents();
            var reporterEvents = TaskExecutionReporter.CreateEvents();

            // Act
            await task.Execute(events: loggerEvents.Concat(reporterEvents).ToArray());

            // Assert
            var reports = task.GetExecutionReports();
            reports.Should().NotBeNull().And.NotBeEmpty();
        }

        [Fact]
        public async Task all_steps_ignored___report_still_generated()
        {
            // Arrange
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Basic(
                    "Ignored1",
                    () => { },
                    x => false),
                TaskStepBuilder.Build.Basic(
                    "Ignored2",
                    () => { },
                    x => false));

            var loggerEvents = TaskExecutionLogger.CreateEvents();
            var reporterEvents = TaskExecutionReporter.CreateEvents();

            // Act
            await task.Execute(events: loggerEvents.Concat(reporterEvents).ToArray());

            // Assert
            var reports = task.GetExecutionReports();
            reports.Should().NotBeNull().And.NotBeEmpty();
        }

        [Fact]
        public async Task task_fails___no_report()
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
            try
            {
                await task.Execute(events: loggerEvents.Concat(reporterEvents).ToArray());
            }
            catch (TaskExecutionException)
            {
            }
            catch (OperationCanceledException)
            {
            }

            // Assert
            var reports = task.Extras.TryGet<IReadOnlyCollection<TaskExecutionReport>>(TaskExecutionReportingUtils.TaskExecutionReportsExtraKey);
            reports.Should().BeNull();
        }
    }
}
