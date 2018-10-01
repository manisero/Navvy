using System.Threading;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Events;
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
                PipelineTaskStep
                    .Builder<int>("Pipeline")
                    .WithInput(new[] { 1, 2, 3, 4, 5, 6 })
                    .WithBlock("Sum", x => sum += x)
                    .WithBlock("Log", x => _output.WriteLine(x.ToString()))
                    .Build(),
                new BasicTaskStep(
                    "Complete",
                    () => { completed = true; }));

            var progress = new SynchronousProgress<TaskProgress>(x => _output.WriteLine($"{x.StepName}: {x.ProgressPercentage}%"));
            var cancellationSource = new CancellationTokenSource();

            var taskEvents = new TaskExecutionEvents(
                taskStarted: x => _output.WriteLine("Task started."),
                taskEnded: x => _output.WriteLine($"Task ended after {x.Duration.Ticks} ticks."),
                stepStarted: x => _output.WriteLine($"Step '{x.Step.Name}' started."),
                stepEnded: x => _output.WriteLine($"Step '{x.Step.Name}' ended after {x.Duration.Ticks} ticks."));

            var pipelineEvents = new PipelineExecutionEvents(
                itemMaterialized: x => _output.WriteLine($"Item {x.ItemNumber} of step '{x.Step.Name}' materialized."),
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
