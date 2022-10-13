using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.PipelineProcessing;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests
{
    public class error_handling
    {
        private const string FailingStepName = "Failing Step";
        private readonly Exception _error = new Exception();

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task basic(ResolverType resolverType)
        {
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Basic(
                    FailingStepName,
                    () => throw _error));
            
            await test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential, 0)]
        [InlineData(ResolverType.Sequential, 1)]
        [InlineData(ResolverType.Streaming, 0)]
        [InlineData(ResolverType.Streaming, 1)]
        public async Task pipeline___catches_error_in_input_materialization(
            ResolverType resolverType,
            int invalidItemIndex)
        {
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        FailingStepName)
                    .WithInput(
                        new[] { 0, 1, 2 }.Select((x, i) =>
                        {
                            if (i == invalidItemIndex)
                            {
                                throw _error;
                            }

                            return x;
                        }),
                        3)
                    .WithBlock(
                        "Block",
                        _ => { })
                    .Build());

            await test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline___catches_error_in_first_block(
            ResolverType resolverType)
        {
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        FailingStepName)
                    .WithInput(new[] { 0, 1, 2 })
                    .WithBlock(
                        "Block",
                        _ => throw _error)
                    .Build());
            
            await test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline___catches_error_in_non_first_block(
            ResolverType resolverType)
        {
            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        FailingStepName)
                    .WithInput(new[] { 0, 1, 2 })
                    .WithBlock(
                        "Block",
                        _ => { })
                    .WithBlock(
                        "Errors",
                        _ => throw _error)
                    .Build());

            await test(task, resolverType);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline___item_following_invalid_item_is_not_processed(
            ResolverType resolverType)
        {
            var completed = false;

            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        FailingStepName)
                    .WithInput(new[] { 0, 1, 2 })
                    .WithBlock(
                        "Errors / Complete",
                        x =>
                        {
                            if (x == 0)
                            {
                                throw _error;
                            }
                            else
                            {
                                completed = true;
                            }
                        })
                    .Build());

            await test(task, resolverType);

            completed.Should().Be(false);
        }

        [Theory]
        [InlineData(ResolverType.Sequential)]
        [InlineData(ResolverType.Streaming)]
        public async Task pipeline___invalid_item_is_not_further_processed(
            ResolverType resolverType)
        {
            var completed = false;

            var task = new TaskDefinition(
                TaskStepBuilder.Build.Pipeline<int>(
                        FailingStepName)
                    .WithInput(new[] { 0, 1, 2 })
                    .WithBlock(
                        "Errors",
                        x => throw _error)
                    .WithBlock(
                        "Complete",
                        x => { completed = true; })
                    .Build());

            await test(task, resolverType);

            completed.Should().Be(false);
        }

        private async Task test(
            TaskDefinition task,
            ResolverType resolverType)
        {
            // Act
            var act = () => task.Execute(resolverType);

            // Assert
            var errors =
                (await act.Should().ThrowExactlyAsync<TaskExecutionException>())
                .Subject
                .ToArray();

            errors.Should().HaveCount(1);

            var error = errors.Single();
            error.StepName.Should().Be(FailingStepName);
            error.InnerException.Should().BeSameAs(_error);
        }
    }
}
