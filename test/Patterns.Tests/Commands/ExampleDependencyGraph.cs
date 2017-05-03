using IPatternsCommands;
using Patterns.Ninject.Commands;
using Patterns.Tests.ExampleCommands;
using Patterns.Tests.ExampleDependencies;
using Ninject.Modules;

namespace Patterns.Tests {

    public class ExampleDependencyGraph : NinjectModule {

        private readonly IUserService userService;

        public ExampleDependencyGraph(IUserService userService) {
            this.userService = userService;
        }

        public override void Load() {
            Bind<IUserService>()
                .ToConstant(userService);

            Bind<IActionHandler<ResetPasswordAction>>()
                .To<ResetPasswordAction.Handler>();

            Bind<IFunctionHandler<AuthenticateUserFunction, AuthenticateUserFunction.Result>>()
                .To<AuthenticateUserFunction.Handler>();

            Bind<IActionHandlerAsync<AsyncResetPasswordAction>>()
                .To<AsyncResetPasswordAction.Handler>();

            Bind<IFunctionHandlerAsync<AsyncAuthenticateUserFunction, AsyncAuthenticateUserFunction.Result>>()
                .To<AsyncAuthenticateUserFunction.Handler>();

            Bind<ICommandHandlerFactory>()
                .To<NinjectCommandHandlerFactory>()
                .InSingletonScope();

            Bind<ICommandRouter>()
                .To<CommandRouter>()
                .InSingletonScope();
        }

    }

}