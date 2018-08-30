using System.Collections.Generic;
using FluentAssertions;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.PipelineProcessing;
using Manisero.StreamProcessingModel.PipelineProcessing.Events;
using Manisero.StreamProcessingModel.Samples.Utils;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples.Telemetry
{
    public class pipeline_execution_telemetry
    {
        [Fact]
        public void batch_start_and_end_is_reported()
        {
            // Arrange
            BatchStartedEvent? startedEvent = null;
            BatchEndedEvent? endedEvent = null;

            var events = new PipelineExecutionEvents();
            events.BatchStarted += x => startedEvent = x;
            events.BatchEnded += x => endedEvent = x;

            var batch = new[] { 0 };

            var taskDescription = new TaskDescription
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
            taskDescription.Execute(events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.BatchNumber.ShouldBeEquivalentTo(1);
            startedEvent.Value.Batch.ShouldBeEquivalentTo(batch);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.BatchNumber.ShouldBeEquivalentTo(1);
            endedEvent.Value.Batch.ShouldBeEquivalentTo(batch);
            endedEvent.Value.Duration.Ticks.Should().BePositive();
        }

        [Fact]
        public void block_start_and_end_is_reported()
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

            var taskDescription = new TaskDescription
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
            taskDescription.Execute(events: events);

            // Assert
            startedEvent.Should().NotBeNull();
            startedEvent.Value.Block.ShouldBeEquivalentTo(block);

            endedEvent.Should().NotBeNull();
            endedEvent.Value.Block.ShouldBeEquivalentTo(block);
            endedEvent.Value.Duration.Ticks.Should().BePositive();
        }
    }
}
