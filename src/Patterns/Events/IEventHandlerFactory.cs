using System.Collections.Generic;

namespace Patterns.Events
{
    public interface IEventHandlerFactory
    {
        /// <summary>
        /// Gets the event handlers for the specified event type
        /// </summary>
        /// <returns>The event handlers</returns>
        IEnumerable<IEventHandler> GetHandlers<TEvent>() where TEvent : IEvent;

        /// <summary>
        /// Releases an event handler.
        /// </summary>
        /// <param name="handler">Handle to be released</param>
        void ReleaseHandler(IEventHandler handler);
    }
}