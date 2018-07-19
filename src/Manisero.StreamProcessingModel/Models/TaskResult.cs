using System;

namespace Manisero.StreamProcessingModel.Models
{
    public class TaskResult
    {
        public TaskOutcome Outcome { get; set; }
        public Exception Error { get; set; }

        public static TaskResult Finished()
            => new TaskResult
            {
                Outcome = TaskOutcome.Finished
            };

        public static TaskResult Canceled()
            => new TaskResult
            {
                Outcome = TaskOutcome.Canceled
            };

        public static TaskResult Failed(Exception error)
            => new TaskResult
            {
                Outcome = TaskOutcome.Failed,
                Error = error
            };
    }

    public enum TaskOutcome
    {
        Finished,
        Canceled,
        Failed
    }
}
