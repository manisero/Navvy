using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using Manisero.StreamProcessingModel.BasicProcessing;
using Manisero.StreamProcessingModel.Core;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Xunit;

namespace Manisero.StreamProcessingModel.Samples
{
    public class unexpected_error_handling
    {
        private static readonly Exception Error = new Exception();

        private class ThrowingStepExecutorResolver : ITaskStepExecutorResolver
        {
            public ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
                where TTaskStep : ITaskStep
                => new ThrowingStepExecutor<TTaskStep>();
        }

        private class ThrowingStepExecutor<TTaskStep> : ITaskStepExecutor<TTaskStep>
            where TTaskStep : ITaskStep
        {
            public void Execute(TTaskStep step, TaskStepExecutionContext context, IProgress<byte> progress, CancellationToken cancellation)
                => throw Error;
        }

        [Fact]
        public void test()
        {
            // Arrange
            var taskDescription = new TaskDescription
            {
                Steps = new List<ITaskStep>
                {
                    BasicTaskStep.Empty("Step")
                }
            };
            
            var progress = new Progress<TaskProgress>(_ => { });
            var cancellationSource = new CancellationTokenSource();
            var executor = new TaskExecutor(new ThrowingStepExecutorResolver());

            // Act
            Action act = () => executor.Execute(taskDescription, progress, cancellationSource.Token);

            // Assert
            act.ShouldThrow<Exception>().Which.Should().BeSameAs(Error);
        }
    }
}
