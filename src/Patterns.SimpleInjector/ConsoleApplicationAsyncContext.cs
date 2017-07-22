using SimpleInjector;
using SimpleInjector.Extensions.ExecutionContextScoping;

namespace Patterns.SimpleInjector
{
    /// <summary>
    /// Async/await should use Per Execution Context Scope
    /// http://simpleinjector.readthedocs.io/en/latest/lifetimes.html#per-execution-context-scope-async-await
    /// </summary>
    /// <example>
    /// Using async command
    /// <code>
    /// public class CommandModule : IModule
    /// {
    ///     public void RegisterServices(Container container)
    ///     {
    ///         new CommandAutoRegistryPackage().RegisterServices(container);
    ///     }
    /// }
    /// public class SimpleCommand : IFunction<SimpleCommand.Result>
    /// {
    ///     public class Result { }
    ///     public class Handler : IFunctionHandlerAsync<SimpleCommand, Result>
    ///     {
    ///         public async Task<Result> ExecuteAsync(SimpleCommand command) { throw new NotImplementedException(); }
    ///     }
    /// }
    /// class Program
    /// {
    ///     static void Main(string[] args)
    ///     {
    ///         MainAsync(args).Wait();
    ///     }
    ///     private static async Task MainAsync(string[] args)
    ///     {
    ///         using (var context = new ConsoleApplicationAsyncContext(new CommandModule()))
    ///         {
    ///             var command = new SimpleCommand();
    ///             var router = context.Container.GetInstance<ICommandRouter>();
    ///             await router.ExecuteFunctionAsync(new SimpleCommand());
    ///         }
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ConsoleApplicationAsyncContext : AbstractConsoleApplicationContext
    {
        public ConsoleApplicationAsyncContext(Lifestyle defaultLifestyle, params IModule[] modules)
            : base(defaultLifestyle, new ExecutionContextScopeLifestyle(), modules)
        {
        }

        public ConsoleApplicationAsyncContext(params IModule[] modules)
            : this(Lifestyle.Transient, modules)
        {
        }

        protected override Scope BeginScope()
        {
            return Container.BeginExecutionContextScope();
        }
    }
}
