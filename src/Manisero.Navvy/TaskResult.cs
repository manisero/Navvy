using System.Collections.Generic;
using System.Linq;

namespace Manisero.Navvy
{
    /// <summary>Result of a task execution, returned by <see cref="ITaskExecutor"/>. Remember to check the <see cref="Outcome"/> after each task execution.</summary>
    public class TaskResult
    {
        /// <summary>Final status of the task.</summary>
        public TaskOutcome Outcome { get; }

        /// <summary>Any exceptions caught during task execution. If <see cref="Outcome"/> != <see cref="TaskOutcome.Failed"/>, null.</summary>
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
}
