using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Models;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests
{
    public class progress_reporting
    {
        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void basic(ResolverType resolverType)
        {
            test(
                resolverType,
                BasicTaskStep.Empty("Step"),
                new byte[] { 100 });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(3),
                new byte[] { 33, 66, 100 });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___more_items_than_expected(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(3, 2),
                new byte[] { 50, 100, 100 }); // TODO: Consider not reporting unexpected items
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___less_items_than_expected(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(3, 4),
                new byte[] { 25, 50, 75 }); // TODO: Consider including step status in TaskProgress (and reporting finished when finished)
        }

        private void test(
            ResolverType resolverType,
            ITaskStep taskStep,
            ICollection<byte> expectedProgressReports)
        {
            // Arrange
            var progressReports = new List<TaskProgress>();

            var task = new TaskDefinition(taskStep);

            var progress = new SynchronousProgress<TaskProgress>(x => progressReports.Add(x));

            // Act
            task.Execute(resolverType, progress);

            // Assert
            progressReports.Select(x => x.StepName).Should().OnlyContain(x => x == taskStep.Name);
            progressReports.Select(x => x.ProgressPercentage).Should().BeEquivalentTo(expectedProgressReports);
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
