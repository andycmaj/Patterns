using Patterns.Tests.ExampleCommands;

using Xunit;

namespace Patterns.Tests {

    public class FunctionIntegrationTest : AbstractCommandIntegrationTest {

        private const int UserId = 1;
        private const string Password = "password";

        [Fact]
        public void Can_Inject_FunctionHandler_Dependencies() {
            MockUserService
                .Setup(svc => svc.VerifyPassword(UserId, Password))
                .Returns(true);

            var function = new AuthenticateUserFunction {
                UserId = UserId,
                Password = Password
            };

            var result = CommandRouter.ExecuteFunction(function);

            Assert.True(result.IsAuthenticated);

            MockUserService.VerifyAll();
        }

        [Fact]
        public async void Can_Inject_FunctionHandlerAsync_Dependencies()
        {
            MockUserService
                .Setup(svc => svc.VerifyPassword(UserId, Password))
                .Returns(true);

            var function = new AsyncAuthenticateUserFunction
            {
                UserId = UserId,
                Password = Password
            };

            var result = await CommandRouter.ExecuteFunctionAsync(function);

            Assert.True(result.IsAuthenticated);

            MockUserService.VerifyAll();
        }


    }

}