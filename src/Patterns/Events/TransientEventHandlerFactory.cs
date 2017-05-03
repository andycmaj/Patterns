using System;
using System.Collections.Generic;

namespace Patterns.Events
{
    /// <summary>
    /// This <see cref="IEventHandlerFactory"/> creates a new <see cref="IEventHandler"/>
    /// instance to handle each triggered event.
    /// </summary>
    internal class TransientEventHandlerFactory<THandler> : IEventHandlerFactory
        where THandler : IEventHandler, new()
    {
        /// <summary>
        /// Creates a new instance of the handler object every time.
        /// </summary>
        /// <returns>The handler object</returns>
        public IEnumerable<IEventHandler> GetHandlers<TEvent>() where TEvent : IEvent
        {
            return new[] { new THandler() as IEventHandler };
        }

        /// <summary>
        /// Disposes the handler object if it's <see cref="IDisposable"/>.
        /// Does nothing if it's not.
        /// </summary>
        /// <param name="handler">Handler to be released</param>
        public void ReleaseHandler(IEventHandler handler)
        {
            if (handler is IDisposable)
            {
                (handler as IDisposable).Dispose();
            }
        }
    }
}