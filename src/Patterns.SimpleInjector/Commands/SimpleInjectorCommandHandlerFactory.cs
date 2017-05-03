using System;
using Patterns.Commands;
using SimpleInjector;

namespace Patterns.SimpleInjector.Commands
{
    public class SimpleInjectorCommandHandlerFactory : ICommandHandlerFactory
    {
        private readonly Container container;

        public SimpleInjectorCommandHandlerFactory(Container container)
        {
            this.container = container;
        }

        public ICommandHandler Create(Type handlerType)
        {
            return (ICommandHandler)container.GetInstance(handlerType);
        }

        public void Release(ICommandHandler handler)
        {
            // No-op for SimpleInjector
        }
    }
}
