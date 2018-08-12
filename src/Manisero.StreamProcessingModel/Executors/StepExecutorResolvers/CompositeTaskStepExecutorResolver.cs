using System;
using System.Collections.Generic;
using Manisero.StreamProcessingModel.Models;
using Manisero.StreamProcessingModel.Utils;

namespace Manisero.StreamProcessingModel.Executors.StepExecutorResolvers
{
    public class CompositeTaskStepExecutorResolver : ITaskStepExecutorResolver
    {
        private readonly IDictionary<Type, ITaskStepExecutorResolver> _specificResolvers;

        public CompositeTaskStepExecutorResolver(
            IDictionary<Type, ITaskStepExecutorResolver> specificResolvers)
        {
            _specificResolvers = specificResolvers;
        }

        public ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep
        {
            var stepType = typeof(TTaskStep);
            var resolver = TryGetSpecificResolver(stepType);

            if (resolver == null)
            {
                throw new NotSupportedException($"{stepType} task step type not supported.");
            }

            return resolver.Resolve<TTaskStep>();
        }

        private ITaskStepExecutorResolver TryGetSpecificResolver(
            Type stepType)
        {
            var resolver = _specificResolvers.GetValueOrDefault(stepType);

            if (resolver == null && stepType.IsConstructedGenericType)
            {
                resolver = _specificResolvers.GetValueOrDefault(stepType.GetGenericTypeDefinition());
            }

            return resolver;
        }
    }
}
