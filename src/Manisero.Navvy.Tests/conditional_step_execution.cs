using System;
using System.Threading;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Tests.Utils;
using Xunit;

namespace Manisero.Navvy.Tests
{
    public class conditional_step_execution
    {
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private bool _testedStepExecuted;

        [Theory]
        [InlineData(ResolverType.Sequential, TaskOutcome.Successful, TaskOutcome.Successful, TaskOutcome.Successful)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Successful, TaskOutcome.Canceled, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Successful, TaskOutcome.Failed, TaskOutcome.Failed)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, TaskOutcome.Successful, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, TaskOutcome.Canceled, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, TaskOutcome.Failed, TaskOutcome.Failed)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Failed, TaskOutcome.Successful, TaskOutcome.Failed)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Failed, TaskOutcome.Canceled, TaskOutcome.Failed)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Failed, TaskOutcome.Failed, TaskOutcome.Failed)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Successful, TaskOutcome.Successful, TaskOutcome.Successful)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Successful, TaskOutcome.Canceled, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Successful, TaskOutcome.Failed, TaskOutcome.Failed)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Canceled, TaskOutcome.Successful, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Canceled, TaskOutcome.Canceled, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Canceled, TaskOutcome.Failed, TaskOutcome.Failed)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Failed, TaskOutcome.Successful, TaskOutcome.Failed)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Failed, TaskOutcome.Canceled, TaskOutcome.Failed)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Failed, TaskOutcome.Failed, TaskOutcome.Failed)]
        public void TaskExecutionCondition_and_BasicTaskStep_body_get_most_severe_previous_step_outcome(
            ResolverType resolverType,
            TaskOutcome firstStepOutcome,
            TaskOutcome secondStepOutcome,
            TaskOutcome outcomePassedToThirdStepCondition)
        {
            // Arrange
            var actualOutcomePassedToBody = (TaskOutcome?)null;
            var actualOutcomePassedToCondition = (TaskOutcome?)null;

            var task = new TaskDefinition(
                GetStepForOutcome(firstStepOutcome),
                GetStepForOutcome(secondStepOutcome, TaskStepUtils.AlwaysExecuteCondition),
                new BasicTaskStep(
                    "TestedBody",
                    x => actualOutcomePassedToBody = x),
                new BasicTaskStep(
                    "TestedConditionalStep",
                    () => { },
                    x =>
                    {
                        actualOutcomePassedToCondition = x;
                        return true;
                    }));

            // Act
            task.Execute(resolverType, cancellation: _cancellationSource);

            // Assert
            actualOutcomePassedToBody.Should().Be(outcomePassedToThirdStepCondition, nameof(actualOutcomePassedToBody));
            actualOutcomePassedToCondition.Should().Be(outcomePassedToThirdStepCondition, nameof(actualOutcomePassedToCondition));
        }

        [Theory]
        [InlineData(ResolverType.Sequential, TaskOutcome.Successful, true)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, false)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Failed, false)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Successful, true)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Canceled, false)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Failed, false)]
        public void default_condition___executed_only_if_successful(
            ResolverType resolverType,
            TaskOutcome precedingStepOutcome,
            bool testedStepExecuted)
        {
            var task = new TaskDefinition(
                GetStepForOutcome(precedingStepOutcome),
                new BasicTaskStep(
                    "TestedStep",
                    () => { _testedStepExecuted = true; }));

            test_conditional_execution(resolverType, task, testedStepExecuted);
        }

        [Theory]
        [InlineData(ResolverType.Sequential, true)]
        [InlineData(ResolverType.Sequential, false)]
        [InlineData(ResolverType.Streaming, true)]
        [InlineData(ResolverType.Streaming, false)]
        public void custom_conditon(
            ResolverType resolverType,
            bool shouldExecute)
        {
            var task = new TaskDefinition(
                new BasicTaskStep(
                    "TestedStep",
                    () => { _testedStepExecuted = true; },
                    _ => shouldExecute));

            test_conditional_execution(resolverType, task, shouldExecute);
        }

        private void test_conditional_execution(
            ResolverType resolverType,
            TaskDefinition task,
            bool testedStepExecuted)
        {
            // Act
            task.Execute(resolverType, cancellation: _cancellationSource);

            // Assert
            _testedStepExecuted.Should().Be(testedStepExecuted);
        }

        private ITaskStep GetStepForOutcome(
            TaskOutcome outcome,
            Func<TaskOutcome, bool> executionCondition = null)
        {
            return new BasicTaskStep(
                Guid.NewGuid().ToString(),
                () =>
                {
                    switch (outcome)
                    {
                        case TaskOutcome.Successful:
                            return;
                        case TaskOutcome.Canceled:
                            _cancellationSource.Cancel();
                            return;
                        case TaskOutcome.Failed:
                            throw new Exception();
                    }
                },
                executionCondition);
        }
    }
}
