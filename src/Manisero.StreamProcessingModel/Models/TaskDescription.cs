using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.Models
{
    public class TaskDescription
    {
        public IList<ITaskStep> Steps { get; set; }
    }
}
