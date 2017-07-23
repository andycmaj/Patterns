using AndyC.Patterns.Events;

namespace Commands.Tests.Events
{
    public class SimpleEvent : Event
    {
        public int Value { get; set; }

        public SimpleEvent(object source, int value)
        {
            EventSource = source;
            Value = value;
        }

        public SimpleEvent(int value)
        {
            Value = value;
        }
    }
}
