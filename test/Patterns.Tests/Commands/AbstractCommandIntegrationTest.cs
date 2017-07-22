using Patterns.Tests.ExampleDependencies;
using Patterns.Commands;
using FakeItEasy;

namespace Patterns.Tests.Commands
{
    public abstract class AbstractCommandIntegrationTest
    {
        protected IUserService MockUserService;
        protected ICommandRouter CommandRouter;

        public AbstractCommandIntegrationTest()
        {
            MockUserService = A.Fake<IUserService>();

            var handlerFactory = new TestCommandHandlerFactory(MockUserService);

            CommandRouter = new CommandRouter(handlerFactory);
        }
    }
}