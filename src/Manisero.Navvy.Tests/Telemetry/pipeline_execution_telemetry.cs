﻿using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Events;
using Manisero.Navvy.PipelineProcessing.Models;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests.Telemetry
{
    public class pipeline_execution_telemetry
    {
        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task item_start_and_end_is_reported(ResolverType resolverType)
        {
            // Arrange
            ItemMaterializedEvent? startedEvent = null;
            ItemEndedEvent? endedEvent = null;

            var events = new PipelineExecutionEvents(
                itemMaterialized: x => startedEvent = x,
                itemEnded: x => endedEvent = x);

            var item = 1;

            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        "Step")
                    .WithInput(new[] { item })
                    .Build());

            // Act
            await task.Execute(resolverType, events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.ItemNumber.Should().Be(1);
            startedEvent.Value.Item.Should().Be(item);
            startedEvent.Value.MaterializationDuration.Should().BePositive();

            endedEvent.Should().NotBeNull();
            endedEvent.Value.ItemNumber.Should().Be(1);
            endedEvent.Value.Item.Should().Be(item);
            endedEvent.Value.Duration.Should().BePositive();
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task block_start_and_end_is_reported(ResolverType resolverType)
        {
            // Arrange
            BlockStartedEvent? startedEvent = null;
            BlockEndedEvent? endedEvent = null;

            var events = new PipelineExecutionEvents(
                blockStarted: x => startedEvent = x,
                blockEnded: x => endedEvent = x);

            var block = new PipelineBlock<int>(
                "Block",
                x => { });

            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        "Step")
                    .WithInput(new[] { 0 })
                    .WithBlock(block)
                    .Build());

            // Act
            await task.Execute(resolverType, events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Block.Should().BeSameAs(block);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Block.Should().BeSameAs(block);
            endedEvent.Value.Duration.Should().BePositive();
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline_end_is_reported(ResolverType resolverType)
        {
            // Arrange
            PipelineEndedEvent? endedEvent = null;

            var events = new PipelineExecutionEvents(
                pipelineEnded: x => endedEvent = x);

            var block1 = new PipelineBlock<int>(
                "Block1",
                x => { });

            var block2 = new PipelineBlock<int>(
                "Block1",
                x => { });

            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        "Step")
                    .WithInput(
                        new[] { 0 },
                        1)
                    .WithBlock(block1)
                    .WithBlock(block2)
                    .Build());

            // Act
            await task.Execute(resolverType, events: events);

            // Assert
            endedEvent.Should().NotBeNull();
            endedEvent.Value.Step.Should().BeSameAs(task.Steps[0]);
            endedEvent.Value.TotalInputMaterializationDuration.Should().BePositive();

            endedEvent.Value.TotalBlockDurations.Should().NotBeNull();
            var totalBlockDurations = endedEvent.Value.TotalBlockDurations.ToDictionary(x => x.Key, x => x.Value);
            totalBlockDurations.Should().ContainKey(block1.Name).WhoseValue.Should().BePositive();
            totalBlockDurations.Should().ContainKey(block2.Name).WhoseValue.Should().BePositive();
        }
    }
}
