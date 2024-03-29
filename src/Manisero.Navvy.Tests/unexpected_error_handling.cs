﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core.StepExecution;
using Xunit;

namespace Manisero.Navvy.Tests
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
            public Task Execute(TTaskStep step, TaskStepExecutionContext context, CancellationToken cancellation)
                => throw Error;
        }

        [Fact]
        public async Task test()
        {
            // Arrange
            var task = new TaskDefinition(
                BasicTaskStep.Empty("Step"));
            
            var executor = new TaskExecutorBuilder()
                .RegisterStepExecution(typeof(BasicTaskStep), new ThrowingStepExecutorResolver())
                .Build();

            // Act
            Func<Task> act = () => executor.Execute(task);

            // Assert
            (await act.Should().ThrowAsync<Exception>()).Which.Should().BeSameAs(Error);
        }
    }
}
