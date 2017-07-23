using System;

namespace AndyC.Patterns.Events
{
    public interface IEventBus
    {
        /// <summary>
        /// Registers to an event.
        /// Specified action is called for all event occurrences.
        /// </summary>
        /// <param name="action">Action to handle events</param>
        /// <typeparam name="TEvent">Event type</typeparam>
        IDisposable Register<TEvent>(Action<TEvent> action)
            where TEvent : IEvent;

        /// <summary>
        /// Registers to an event. 
        /// Specified instance of the handler is used for all event occurrences.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="handler">Object to handle the event</param>
        IDisposable Register<TEvent>(IEventHandler<TEvent> handler)
            where TEvent : IEvent;

        /// <summary>
        /// Registers to an event.
        /// A new instance of <see cref="THandler"/> object is created for every event occurrence.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <typeparam name="THandler">Type of the event handler</typeparam>
        IDisposable Register<TEvent, THandler>()
            where TEvent : IEvent
            where THandler : IEventHandler<TEvent>, new();

        /// <summary>
        /// Registers to an event.
        /// Specified instance of the handler is used for all event occurrences.
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="handler">Object to handle the event</param>
        IDisposable Register(Type eventType, IEventHandler handler);

        /// <summary>
        /// Registers to an event.
        /// Specified factory is used to create/release handlers
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="handlerFactory">A factory to create/release handlers</param>
        IDisposable Register<TEvent>(IEventHandlerFactory handlerFactory) 
            where TEvent : IEvent;

        /// <summary>
        /// Registers to an event.
        /// Specified factory is used to create/release handlers
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="handlerFactory">A factory to create/release handlers</param>
        IDisposable Register(Type eventType, IEventHandlerFactory handlerFactory);

        #region Unregister

        /// <summary>
        /// Unregisters from an event.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="action"></param>
        void Unregister<TEvent>(Action<TEvent> action) where TEvent : IEvent;

        /// <summary>
        /// Unregisters from an event.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="handler">Handler object that is registered before</param>
        void Unregister<TEvent>(IEventHandler<TEvent> handler) where TEvent : IEvent;

        /// <summary>
        /// Unregisters from an event.
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="handler">Handler object that is registered before</param>
        void Unregister(Type eventType, IEventHandler handler);

        /// <summary>
        /// Unregisters from an event.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="factory">Factory object that is registered before</param>
        void Unregister<TEvent>(IEventHandlerFactory factory) where TEvent : IEvent;

        /// <summary>
        /// Unregisters from an event.
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="factory">Factory object that is registered before</param>
        void Unregister(Type eventType, IEventHandlerFactory factory);

        /// <summary>
        /// Unregisters all event handlers of given event type.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        void UnregisterAll<TEvent>() where TEvent : IEvent;

        /// <summary>
        /// Unregisters all event handlers of given event type.
        /// </summary>
        /// <param name="eventType">Event type</param>
        void UnregisterAll(Type eventType);

        #endregion

        /// <summary>
        /// Triggers an event.
        /// </summary>
        /// <typeparam name="TEvent">Event type</typeparam>
        /// <param name="eventData">Related data for the event</param>
        void Trigger<TEvent>(TEvent e)
            where TEvent : IEvent;
    }
}
