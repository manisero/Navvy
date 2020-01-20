namespace Manisero.Navvy.Core.StepExecution
{
    public class TaskStepExecutionContext
    {
        public TaskStepExecutionParameters Parameters { get; }

        public TaskExecutionState State { get; }

        public TaskStepExecutionContext(
            TaskStepExecutionParameters parameters,
            TaskExecutionState state)
        {
            Parameters = parameters;
            State = state;
        }
    }

    public class TaskStepExecutionParameters
    {
        public TaskDefinition Task { get; set; }

        public ExecutionEventsBag EventsBag { get; set; }
    }

    public class TaskExecutionState
    {
        public TaskOutcome OutcomeSoFar { get; set; }
    }
}
