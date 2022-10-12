using System.Threading;
using System.Threading.Tasks;

namespace Manisero.Navvy.Core.StepExecution
{
    public interface ITaskStepExecutor<TTaskStep>
        where TTaskStep : ITaskStep
    {
        Task Execute(
            TTaskStep step,
            TaskStepExecutionContext context,
            CancellationToken cancellation);
    }
}
