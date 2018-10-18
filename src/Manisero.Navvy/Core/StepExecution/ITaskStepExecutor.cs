using System.Threading;

namespace Manisero.Navvy.Core.StepExecution
{
    public interface ITaskStepExecutor<TTaskStep>
        where TTaskStep : ITaskStep
    {
        void Execute(
            TTaskStep step,
            TaskStepExecutionContext context,
            CancellationToken cancellation);
    }
}
