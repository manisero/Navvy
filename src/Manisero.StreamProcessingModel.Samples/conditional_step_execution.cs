using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.Executors;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Models.TaskSteps;
using Manisero.StreamProcessingModel.Samples.Utils;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples
{
    public class conditional_step_execution
    {
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private bool _testedStepExecuted;

        [Theory]
        [InlineData(ResolverType.Sequential, TaskOutcome.Successful, TaskOutcome.Successful, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Successful, TaskOutcome.Canceled, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Successful, TaskOutcome.Failed, TaskOutcome.Failed)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, TaskOutcome.Successful, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, TaskOutcome.Canceled, TaskOutcome.Canceled)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, TaskOutcome.Failed, TaskOutcome.Failed)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Failed, TaskOutcome.Successful, TaskOutcome.Failed)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Failed, TaskOutcome.Canceled, TaskOutcome.Failed)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Failed, TaskOutcome.Failed, TaskOutcome.Failed)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Successful, TaskOutcome.Successful, TaskOutcome.Canceled)]
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

            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    GetStepForOutcome(firstStepOutcome),
                    GetStepForOutcome(secondStepOutcome),
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

            var progress = new Progress<TaskProgress>(_ => { });
            var executor = new TaskExecutor(TaskExecutorResolvers.Get(resolverType));

            // Act
            executor.Execute(taskDescription, progress, _cancellationSource.Token);

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
        public void default_condition_is_only_finished(
            ResolverType resolverType,
            TaskOutcome precedingStepOutcome,
            bool testedStepExecuted)
        {
            var testedStep = new BasicTaskStep(
                "TestedStep",
                () => { _testedStepExecuted = true; });

            test_conditional_execution(resolverType, precedingStepOutcome, testedStep, testedStepExecuted);
        }

        [Theory]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, TaskOutcome.Canceled, true)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, TaskOutcome.Successful, false)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Canceled, TaskOutcome.Canceled, true)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Canceled, TaskOutcome.Successful, false)]
        public void conditon_specified(
            ResolverType resolverType,
            TaskOutcome precedingStepOutcome,
            TaskOutcome outcomeToExecuteOn,
            bool testedStepExecuted)
        {
            var testedStep = new BasicTaskStep(
                "TestedStep",
                () => { _testedStepExecuted = true; },
                x => x == outcomeToExecuteOn);

            test_conditional_execution(resolverType, precedingStepOutcome, testedStep, testedStepExecuted);
        }

        private void test_conditional_execution(
            ResolverType resolverType,
            TaskOutcome precedingStepOutcome,
            ITaskStep testedStep,
            bool testedStepExecuted)
        {
            // Arrange
            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    GetStepForOutcome(precedingStepOutcome),
                    testedStep
                }
            };

            var progress = new Progress<TaskProgress>(_ => { });
            var executor = new TaskExecutor(TaskExecutorResolvers.Get(resolverType));

            // Act
            executor.Execute(taskDescription, progress, _cancellationSource.Token);

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
