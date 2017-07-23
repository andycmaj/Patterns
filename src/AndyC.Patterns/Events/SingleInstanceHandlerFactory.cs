using System.Collections.Generic;

namespace AndyC.Patterns.Events
{
    /// <summary>
    /// This <see cref="IEventHandlerFactory"/> implementation is used to handle events
    /// by a single instance object.
    /// </summary>
    internal class SingleInstanceHandlerFactory : IEventHandlerFactory
    {
        /// <summary>
        /// The event handler instance.
        /// </summary>
        public IEventHandler HandlerInstance { get; private set; }

        public SingleInstanceHandlerFactory(IEventHandler handler)
        {
            HandlerInstance = handler;
        }

        /// <summary>
        /// Returns the provided handler instance every time.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEventHandler> GetHandlers<TEvent>() where TEvent : IEvent
        {
            return new[] { HandlerInstance };
        }

        public void ReleaseHandler(IEventHandler handler)
        {
            // No-op. provider of handler is responsible for disposing.
        }
    }
}