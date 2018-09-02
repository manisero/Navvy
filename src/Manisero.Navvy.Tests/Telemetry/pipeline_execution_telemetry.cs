using System.Collections.Generic;
using FluentAssertions;
using Manisero.Navvy.Core.Models;
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
        public void item_start_and_end_is_reported(ResolverType resolverType)
        {
            // Arrange
            ItemStartedEvent? startedEvent = null;
            ItemEndedEvent? endedEvent = null;

            var events = new PipelineExecutionEvents();
            events.ItemStarted += x => startedEvent = x;
            events.ItemEnded += x => endedEvent = x;

            var item = 1;

            var task = new TaskDefinition(
                new PipelineTaskStep<int>(
                    "Step",
                    new[] { item },
                    new List<PipelineBlock<int>>()));

            // Act
            task.Execute(resolverType, events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.ItemNumber.ShouldBeEquivalentTo(1);
            startedEvent.Value.Item.ShouldBeEquivalentTo(item);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.ItemNumber.ShouldBeEquivalentTo(1);
            endedEvent.Value.Item.ShouldBeEquivalentTo(item);
            endedEvent.Value.Duration.Ticks.Should().BePositive();
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void block_start_and_end_is_reported(ResolverType resolverType)
        {
            // Arrange
            BlockStartedEvent? startedEvent = null;
            BlockEndedEvent? endedEvent = null;

            var events = new PipelineExecutionEvents();
            events.BlockStarted += x => startedEvent = x;
            events.BlockEnded += x => endedEvent = x;

            var block = new PipelineBlock<int>(
                "Block",
                x => { });

            var task = new TaskDefinition(
                new PipelineTaskStep<int>(
                    "Step",
                    new[] { 0 },
                    new List<PipelineBlock<int>> { block }));

            // Act
            task.Execute(resolverType, events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Block.ShouldBeEquivalentTo(block);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Block.ShouldBeEquivalentTo(block);
            endedEvent.Value.Duration.Ticks.Should().BePositive();
        }
    }
}
