using System.Collections.Generic;

namespace Manisero.Navvy.Core.Models
{
    public class TaskDefinition
    {
        public IList<ITaskStep> Steps { get; set; }
    }
}
