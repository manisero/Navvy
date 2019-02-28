using System;
using System.Collections.Generic;
using Manisero.Navvy.BasicProcessing;
using Manisero.Navvy.Core;
using Manisero.Navvy.Core.StepExecution;
using Manisero.Navvy.PipelineProcessing;

namespace Manisero.Navvy
{
    public interface ITaskExecutorBuilder
    {
        ITaskExecutorBuilder RegisterStepExecution(
            Type stepType,
            ITaskStepExecutorResolver resolver);

        ITaskExecutorBuilder RegisterEvents(
            params IExecutionEvents[] events);

        ITaskExecutor Build();
    }

    /// <summary>Builds default implementation of <see cref="ITaskExecutor"/>. Returned executor is stateless, can be used to execute multiple tasks.</summary>
    public class TaskExecutorBuilder : ITaskExecutorBuilder
    {
        private readonly IDictionary<Type, ITaskStepExecutorResolver> _stepExecutorResolvers
            = new Dictionary<Type, ITaskStepExecutorResolver>();

        private readonly List<IExecutionEvents> _events = new List<IExecutionEvents>();

        public TaskExecutorBuilder()
        {
            _stepExecutorResolvers.Add(typeof(BasicTaskStep), new BasicStepExecutorResolver());
            _stepExecutorResolvers.Add(typeof(PipelineTaskStep<>), new SequentialPipelineStepExecutorResolver());
        }

        public ITaskExecutorBuilder RegisterStepExecution(
            Type stepType,
            ITaskStepExecutorResolver resolver)
        {
            _stepExecutorResolvers[stepType] = resolver;
            return this;
        }

        public ITaskExecutorBuilder RegisterEvents(
            params IExecutionEvents[] events)
        {
            _events.AddRange(events);
            
            return this;
        }

        public ITaskExecutor Build()
        {
            return new TaskExecutor(
                new CompositeTaskStepExecutorResolver(_stepExecutorResolvers),
                new ExecutionEventsBag(_events));
        }
    }
}
