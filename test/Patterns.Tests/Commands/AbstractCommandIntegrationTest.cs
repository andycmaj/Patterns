using Moq;
using Patterns.Tests.ExampleDependencies;
using Ninject;
using Patterns.Commands;

namespace Patterns.Tests
{

    public abstract class AbstractCommandIntegrationTest {

        protected Mock<IUserService> MockUserService;
        protected ICommandRouter CommandRouter;

        public AbstractCommandIntegrationTest() {
            MockUserService = new Mock<IUserService>(MockBehavior.Strict);

            var kernel =
                new StandardKernel(
                    new ExampleDependencyGraph(MockUserService.Object)
                );

            CommandRouter = kernel.Get<ICommandRouter>();
        }

    }

}
