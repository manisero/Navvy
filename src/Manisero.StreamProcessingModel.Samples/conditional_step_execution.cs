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
        [InlineData(ResolverType.Sequential, TaskOutcome.Finished, true)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Canceled, false)]
        [InlineData(ResolverType.Sequential, TaskOutcome.Failed, false)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Finished, true)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Canceled, false)]
        [InlineData(ResolverType.Streaming, TaskOutcome.Failed, false)]
        public void basic__default(
            ResolverType resolverType,
            TaskOutcome precedingStepOutcome,
            bool testedStepExecuted)
        {
            var testedStep = new BasicTaskStep(
                "TestedStep",
                () => { _testedStepExecuted = true; });

            test(resolverType, precedingStepOutcome, testedStep, testedStepExecuted);
        }

        private void test(
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
                    new BasicTaskStep(
                        "PrecedingStep",
                        () =>
                        {
                            switch (precedingStepOutcome)
                            {
                                case TaskOutcome.Finished:
                                    return;
                                case TaskOutcome.Canceled:
                                    _cancellationSource.Cancel();
                                    return;
                                case TaskOutcome.Failed:
                                    throw new Exception();
                            }
                        }),
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
    }
}
