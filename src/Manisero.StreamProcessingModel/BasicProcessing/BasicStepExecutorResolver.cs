using Manisero.StreamProcessingModel.Core.Models;
using Manisero.StreamProcessingModel.Core.StepExecution;

namespace Manisero.StreamProcessingModel.BasicProcessing
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
