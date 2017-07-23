using System;

namespace AndyC.Patterns.Events
{
    public class EventHandlingException : Exception
    {
        public EventHandlingException(
            object eventInstance,
            Exception innerException
        ) : base(
            $"Exception thrown while handling an event: " +
            $"{eventInstance.GetType().FullName}",
            innerException
        )
        {
        }

        public EventHandlingException(Exception innerException)
            : base("Exception throw while handling an event.", innerException)
        {
        }
    }
}
