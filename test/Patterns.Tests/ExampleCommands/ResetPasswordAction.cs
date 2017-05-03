using System;
using Patterns.Tests.ExampleDependencies;
using Patterns.Commands;

namespace Patterns.Tests.ExampleCommands {

    public class ResetPasswordAction : IAction {

        public int UserId { get; set; }

        public class Handler : IActionHandler<ResetPasswordAction> {

            private readonly IUserService userService;

            public Handler(IUserService userService) {
                this.userService = userService;
            }

            public void Execute(ResetPasswordAction action) {
                var user = userService.GetUser(action.UserId);

                if (user == null) {
                    throw new Exception("user not found");
                }

                userService.ResetPassword(user);
            }

        }

    }

}