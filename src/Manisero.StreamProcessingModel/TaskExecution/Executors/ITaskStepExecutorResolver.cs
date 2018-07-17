using Manisero.StreamProcessingModel.TaskExecution.Models;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors
{
    public interface ITaskStepExecutorResolver
    {
        ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep;
    }
}
