using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Manisero.Navvy
{
    public class TaskDefinition
    {
        public string Name { get; }

        public IList<ITaskStep> Steps { get; }

        public IDictionary<string, object> Extras { get; } = new ConcurrentDictionary<string, object>();

        public TaskDefinition(
            string name,
            IList<ITaskStep> steps)
        {
            Name = name;
            Steps = steps;
        }

        public TaskDefinition(
            string name,
            params ITaskStep[] steps)
            : this(name, (IList<ITaskStep>)steps)
        {
        }

        public TaskDefinition(
            IList<ITaskStep> steps)
            : this(null, steps)
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
