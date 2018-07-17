using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.TaskExecution.Models
{
    public class TaskDescription
    {
        public IList<ITaskStep> Steps { get; set; }
    }
}
