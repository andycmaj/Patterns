using System;
using Patterns.Tests.ExampleDependencies;
using Patterns.Commands;
using System.Threading.Tasks;

namespace Patterns.Tests.ExampleCommands {

    public class AsyncResetPasswordAction : IAction {

        public int UserId { get; set; }

        public class Handler : IActionHandlerAsync<AsyncResetPasswordAction> {

            private readonly IUserService userService;

            public Handler(IUserService userService) {
                this.userService = userService;
            }

            public async Task ExecuteAsync(AsyncResetPasswordAction action) {
                var user = userService.GetUser(action.UserId);

                if (user == null) {
                    throw new Exception("user not found");
                }

                userService.ResetPassword(user);
                await Task.CompletedTask;
            }

        }

    }

}