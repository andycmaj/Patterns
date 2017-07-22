using System.Collections.Generic;
using Xunit;
using Patterns.Events;
using FakeItEasy;

namespace Commands.Tests.Events
{
    public class EventBusTest
    {
        private readonly IEventBus eventBus = new EventBus();

        public EventBusTest()
        {
            SimpleEventHandler.ResetCounts();
            AnotherSimpleEventHandler.ResetCounts();
        }

        [Fact]
        public void Register_TransientEventHandler_Should_Call_Handler_And_Dispose()
        {
            eventBus.Register<SimpleEvent, SimpleEventHandler>();

            eventBus.Trigger(new SimpleEvent(1));
            eventBus.Trigger(new SimpleEvent(2));
            eventBus.Trigger(new SimpleEvent(3));

            Assert.Equal(3, SimpleEventHandler.HandleCount);
            Assert.Equal(3, SimpleEventHandler.DisposeCount);
        }

        [Fact]
        public void Register_EventHandlerFactory_Should_Call_All_Handlers_Returned_By_Factory()
        {
            var multipleHandlers =
                new List<IEventHandler<SimpleEvent>>()
                {
                    new SimpleEventHandler(),
                    new AnotherSimpleEventHandler()
                };

            var eventHandlerFactory = A.Fake<IEventHandlerFactory>();
            A.CallTo(() => eventHandlerFactory.GetHandlers<SimpleEvent>())
                .Returns(multipleHandlers);

            eventBus.Register<SimpleEvent>(eventHandlerFactory);

            eventBus.Trigger(new SimpleEvent(1));
            eventBus.Trigger(new SimpleEvent(2));
            eventBus.Trigger(new SimpleEvent(3));

            Assert.Equal(3, SimpleEventHandler.HandleCount);

            Assert.Equal(3, AnotherSimpleEventHandler.HandleCount);
        }

        [Fact]
        public void Register_Multiple_EventHandlerFactories_Should_Call_Handlers_From_All_Factories()
        {
            // two different internal TransientEventHandlerFactory's
            eventBus.Register<SimpleEvent, SimpleEventHandler>();
            eventBus.Register<SimpleEvent, AnotherSimpleEventHandler>();

            eventBus.Trigger(new SimpleEvent(1));

            Assert.Equal(1, SimpleEventHandler.HandleCount);
            Assert.Equal(1, SimpleEventHandler.DisposeCount);

            Assert.Equal(1, AnotherSimpleEventHandler.HandleCount);
            Assert.Equal(1, AnotherSimpleEventHandler.DisposeCount);
        }

        [Fact(Skip = "Event Inheritance not yet implemented")]
        public void Can_Handle_Subclasses_Of_Registered_Events()
        {
            eventBus.Register<SimpleEventSubclass, SimpleEventHandler>();

            eventBus.Trigger(new SimpleEvent(1));
            eventBus.Trigger(new SimpleEvent(2));
            eventBus.Trigger(new SimpleEvent(3));

            Assert.Equal(3, SimpleEventHandler.HandleCount);
        }
    }
}
