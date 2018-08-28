using Manisero.StreamProcessingModel.Core.Models;

namespace Manisero.StreamProcessingModel.Core.StepExecution
{
    public class TaskStepExecutionContext
    {
        public TaskDescription TaskDescription { get; set; }

        public ExecutionEventsBag EventsBag { get; set; }
    }
}
