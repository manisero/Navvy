using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.Core.StepExecution
{
    public interface ITaskStepExecutorResolver
    {
        ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep;
    }
}
