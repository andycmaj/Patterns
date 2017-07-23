using SimpleInjector;
using SimpleInjector.Extensions.LifetimeScoping;

namespace AndyC.Patterns.SimpleInjector
{
    public class ConsoleApplicationContext : AbstractConsoleApplicationContext
    {
        public ConsoleApplicationContext(Lifestyle defaultLifestyle, params IModule[] modules)
            : base(defaultLifestyle, new LifetimeScopeLifestyle(), modules)
        {
        }

        public ConsoleApplicationContext(params IModule[] modules)
            : this(Lifestyle.Transient, modules)
        {
        }

        protected override Scope BeginScope()
        {
            return Container.BeginLifetimeScope();
        }
    }
}
