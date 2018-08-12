using System.Collections.Generic;
using System.Linq;

namespace Manisero.StreamProcessingModel.Models
{
    public class TaskResult
    {
        public TaskOutcome Outcome { get; }
        public IReadOnlyCollection<TaskExecutionException> Errors { get; }

        public TaskResult(
            bool canceled,
            IReadOnlyCollection<TaskExecutionException> errors = null)
        {
            Errors = errors?.Any() == true
                ? errors
                : null;

            Outcome = Errors != null
                ? TaskOutcome.Failed
                : canceled
                    ? TaskOutcome.Canceled
                    : TaskOutcome.Successful;
        }
    }

    public enum TaskOutcome
    {
        Successful = 1,
        Canceled = 2,
        Failed = 3
    }
}
