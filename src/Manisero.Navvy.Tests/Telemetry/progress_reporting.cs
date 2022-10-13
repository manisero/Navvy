using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests.Telemetry
{
    public class progress_reporting
    {
        [Fact]
        public async Task basic_automatic()
        {
            await test(
                ResolverType.Sequential,
                BasicTaskStep.Empty("Step"),
                new[] { 1f });
        }

        [Fact]
        public async Task basic_manual()
        {
            await test(
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
        public async Task pipeline(ResolverType resolverType)
        {
            await test(
                resolverType,
                GetPipelineStep(4),
                new[] { 0.25f, 0.5f, 0.75f, 1f });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline___more_items_than_expected(ResolverType resolverType)
        {
            await test(
                resolverType,
                GetPipelineStep(3, 2),
                new[] { 0.5f, 1f, 1f });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline___less_items_than_expected(ResolverType resolverType)
        {
            await test(
                resolverType,
                GetPipelineStep(3, 4),
                new[] { 0.25f, 0.5f, 0.75f });
        }

        private async Task test(
            ResolverType resolverType,
            ITaskStep taskStep,
            ICollection<float> expectedProgressReports)
        {
            // Arrange
            var progressReports = new List<StepProgressedEvent>();
            
            var task = new TaskDefinition(taskStep);
            var events = new TaskExecutionEvents(stepProgressed: progressReports.Add);

            // Act
            await task.Execute(resolverType, events: events);

            // Assert
            progressReports.Should().HaveCount(expectedProgressReports.Count);
            progressReports.Select(x => x.Step.Name).Should().OnlyContain(x => x == taskStep.Name);
            progressReports.Select(x => x.Progress).Should().BeEquivalentTo(expectedProgressReports);
        }

        private ITaskStep GetPipelineStep(int actualItemsCount, int? expectedItemsCount = null)
        {
            return TaskStepBuilder.Build.Pipeline<int>(
                    "Step")
                .WithInput(
                    Enumerable.Repeat(0, actualItemsCount),
                    expectedItemsCount ?? actualItemsCount)
                .WithBlock(
                    "Block",
                    x => { })
                .Build();
        }
    }
}
