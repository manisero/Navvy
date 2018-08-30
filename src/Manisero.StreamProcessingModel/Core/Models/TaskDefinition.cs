using System.Collections.Generic;

namespace Manisero.StreamProcessingModel.Core.Models
{
    public class TaskDefinition
    {
        public IList<ITaskStep> Steps { get; set; }
    }
}
