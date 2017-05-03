using Patterns.Tests.ExampleCommands;
using Patterns.Tests.ExampleDependencies;

using Xunit;

namespace Patterns.Tests {

    public class ActionIntegrationTest : AbstractCommandIntegrationTest {

        private const int UserId = 1;

        [Fact]
        public void Can_Inject_ActionHandler_Dependencies() {
            var user = new User {
                Id = UserId
            };

            MockUserService
                .Setup(svc => svc.GetUser(UserId))
                .Returns(user);

            MockUserService
                .Setup(svc => svc.ResetPassword(user))
                .Verifiable();

            var action = new ResetPasswordAction {
                UserId = UserId
            };

            CommandRouter.ExecuteAction(action);

            MockUserService.VerifyAll();
        }

        [Fact]
        public async void Can_Inject_ActionHandlerAsync_Dependencies()
        {
            var user = new User
            {
                Id = UserId
            };

            MockUserService
                .Setup(svc => svc.GetUser(UserId))
                .Returns(user);

            MockUserService
                .Setup(svc => svc.ResetPassword(user))
                .Verifiable();

            var action = new AsyncResetPasswordAction
            {
                UserId = UserId
            };

            await CommandRouter.ExecuteActionAsync(action);

            MockUserService.VerifyAll();
        }
    }

}
