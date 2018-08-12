using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.Core.Models
{
    public class TaskDescription
    {
        public IList<ITaskStep> Steps { get; set; }
    }
}
