using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AndyC.Patterns.Events
{
    /// <summary>
    /// Implements EventBus as Singleton pattern.
    /// </summary>
    public class EventBus : IEventBus
    {
        /// <summary>
        /// Gets the default <see cref="EventBus"/> instance.
        /// </summary>
        public static EventBus Default { get { return DefaultInstance; } }
        private static readonly EventBus DefaultInstance = new EventBus();

        /// <summary>
        /// All registered handler factories.
        /// Key: Type of the event
        /// Value: List of handler factories
        /// </summary>
        private readonly Dictionary<Type, List<IEventHandlerFactory>> _handlerFactories;

        /// <summary>
        /// Creates a new <see cref="EventBus"/> instance.
        /// Instead of creating a new instace, you can use <see cref="Default"/> to use Global <see cref="EventBus"/>.
        /// </summary>
        public EventBus()
        {
            _handlerFactories = new Dictionary<Type, List<IEventHandlerFactory>>();
        }

        /// <inheritdoc/>
        public IDisposable Register<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            return Register(typeof(TEvent), new ActionEventHandler<TEvent>(action));
        }

        /// <inheritdoc/>
        public IDisposable Register<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
        {
            return Register(typeof(TEvent), handler);
        }

        /// <inheritdoc/>
        public IDisposable Register<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>, new()
        {
            return Register(typeof(TEvent), new TransientEventHandlerFactory<THandler>());
        }

        /// <inheritdoc/>
        public IDisposable Register(Type eventType, IEventHandler handler)
        {
            return Register(eventType, new SingleInstanceHandlerFactory(handler));
        }

        /// <inheritdoc/>
        public IDisposable Register<TEvent>(IEventHandlerFactory handlerFactory) where TEvent : IEvent
        {
            return Register(typeof(TEvent), handlerFactory);
        }

        /// <inheritdoc/>
        public IDisposable Register(Type eventType, IEventHandlerFactory handlerFactory)
        {
            lock (_handlerFactories)
            {
                GetOrCreateHandlerFactories(eventType).Add(handlerFactory);
                return new FactoryUnregistrar(this, eventType, handlerFactory);
            }
        }

        /// <inheritdoc/>
        public void Unregister<TEvent>(Action<TEvent> action) where TEvent : IEvent
        {
            lock (_handlerFactories)
            {
                GetOrCreateHandlerFactories(typeof(TEvent))
                    .RemoveAll(
                        factory =>
                        {
                            if (factory is SingleInstanceHandlerFactory)
                            {
                                var singleInstanceFactory = factory as SingleInstanceHandlerFactory;
                                if (singleInstanceFactory.HandlerInstance is ActionEventHandler<TEvent>)
                                {
                                    var actionHandler = singleInstanceFactory.HandlerInstance as ActionEventHandler<TEvent>;
                                    if (actionHandler.Action == action)
                                    {
                                        return true;
                                    }
                                }
                            }

                            return false;
                        });
            }
        }

        /// <inheritdoc/>
        public void Unregister<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent
        {
            Unregister(typeof(TEvent), handler);
        }

        /// <inheritdoc/>
        public void Unregister(Type eventType, IEventHandler handler)
        {
            lock (_handlerFactories)
            {
                GetOrCreateHandlerFactories(eventType)
                    .RemoveAll(
                        factory =>
                        factory is SingleInstanceHandlerFactory && (factory as SingleInstanceHandlerFactory).HandlerInstance == handler
                    );
            }
        }

        /// <inheritdoc/>
        public void Unregister<TEvent>(IEventHandlerFactory factory) where TEvent : IEvent
        {
            Unregister(typeof(TEvent), factory);
        }

        /// <inheritdoc/>
        public void Unregister(Type eventType, IEventHandlerFactory factory)
        {
            lock (_handlerFactories)
            {
                GetOrCreateHandlerFactories(eventType).Remove(factory);
            }
        }

        /// <inheritdoc/>
        public void UnregisterAll<TEvent>() where TEvent : IEvent
        {
            UnregisterAll(typeof(TEvent));
        }

        /// <inheritdoc/>
        public void UnregisterAll(Type eventType)
        {
            lock (_handlerFactories)
            {
                GetOrCreateHandlerFactories(eventType).Clear();
            }
        }

        /// <inheritdoc/>
        public void Trigger<TEvent>(TEvent @event) where TEvent : IEvent
        {
            var eventType = typeof(TEvent);

            foreach (var factoryToTrigger in GetHandlerFactories(eventType))
            {
                var eventHandlers = factoryToTrigger.GetHandlers<TEvent>();
                if (eventHandlers == null || !eventHandlers.Any())
                {
                    throw new Exception(
                        $"Registered event handler for event type {eventType.Name} does not implement " +
                        $"IEventHandler<{eventType.Name}> interface!"
                    );
                }

                foreach (var eventHandler in eventHandlers)
                {
                    var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);

                    var handleMethod = HandleEventMethodTemplate.MakeGenericMethod(eventType);

                    try
                    {
                        handleMethod.Invoke(null, new object[] { eventHandler, @event });
                    }
                    catch (TargetInvocationException ex)
                    {
                        throw new EventHandlingException(@event, ex.InnerException);
                    }
                    finally
                    {
                        factoryToTrigger.ReleaseHandler(eventHandler);
                    }
                }
            }
        }

        private IEnumerable<IEventHandlerFactory> GetHandlerFactories(Type eventType)
        {
            var handlerFactoryList = new List<IEventHandlerFactory>();

            lock (_handlerFactories)
            {
                foreach (var handlerFactory in _handlerFactories.Where(hf => ShouldTriggerEventForHandler(eventType, hf.Key)))
                {
                    handlerFactoryList.AddRange(handlerFactory.Value);
                }
            }

            return handlerFactoryList.ToArray();
        }

        private static bool ShouldTriggerEventForHandler(Type eventType, Type handlerType)
        {
            //Should trigger same type
            if (handlerType == eventType)
            {
                return true;
            }

            //Should trigger
            if (handlerType.IsAssignableFrom(eventType))
            {
                return true;
            }

            return false;
        }

        private static readonly MethodInfo HandleEventMethodTemplate =
            ReflectionHelper.GetGenericMethodDefinition(() =>
                HandleEventShim<IEvent>(null, null)
            );

        private static void HandleEventShim<TEvent>(
            IEventHandler<TEvent> eventHandler,
            TEvent @event
        ) where TEvent : IEvent
        {
            eventHandler.HandleEvent(@event);
        }

        private List<IEventHandlerFactory> GetOrCreateHandlerFactories(Type eventType)
        {
            List<IEventHandlerFactory> handlers;
            if (!_handlerFactories.TryGetValue(eventType, out handlers))
            {
                _handlerFactories[eventType] = handlers = new List<IEventHandlerFactory>();
            }

            return handlers;
        }
    }
}
