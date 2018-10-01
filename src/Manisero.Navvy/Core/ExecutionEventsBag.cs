using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.Core
{
    public class ExecutionEventsBag
    {
        private static readonly MethodInfo CreateEventsGroupMethod
            = typeof(ExecutionEventsBag).GetMethod(nameof(CreateEventsGroup), BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>Events type -> groups of Events of given type</summary>
        private readonly IDictionary<Type, IExecutionEventsGroup> _events;

        public ExecutionEventsBag()
            : this(Enumerable.Empty<IExecutionEvents>())
        {
        }

        public ExecutionEventsBag(
            IEnumerable<IExecutionEvents> events)
        {
            _events = events
                .GroupBy(x => x.GetType())
                .ToDictionary(
                    x => x.Key,
                    x => CreateEventsGroupMethod.InvokeAsGeneric<IExecutionEventsGroup>(this, new[] { x.Key }, x));
        }

        public ExecutionEventsBag(
            ExecutionEventsBag other1,
            ExecutionEventsBag other2)
        {
            _events = other1._events.Merge(
                other2._events,
                (x, y) => x.Merge(y));
        }

        public ExecutionEventsGroup<TEvents> TryGetEvents<TEvents>()
            where TEvents : class, IExecutionEvents
        {
            var events = _events.GetValueOrDefault(typeof(TEvents));

            return events != null
                ? (ExecutionEventsGroup<TEvents>)events
                : null;
        }

        private ExecutionEventsGroup<TEvents> CreateEventsGroup<TEvents>(
            IEnumerable<IExecutionEvents> events)
            where TEvents : class, IExecutionEvents
            => new ExecutionEventsGroup<TEvents>(events.Cast<TEvents>().ToArray());
    }

    internal interface IExecutionEventsGroup
    {
        IExecutionEventsGroup Merge(
            IExecutionEventsGroup other);
    }

    public class ExecutionEventsGroup<TEvents> : IExecutionEventsGroup
        where TEvents : class, IExecutionEvents
    {
        private readonly ICollection<TEvents> _events;

        public ExecutionEventsGroup(
            ICollection<TEvents> events)
        {
            _events = events;
        }

        public void Raise(
            Action<TEvents> raiseAction)
        {
            foreach (var e in _events)
            {
                raiseAction(e);
            }
        }

        IExecutionEventsGroup IExecutionEventsGroup.Merge(
            IExecutionEventsGroup other)
        {
            var otherGroup = (ExecutionEventsGroup<TEvents>)other;

            return new ExecutionEventsGroup<TEvents>(_events.Concat(otherGroup._events).ToArray());
        }
    }
}
