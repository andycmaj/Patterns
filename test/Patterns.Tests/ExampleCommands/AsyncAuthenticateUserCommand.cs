using Patterns.Tests.ExampleDependencies;
using Patterns.Commands;
using System.Threading.Tasks;
using System;

namespace Patterns.Tests.ExampleCommands
{

    public class AsyncAuthenticateUserFunction
        : IFunction<AsyncAuthenticateUserFunction.Result>
    {

        public int UserId { get; set; }
        public string Password { get; set; }

        public class Result
        {

            public bool IsAuthenticated { get; set; }

        }

        public class Handler
            : IFunctionHandlerAsync
                <AsyncAuthenticateUserFunction, AsyncAuthenticateUserFunction.Result>
        {

            private readonly IUserService userService;

            public Handler(IUserService userService)
            {
                this.userService = userService;
            }

            public async Task<AsyncAuthenticateUserFunction.Result> ExecuteAsync(AsyncAuthenticateUserFunction function)
            {
                var isAuthenticated = userService.VerifyPassword(
                        function.UserId,
                        function.Password);

                return await Task.FromResult(new Result
                {
                    IsAuthenticated = isAuthenticated
                });
            }
        }

    }

}
