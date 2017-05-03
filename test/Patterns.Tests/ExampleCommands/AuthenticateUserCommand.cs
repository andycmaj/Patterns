using Patterns.Tests.ExampleDependencies;
using Patterns.Commands;

namespace Patterns.Tests.ExampleCommands
{

    public class AuthenticateUserFunction
        : IFunction<AuthenticateUserFunction.Result>
    {

        public int UserId { get; set; }
        public string Password { get; set; }

        public class Result
        {

            public bool IsAuthenticated { get; set; }

        }

        public class Handler
            : IFunctionHandler
                <AuthenticateUserFunction, AuthenticateUserFunction.Result>
        {

            private readonly IUserService userService;

            public Handler(IUserService userService)
            {
                this.userService = userService;
            }

            public Result Execute(AuthenticateUserFunction function)
            {
                var isAuthenticated = userService.VerifyPassword(
                    function.UserId,
                    function.Password);

                return new Result
                {
                    IsAuthenticated = isAuthenticated
                };
            }

        }

    }

}