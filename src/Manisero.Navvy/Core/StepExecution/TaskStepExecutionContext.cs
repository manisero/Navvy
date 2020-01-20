namespace Manisero.Navvy.Core.StepExecution
{
    public class TaskStepExecutionContext
    {
        public TaskStepExecutionContext(
            TaskDefinition task,
            ExecutionEventsBag eventsBag,
            TaskOutcome outcomeSoFar)
        {
            Task = task;
            EventsBag = eventsBag;
            OutcomeSoFar = outcomeSoFar;
        }

        public TaskDefinition Task { get; }

        public ExecutionEventsBag EventsBag { get; }

        public TaskOutcome OutcomeSoFar { get; }
    }
}
