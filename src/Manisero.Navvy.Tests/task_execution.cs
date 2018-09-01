using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Events;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Events;
using Manisero.Navvy.Tests.Utils;
using Xunit;
using Xunit.Abstractions;

namespace Manisero.Navvy.Tests
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

            var task = new TaskDefinition(
                new BasicTaskStep(
                    "Initialize",
                    () => { initialized = true; }),
                new PipelineTaskStep<int>(
                    "Pipeline",
                    new[] { 1, 2, 3, 4, 5, 6 },
                    new List<PipelineBlock<int>>
                    {
                        new PipelineBlock<int>(
                            "Sum",
                            x => sum += x),
                        new PipelineBlock<int>(
                            "Log",
                            x => _output.WriteLine(x.ToString()))
                    }),
                new BasicTaskStep(
                    "Complete",
                    () => { completed = true; }));

            var progress = new Progress<TaskProgress>(x => _output.WriteLine($"{x.StepName}: {x.ProgressPercentage}%"));
            var cancellationSource = new CancellationTokenSource();

            var taskEvents = new TaskExecutionEvents(
                taskStarted: x => _output.WriteLine("Task started."),
                taskEnded: x => _output.WriteLine($"Task ended after {x.Duration.Ticks} ticks."),
                stepStarted: x => _output.WriteLine($"Step '{x.Step.Name}' started."),
                stepEnded: x => _output.WriteLine($"Step '{x.Step.Name}' ended after {x.Duration.Ticks} ticks."));

            var pipelineEvents = new PipelineExecutionEvents(
                itemStarted: x => _output.WriteLine($"Item {x.ItemNumber} of step '{x.Step.Name}' started."),
                itemEnded: x => _output.WriteLine($"Item {x.ItemNumber} of step '{x.Step.Name}' ended after {x.Duration.Ticks} ticks."),
                blockStarted: x => _output.WriteLine($"Block '{x.Block.Name}' of step '{x.Step.Name}' started processing item {x.ItemNumber}."),
                blockEnded: x => _output.WriteLine($"Block '{x.Block.Name}' of step '{x.Step.Name}' ended processing item {x.ItemNumber} after {x.Duration.Ticks} ticks."));

            // Act
            var result = task.Execute(resolverType, progress, cancellationSource, taskEvents, pipelineEvents);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Successful);
            initialized.Should().Be(true);
            sum.Should().Be(21);
            completed.Should().Be(true);
        }
    }
}
