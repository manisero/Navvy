using System;
using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.Core
{
    public class ExecutionEventsBag
    {
        /// <summary>Events type -> groups of Events of given type</summary>
        private readonly IDictionary<Type, ICollection<IExecutionEvents>> _events;

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
                    x => (ICollection<IExecutionEvents>)x.ToArray());
        }

        public ExecutionEventsGroup<TEvents>? TryGetEvents<TEvents>()
            where TEvents : class, IExecutionEvents
        {
            var events = _events.GetValueOrDefault(typeof(TEvents));

            if (events == null)
            {
                return null;
            }

            return new ExecutionEventsGroup<TEvents>(events.Cast<TEvents>().ToArray());
        }
    }

    public struct ExecutionEventsGroup<TEvents>
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
    }
}
