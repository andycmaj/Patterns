using System;
using SimpleInjector;
using System.Collections.Generic;
using SimpleInjector.Extensions.LifetimeScoping;

namespace Patterns.SimpleInjector
{
    public sealed class ConsoleApplicationContext : AbstractDispoable
    {
        private IDisposable scope;
        private bool disposed;

        public Container Container { get; private set; }

        public ConsoleApplicationContext(params IModule[] modules)
            : this(Lifestyle.Transient, modules)
        {
        }

        public ConsoleApplicationContext(Lifestyle defaultLifestyle, params IModule[] modules)
            : this(defaultLifestyle, new LifetimeScopeLifestyle(), modules)
        {
        }

        public ConsoleApplicationContext(
            Lifestyle defaultLifestyle, 
            ScopedLifestyle scopedLifeStyle, 
            params IModule[] modules
        )
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
            scope = Container.BeginLifetimeScope();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
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

                disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
