using System;
using SimpleInjector;
using System.Collections.Generic;

namespace AndyC.Patterns.SimpleInjector
{
    public abstract class AbstractConsoleApplicationContext : AbstractDispoable
    {
        private IDisposable scope;
        private bool _disposed;

        public Container Container { get; private set; }

        protected abstract Scope BeginScope();

        public AbstractConsoleApplicationContext(Lifestyle defaultLifestyle, ScopedLifestyle scopedLifeStyle, params IModule[] modules)
        {
            Container = new Container();

            Container.Options.DefaultLifestyle = defaultLifestyle;
            Container.Options.DefaultScopedLifestyle = scopedLifeStyle;

            foreach (var module in modules)
            {
                module.RegisterServices(Container);
            }

            if (Container.GetRegistration(typeof(IEnumerable<IBootstrapper>)) != null)
            {
                using (Container.BeginLifetimeScope())
                {
                    // Discover and run bootstrappers
                    foreach (var bootstrapper in Container.GetAllInstances<IBootstrapper>())
                    {
                        bootstrapper.Bootstrap();
                    }
                }
            }

            Container.Verify();
            scope = BeginScope();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // free other managed objects that implement
                    // IDisposable only
                    scope.Dispose();
                    Container.Dispose();
                }

                // release any unmanaged objects
                // set object references to null
                scope = null;
                Container = null;

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
