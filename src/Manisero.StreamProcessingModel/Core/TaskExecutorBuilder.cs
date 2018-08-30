using System;
using System.Collections.Generic;
using Manisero.StreamProcessingModel.BasicProcessing;
using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;
using Manisero.StreamProcessingModel.PipelineProcessing;
using Manisero.StreamProcessingModel.PipelineProcessing.Sequential;

namespace Manisero.StreamProcessingModel.Core
{
    public interface ITaskExecutorBuilder
    {
        void RegisterStepExecutorResolver(
            Type stepType,
            ITaskStepExecutorResolver resolver);

        void RegisterEvents(
            IExecutionEvents events);

        ITaskExecutor Build();
    }

    public class TaskExecutorBuilder : ITaskExecutorBuilder
    {
        private readonly IDictionary<Type, ITaskStepExecutorResolver> _stepExecutorResolvers
            = new Dictionary<Type, ITaskStepExecutorResolver>();

        private readonly IDictionary<Type, IExecutionEvents> _events
            = new Dictionary<Type, IExecutionEvents>();

        public TaskExecutorBuilder()
        {
            _stepExecutorResolvers.Add(typeof(BasicTaskStep), new BasicStepExecutorResolver());
            _stepExecutorResolvers.Add(typeof(PipelineTaskStep<>), new SequentialPipelineStepExecutorResolver());
        }

        public void RegisterStepExecutorResolver(
            Type stepType,
            ITaskStepExecutorResolver resolver)
        {
            _stepExecutorResolvers.Add(stepType, resolver);
        }

        public void RegisterEvents(
            IExecutionEvents events)
        {
            _events.Add(events.GetType(), events);
        }

        public ITaskExecutor Build()
        {
            return new TaskExecutor(
                new CompositeTaskStepExecutorResolver(_stepExecutorResolvers),
                new ExecutionEventsBag(_events.Values));
        }
    }
}
