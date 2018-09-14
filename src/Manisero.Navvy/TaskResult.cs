using System.Collections.Generic;
using System.Linq;

namespace Manisero.Navvy
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
}
