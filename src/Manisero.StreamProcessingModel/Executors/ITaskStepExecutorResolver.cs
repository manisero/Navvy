using Manisero.StreamProcessingModel.Models;

namespace Manisero.StreamProcessingModel.Executors
{
    public interface ITaskStepExecutorResolver
    {
        ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep;
    }
}
