using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Models;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests.Telemetry
{
    public class progress_reporting
    {
        [Fact]
        public void basic_automatic()
        {
            test(
                ResolverType.Sequential,
                BasicTaskStep.Empty("Step"),
                new[] { 1f });
        }

        [Fact]
        public void basic_manual()
        {
            test(
                ResolverType.Sequential,
                TaskStepBuilder.Build.Basic(
                    "Step",
                    (p, c) =>
                    {
                        p.Report(0.5f);
                        p.Report(1f);
                    }),
                new[] { 0.5f, 1f, 1f });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(4),
                new[] { 0.25f, 0.5f, 0.75f, 1f });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___more_items_than_expected(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(3, 2),
                new[] { 0.5f, 1f, 1f });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___less_items_than_expected(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(3, 4),
                new[] { 0.25f, 0.5f, 0.75f });
        }

        private void test(
            ResolverType resolverType,
            ITaskStep taskStep,
            ICollection<float> expectedProgressReports)
        {
            // Arrange
            var progressReports = new List<StepProgressedEvent>();
            
            var task = new TaskDefinition(taskStep);
            var events = new TaskExecutionEvents(stepProgressed: progressReports.Add);

            // Act
            task.Execute(resolverType, events: events);

            // Assert
            progressReports.Should().HaveCount(expectedProgressReports.Count);
            progressReports.Select(x => x.Step.Name).Should().OnlyContain(x => x == taskStep.Name);
            progressReports.Select(x => x.Progress).Should().BeEquivalentTo(expectedProgressReports);
        }

        private ITaskStep GetPipelineStep(int actualItemsCount, int? expectedItemsCount = null)
        {
            return new PipelineTaskStep<int>(
                "Step",
                Enumerable.Repeat(0, actualItemsCount),
                expectedItemsCount ?? actualItemsCount,
                new List<PipelineBlock<int>>
                {
                    new PipelineBlock<int>(
                        "Block",
                        x => { })
                });
        }
    }
}
