using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.BasicProcessing;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Samples.Utils;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples
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
        public void TaskExecutionCondition_gets_most_severe_previous_step_outcome(
            ResolverType resolverType,
            TaskOutcome firstStepOutcome,
            TaskOutcome secondStepOutcome,
            TaskOutcome outcomePassedToThirdStepCondition)
        {
            // Arrange
            var actualOutcomePassedToCondition = (TaskOutcome?)null;

            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    GetStepForOutcome(firstStepOutcome),
                    GetStepForOutcome(secondStepOutcome, _ => true),
                    new BasicTaskStep(
                        "TestedStep",
                        () => { },
                        x =>
                        {
                            actualOutcomePassedToCondition = x;
                            return true;
                        })
                }
            };

            // Act
            task.Execute(resolverType, cancellation: _cancellationSource);

            // Assert
            actualOutcomePassedToCondition.Should().Be(outcomePassedToThirdStepCondition);
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
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    GetStepForOutcome(precedingStepOutcome),
                    new BasicTaskStep(
                        "TestedStep",
                        () => { _testedStepExecuted = true; })
                }
            };

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
            var task = new TaskDefinition
            {
                Steps = new List<ITaskStep>
                {
                    new BasicTaskStep(
                        "TestedStep",
                        () => { _testedStepExecuted = true; },
                        _ => shouldExecute)
                }
            };

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
