using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.Models;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests
{
    public class cancellation
    {
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private bool _completed;

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void basic(ResolverType resolverType)
        {
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "Cancel",
                        _cancellationSource.Cancel),
                    new BasicTaskStep(
                        "Complete",
                        () => { _completed = true; })
                }
            };

            test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___within_single_block_between_batches(ResolverType resolverType)
        {
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        "Step",
                        new[]
                        {
                            new[] { 0 },
                            new[] { 1 }
                        },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.BatchBody(
                                "Cancel / Complete",
                                x =>
                                {
                                    if (x.Contains(0))
                                    {
                                        _cancellationSource.Cancel();
                                    }
                                    else
                                    {
                                        _completed = true;
                                    }
                                })
                        })
                }
            };

            test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public void pipeline___between_blocks(ResolverType resolverType)
        {
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new PipelineTaskStep<int>(
                        "Step",
                        new[] { new[] { 0 } },
                        new List<PipelineBlock<int>>
                        {
                            PipelineBlock<int>.BatchBody(
                                "Cancel",
                                x => { _cancellationSource.Cancel(); }),
                            PipelineBlock<int>.BatchBody(
                                "Complete",
                                x => { _completed = true; })
                        })
                }
            };

            test(task, resolverType);
        }

        private void test(
            TaskDefinition task,
            ResolverType resolverType)
        {
            // Act
            var result = task.Execute(resolverType, cancellation: _cancellationSource);

            // Assert
            result.Outcome.Should().Be(TaskOutcome.Canceled);
            _completed.Should().Be(false);
        }
    }
}
