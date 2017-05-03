using System;

namespace Patterns.Events
{
    /// <summary>
    /// Defines interface for all Event types.
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// The time when the event occured.
        /// </summary>
        DateTime EventTime { get; }

        /// <summary>
        /// The object which triggers the event (optional).
        /// </summary>
        object EventSource { get; }
    }
}
