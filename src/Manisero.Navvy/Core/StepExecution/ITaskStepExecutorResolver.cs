using Manisero.Navvy.Core.Models;

namespace Manisero.Navvy.Core.StepExecution
{
    public interface ITaskStepExecutorResolver
    {
        ITaskStepExecutor<TTaskStep> Resolve<TTaskStep>()
            where TTaskStep : ITaskStep;
    }
}
