using System.Collections.Generic;
using AndyC.Patterns.Events;
using SimpleInjector;

namespace AndyC.Patterns.SimpleInjector.Events
{
    /// <summary>
    /// This <see cref="IEventHandlerFactory"/> implementation is used to get/release
    /// handlers using Ioc.
    /// </summary>
    public class SimpleInjectorEventHandlerFactory : IEventHandlerFactory
    {
        private readonly Container container;

        /// <summary>
        /// Creates a new instance of <see cref="SimpleInjectorEventHandlerFactory"/> class.
        /// </summary>
        public SimpleInjectorEventHandlerFactory(Container container)
        {
            this.container = container;
        }

        /// <summary>
        /// Resolves handler objects from Ioc container.
        /// </summary>
        /// <returns>Resolved handler object</returns>
        public IEnumerable<IEventHandler> GetHandlers<TEvent>() where TEvent : IEvent
        {
            return container.GetAllInstances<IEventHandler<TEvent>>();
        }

        /// <summary>
        /// Releases handler object using Ioc container.
        /// </summary>
        /// <param name="handler">Handler to be released</param>
        public void ReleaseHandler(IEventHandler handler)
        {
        }
    }
}