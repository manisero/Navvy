using System.Collections.Generic;

namespace Manisero.Navvy
{
    public class TaskDefinition
    {
        public IList<ITaskStep> Steps { get; }

        public TaskDefinition(
            IList<ITaskStep> steps)
        {
            Steps = steps;
        }

        public TaskDefinition(
            params ITaskStep[] steps)
            : this((IList<ITaskStep>)steps)
        {
        }
    }
}
