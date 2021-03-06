using System;
using System.Collections.Generic;
using AndyC.Patterns.Commands;
using AndyC.Patterns.Tests.ExampleCommands;
using AndyC.Patterns.Tests.ExampleDependencies;

namespace AndyC.Patterns.Tests.Commands
{
    public class TestCommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly IDictionary<Type, Func<ICommandHandler>> handlerFactories;

        public TestCommandHandlerFactory(IUserService userService)
        {
            handlerFactories = new Dictionary<Type, Func<ICommandHandler>> {
                { typeof(IActionHandler<ResetPasswordAction>), () => new ResetPasswordAction.Handler(userService) },
                { typeof(IActionHandlerAsync<AsyncResetPasswordAction>), () => new AsyncResetPasswordAction.Handler(userService) },
                { typeof(IFunctionHandler<AuthenticateUserFunction, AuthenticateUserFunction.Result>), () => new AuthenticateUserFunction.Handler(userService) },
                { typeof(IFunctionHandlerAsync<AsyncAuthenticateUserFunction, AsyncAuthenticateUserFunction.Result>), () => new AsyncAuthenticateUserFunction.Handler(userService) },
            };
        }

        public ICommandHandler Create(Type handlerType)
        {
            Console.WriteLine($"Trying to create handler type: {handlerType.FullName}");
            return handlerFactories[handlerType]();
        }

        public void Release(ICommandHandler handler)
        {
        }
    }
}