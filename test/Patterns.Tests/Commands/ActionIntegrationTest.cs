using FakeItEasy;
using Patterns.Tests.ExampleCommands;
using Patterns.Tests.ExampleDependencies;
using Xunit;

namespace Patterns.Tests.Commands
{
    public class ActionIntegrationTest : AbstractCommandIntegrationTest
    {
        private const int UserId = 1;

        [Fact]
        public void Can_Inject_ActionHandler_Dependencies()
        {
            var user = new User
            {
                Id = UserId
            };

            A.CallTo(() => MockUserService.GetUser(UserId))
                .Returns(user);

            var action = new ResetPasswordAction
            {
                UserId = UserId
            };

            CommandRouter.ExecuteAction(action);

            A.CallTo(() => MockUserService.ResetPassword(user))
                .MustHaveHappened();
        }

        [Fact]
        public async void Can_Inject_ActionHandlerAsync_Dependencies()
        {
            var user = new User
            {
                Id = UserId
            };


            A.CallTo(() => MockUserService.GetUser(UserId))
                .Returns(user);

            var action = new AsyncResetPasswordAction
            {
                UserId = UserId
            };

            await CommandRouter.ExecuteActionAsync(action);

            A.CallTo(() => MockUserService.ResetPassword(user))
                .MustHaveHappened();
        }
    }

}
