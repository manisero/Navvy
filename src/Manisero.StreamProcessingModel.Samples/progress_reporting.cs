using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.Executors;
using Manisero.StreamProcessingModel.Executors.StepExecutorResolvers;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Models.TaskSteps;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples
{
    public class progress_reporting
    {
        [Fact]
        public void sequential_basic()
        {
            test(
                new SequentialTaskExecutorResolver(),
                GetBasicStep(),
                new[] { 100 });
        }

        [Fact]
        public void sequential_pipeline()
        {
            test(
                new SequentialTaskExecutorResolver(),
                GetPipelineStep(3),
                new[] { 33, 66, 100 });
        }

        [Fact]
        public void sequential_pipeline___more_batches_than_expected()
        {
            test(
                new SequentialTaskExecutorResolver(),
                GetPipelineStep(3, 2),
                new[] { 50, 100, 100 });
        }

        [Fact]
        public void sequential_pipeline___less_batches_than_expected()
        {
            test(
                new SequentialTaskExecutorResolver(),
                GetPipelineStep(3, 4),
                new[] { 25, 50, 75 });
        }

        [Fact]
        public void streaming_basic()
        {
            test(
                new StreamingTaskExecutorResolver(),
                GetBasicStep(),
                new[] { 100 });
        }

        [Fact]
        public void streaming_pipeline()
        {
            test(
                new StreamingTaskExecutorResolver(),
                GetPipelineStep(3),
                new[] { 33, 66, 100 });
        }

        [Fact]
        public void streaming_pipeline___more_batches_than_expected()
        {
            test(
                new StreamingTaskExecutorResolver(),
                GetPipelineStep(3, 2),
                new[] { 50, 100, 100 });
        }

        [Fact]
        public void streaming_pipeline___less_batches_than_expected()
        {
            test(
                new StreamingTaskExecutorResolver(),
                GetPipelineStep(3, 4),
                new[] { 25, 50, 75 });
        }

        private void test(
            ITaskStepExecutorResolver taskStepExecutorResolver,
            ITaskStep taskStep,
            ICollection<int> expectedProgressReports)
        {
            // Arrange
            var progressReports = new List<byte>();

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep> { taskStep }
            };

            var progress = new Progress<TaskProgress>(x => progressReports.Add(x.ProgressPercentage));

            var cancellationSource = new CancellationTokenSource();
            var executor = new TaskExecutor(taskStepExecutorResolver);

            // Act
            executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            progressReports.ShouldAllBeEquivalentTo(expectedProgressReports);
        }

        private ITaskStep GetBasicStep()
        {
            return BasicTaskStep.Empty("Step");
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
