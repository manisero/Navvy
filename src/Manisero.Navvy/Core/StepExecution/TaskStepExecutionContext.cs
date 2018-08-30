using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.Core.StepExecution
{
    public class TaskStepExecutionContext
    {
        public TaskDefinition Task { get; set; }

        public ExecutionEventsBag EventsBag { get; set; }
    }
}
