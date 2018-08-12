using Manisero.StreamProcessingModel.Executors.StepExecutors;
using Manisero.StreamProcessingModel.Models;

namespace Manisero.StreamProcessingModel.Executors.StepExecutorResolvers
{
    public class BasicStepExecutorResolver : ITaskStepExecutorResolver
    {
        private readonly BasicStepExecutor _executor = new BasicStepExecutor();

        public ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep
        {
            return (ITaskStepExecutor<TTaskStep>)_executor;
        }
    }
}
