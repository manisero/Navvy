using System;
using System.Collections.Generic;
using System.Linq;
using Manisero.Navvy.Utils;

namespace Manisero.Navvy.Core.Models
{
    public class ExecutionEventsBag
    {
        /// <summary>Events type -> Events</summary>
        private readonly IDictionary<Type, IExecutionEvents> _events;

        public ExecutionEventsBag()
            : this(Enumerable.Empty<IExecutionEvents>())
        {
        }

        public ExecutionEventsBag(
            IEnumerable<IExecutionEvents> events)
        {
            _events = events.ToDictionary(x => x.GetType());
        }

        public TEvents TryGetEvents<TEvents>()
            where TEvents : class, IExecutionEvents
            => (TEvents)_events.GetValueOrDefault(typeof(TEvents));
    }
}
