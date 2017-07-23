using Xunit;
using System;
using AndyC.Patterns.Events;

namespace Commands.Tests.Events
{
    public class ActionEventHandlerTest
    {
        private readonly IEventBus eventBus = new EventBus();

        [Fact]
        public void Should_Call_Action_On_Event_With_Correct_Source()
        {
            var totalData = 0;

            eventBus.Register<SimpleEvent>(
                eventData =>
                {
                    totalData += eventData.Value;
                    Assert.Equal(this, eventData.EventSource);
                });

            eventBus.Trigger(new SimpleEvent(this, 1));
            eventBus.Trigger(new SimpleEvent(this, 2));
            eventBus.Trigger(new SimpleEvent(this, 3));
            eventBus.Trigger(new SimpleEvent(this, 4));

            Assert.Equal(10, totalData);
        }

        [Fact]
        public void Should_Not_Call_Action_After_Dispose()
        {
            var totalData = 0;

            var action =
                new Action<SimpleEvent>(eventData =>
                {
                    totalData += eventData.Value;
                });

            using (eventBus.Register(action))
            {
                eventBus.Trigger(new SimpleEvent(1));
                eventBus.Trigger(new SimpleEvent(2));
                eventBus.Trigger(new SimpleEvent(3));
            }

            eventBus.Trigger(new SimpleEvent(4));

            Assert.Equal(6, totalData);
        }

        [Fact]
        public void Should_Not_Call_Action_After_Unregister()
        {
            var totalData = 0;

            var action =
               new Action<SimpleEvent>(eventData =>
               {
                   totalData += eventData.Value;
               });

            eventBus.Register(action);

            eventBus.Trigger(new SimpleEvent(1));
            eventBus.Trigger(new SimpleEvent(2));
            eventBus.Trigger(new SimpleEvent(3));

            eventBus.Unregister(action);

            eventBus.Trigger(new SimpleEvent(4));

            Assert.Equal(6, totalData);
        }

    }
}
