using AndyC.Patterns.Tests.ExampleDependencies;
using AndyC.Patterns.Commands;
using FakeItEasy;

namespace AndyC.Patterns.Tests.Commands
{
    public abstract class AbstractCommandIntegrationTest
    {
        protected IUserService MockUserService { get; }
        protected ICommandRouter CommandRouter { get; }

        protected AbstractCommandIntegrationTest()
        {
            MockUserService = A.Fake<IUserService>();

            var handlerFactory = new TestCommandHandlerFactory(MockUserService);

            CommandRouter = new CommandRouter(handlerFactory);
        }
    }
}