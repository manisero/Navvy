﻿using System.Collections.Generic;
using FluentAssertions;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.PipelineProcessing.Events;
using Manisero.Navvy.Samples.Utils;
using Xunit;

namespace Manisero.Navvy.Samples.Telemetry
{
    public class pipeline_execution_telemetry
    {
        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void batch_start_and_end_is_reported(ResolverType resolverType)
        {
            // Arrange
            BatchStartedEvent? startedEvent = null;
            BatchEndedEvent? endedEvent = null;

            var events = new PipelineExecutionEvents();
            events.BatchStarted += x => startedEvent = x;
            events.BatchEnded += x => endedEvent = x;

            var batch = new[] { 0 };

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        "Step",
                        new[] { batch },
                        new List<PipelineBlock<int>>())
                }
            };

            // Act
            task.Execute(resolverType, events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.BatchNumber.ShouldBeEquivalentTo(1);
            startedEvent.Value.Batch.ShouldBeEquivalentTo(batch);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.BatchNumber.ShouldBeEquivalentTo(1);
            endedEvent.Value.Batch.ShouldBeEquivalentTo(batch);
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

            var block = PipelineBlock<int>.BatchBody(
                "Block",
                x => { });

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        "Step",
                        new[] { new[] { 0 }},
                        new List<PipelineBlock<int>> { block })
                }
            };

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
