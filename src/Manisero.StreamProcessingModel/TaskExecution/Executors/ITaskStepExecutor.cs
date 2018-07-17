using Manisero.StreamProcessingModel.TaskExecution.Models;

namespace Manisero.StreamProcessingModel.TaskExecution.Executors
{
    public interface ITaskStepExecutor<TTaskStep>
        where TTaskStep : ITaskStep
    {
        void Execute(TTaskStep step);
    }
}