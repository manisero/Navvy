using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
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
        public async Task basic_automatic(ResolverType resolverType)
        {
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Basic(
                    "Cancel",
                    _cancellationSource.Cancel),
                TaskStepBuilder.Build.Basic(
                    "Complete",
                    () => { _completed = true; }));

            await test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task basic_manual(ResolverType resolverType)
        {
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Basic(
                    "Cancel",
                    (p, c) =>
                    {
                        _cancellationSource.Cancel();
                        c.ThrowIfCancellationRequested();
                        _completed = true;
                    }));

            await test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline___within_single_block_between_items(ResolverType resolverType)
        {
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        "Step")
                    .WithInput(new[] { 0, 1 })
                    .WithBlock(
                        "Cancel / Complete",
                        x =>
                        {
                            if (x == 0)
                            {
                                _cancellationSource.Cancel();
                            }
                            else
                            {
                                _completed = true;
                            }
                        })
                    .Build());

            await test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline___between_blocks(ResolverType resolverType)
        {
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        "Step")
                    .WithInput(new[] { 0 })
                    .WithBlock(
                        "Cancel",
                        x => { _cancellationSource.Cancel(); })
                    .WithBlock(
                        "Complete",
                        x => { _completed = true; })
                    .Build());

            await test(task, resolverType);
        }

        private async Task test(
            TaskDefinition task,
            ResolverType resolverType)
        {
            // Act
            var act = () => task.Execute(resolverType, cancellation: _cancellationSource);

            // Assert
            await act.Should().ThrowAsync<OperationCanceledException>();
            _completed.Should().Be(false);
        }
    }
}
