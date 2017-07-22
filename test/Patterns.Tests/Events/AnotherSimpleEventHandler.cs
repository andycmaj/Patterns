using System;
using Patterns.Events;

namespace Commands.Tests.Events
{
    public class AnotherSimpleEventHandler : IEventHandler<SimpleEvent>, IDisposable
    {
        public static int HandleCount { get; set; }

        public static int DisposeCount { get; set; }

        public static void ResetCounts()
        {
            HandleCount = 0;
            DisposeCount = 0;
        }

        public void HandleEvent(SimpleEvent eventData)
        {
            ++HandleCount;
        }

        public void Dispose()
        {
            ++DisposeCount;
        }
    }
}
