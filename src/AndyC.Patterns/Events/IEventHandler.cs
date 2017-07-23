namespace AndyC.Patterns.Events
{
    public interface IEventHandler
    {
    }

    /// <summary>
    /// Defines an interface of a class that handles events of type <see cref="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">Event type to handle</typeparam>
    public interface IEventHandler<in TEvent> : IEventHandler
    {
        /// <summary>
        /// Handler handles the event by implementing this method.
        /// </summary>
        /// <param name="event">Event</param>
        void HandleEvent(TEvent @event);
    }
}
