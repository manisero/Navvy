namespace Manisero.StreamProcessingModel.Models
{
    public class TaskResult
    {
        public TaskOutcome Outcome { get; set; }
        public TaskExecutionException Error { get; set; }

        public static TaskResult Successful()
            => new TaskResult
            {
                Outcome = TaskOutcome.Successful
            };

        public static TaskResult Canceled()
            => new TaskResult
            {
                Outcome = TaskOutcome.Canceled
            };

        public static TaskResult Failed(TaskExecutionException error)
            => new TaskResult
            {
                Outcome = TaskOutcome.Failed,
                Error = error
            };
    }

    public enum TaskOutcome
    {
        Successful,
        Canceled,
        Failed
    }
}
