using Manisero.Navvy.Core.StepExecution;

namespace Manisero.Navvy.BasicProcessing
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
