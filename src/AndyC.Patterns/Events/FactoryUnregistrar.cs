using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AndyC.Patterns.Events
{
    /// <summary>
    /// Used to unregister a <see cref="IEventHandlerFactory"/> on <see cref="Dispose"/> method.
    /// </summary>
    internal class FactoryUnregistrar : IDisposable
    {
        private readonly IEventBus events;
        private readonly Type eventType;
        private readonly IEventHandlerFactory factory;

        public FactoryUnregistrar(IEventBus events, Type eventType, IEventHandlerFactory factory)
        {
            this.events = events;
            this.eventType = eventType;
            this.factory = factory;
        }

        public void Dispose()
        {
            events.Unregister(eventType, factory);
        }
    }
}
