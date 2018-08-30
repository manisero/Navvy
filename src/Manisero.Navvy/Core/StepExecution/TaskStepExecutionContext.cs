using Manisero.Navvy.Core.Models;

namespace Manisero.Navvy.Core.StepExecution
{
    public class TaskStepExecutionContext
    {
        public TaskDefinition Task { get; set; }

        public ExecutionEventsBag EventsBag { get; set; }
    }
}
