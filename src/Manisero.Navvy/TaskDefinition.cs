using System.Collections.Generic;
using Manisero.Navvy.Core;

namespace Manisero.Navvy
{
    /// <summary>Definition of the task to be executed. It can be executed by <see cref="ITaskExecutor"/>.</summary>
    public class TaskDefinition
    {
        public string Name { get; }

        public IReadOnlyList<ITaskStep> Steps { get; }

        /// <summary>Extra state managed by extensions, e.g. execution log.</summary>
        public TaskExtras Extras { get; } = new TaskExtras();

        public TaskDefinition(
            string name,
            IReadOnlyList<ITaskStep> steps)
        {
            Name = name;
            Steps = steps;
        }

        public TaskDefinition(
            string name,
            params ITaskStep[] steps)
            : this(name, (IReadOnlyList<ITaskStep>)steps)
        {
        }

        public TaskDefinition(
            IReadOnlyList<ITaskStep> steps)
            : this(null, steps)
        {
            Steps = steps;
        }

        public TaskDefinition(
            params ITaskStep[] steps)
            : this((IReadOnlyList<ITaskStep>)steps)
        {
        }
    }
}
