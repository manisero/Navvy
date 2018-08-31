using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.PipelineProcessing;
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
                new[] { 100 });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(3),
                new[] { 33, 66, 100 });
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___more_batches_than_expected(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(3, 2),
                new[] { 50, 100, 100 }); // TODO: Consider not reporting unexpected batches
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___less_batches_than_expected(ResolverType resolverType)
        {
            test(
                resolverType,
                GetPipelineStep(3, 4),
                new[] { 25, 50, 75 }); // TODO: Consider including step status in TaskProgress (and reporting finished when finished)
        }

        private void test(
            ResolverType resolverType,
            ITaskStep taskStep,
            ICollection<int> expectedProgressReports)
        {
            // Arrange
            var progressReports = new List<TaskProgress>();

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep> { taskStep }
            };

            var progress = new SynchronousProgress<TaskProgress>(x => progressReports.Add(x));

            // Act
            task.Execute(resolverType, progress);

            Thread.Sleep(10); // TODO: Remove once AppVeyor issue resolved

            // Assert
            progressReports.Select(x => x.StepName).Should().OnlyContain(x => x == taskStep.Name);
            progressReports.Select(x => x.ProgressPercentage).ShouldAllBeEquivalentTo(expectedProgressReports);
        }

        private ITaskStep GetPipelineStep(int actualBatchesCount, int? expectedBatchesCount = null)
        {
            return new PipelineTaskStep<int>(
                "Step",
                Enumerable.Repeat<ICollection<int>>(new[] { 0 }, actualBatchesCount),
                expectedBatchesCount ?? actualBatchesCount,
                new List<PipelineBlock<int>>
                {
                    new PipelineBlock<int>(
                        "Block",
                        x => { })
                });
        }
    }
}
