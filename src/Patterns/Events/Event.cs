using System;

namespace Patterns.Events
{
    public class Event : IEvent
    {
        public object EventSource { get; set; }

        public DateTime EventTime { get; set; } = DateTime.UtcNow;
    }
}
