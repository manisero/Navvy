using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.BasicProcessing;
using Manisero.StreamProcessingModel.Core.Events;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.PipelineProcessing;
using Manisero.StreamProcessingModel.PipelineProcessing.Events;
using Manisero.StreamProcessingModel.Samples.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Manisero.StreamProcessingModel.Samples
{
    public class task_execution
    {
        private readonly ITestOutputHelper _output;

        public task_execution(
            ITestOutputHelper output)
        {
            _output = output;
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void test(ResolverType resolverType)
        {
            // Arrange
            var initialized = false;
            var sum = 0;
            var completed = false;

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Initialize",
                        () => { initialized = true; }),
                    new PipelineTaskStep<int>(
                        "Pipeline",
                        new[]
                        {
                            new[] { 1, 2, 3 },
                            new[] { 4, 5, 6 }
                        },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.ItemBody(
                                "Sum",
                                x => sum += x),
                            PipelineBlock<int>.BatchBody(
                                "Log",
                                x => _output.WriteLine(string.Join(", ", x)))
                        }),
                    new BasicTaskStep(
                        "Complete",
                        () => { completed = true; })
                }
            };

            var progress = new Progress<TaskProgress>(x => _output.WriteLine($"{x.StepName}: {x.ProgressPercentage}%"));
            var cancellationSource = new CancellationTokenSource();

            var taskEvents = new TaskExecutionEvents(
                taskStarted: x => _output.WriteLine("Task started."),
                taskEnded: x => _output.WriteLine($"Task ended after {x.Duration.Ticks} ticks."),
                stepStarted: x => _output.WriteLine($"Step '{x.Step.Name}' started."),
                stepEnded: x => _output.WriteLine($"Step '{x.Step.Name}' ended after {x.Duration.Ticks} ticks."));

            var pipelineEvents = new PipelineExecutionEvents(
                batchStarted: x => _output.WriteLine($"Batch {x.BatchNumber} of step '{x.Step.Name}' started."),
                batchEnded: x => _output.WriteLine($"Batch {x.BatchNumber} of step '{x.Step.Name}' ended after {x.Duration.Ticks} ticks."),
                blockStarted: x => _output.WriteLine($"Block '{x.Block.Name}' of step '{x.Step.Name}' started processing batch {x.BatchNumber}."),
                blockEnded: x => _output.WriteLine($"Block '{x.Block.Name}' of step '{x.Step.Name}' ended processing batch {x.BatchNumber} after {x.Duration.Ticks} ticks."));

            // Act
            var result = taskDescription.Execute(resolverType, progress, cancellationSource, taskEvents, pipelineEvents);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Successful);
            initialized.Should().Be(true);
            sum.Should().Be(21);
            completed.Should().Be(true);
        }
    }
}
