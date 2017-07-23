using SimpleInjector;
using AndyC.Patterns.Commands;
using Microsoft.Extensions.DependencyModel;
using System;

namespace AndyC.Patterns.SimpleInjector.Commands
{
    public class CommandAutoRegistryModule : IModule
    {
        private readonly Lifestyle commandLifestyle;
        private readonly DependencyContext dependencyContext;
        public CommandAutoRegistryModule() : this(Lifestyle.Transient) { }

        public CommandAutoRegistryModule(
            Lifestyle commandLifestyle,
            DependencyContext dependencyContext = null
        )
        {
            this.commandLifestyle = commandLifestyle;
            this.dependencyContext = dependencyContext ?? DependencyContext.Default;
        }

        public void RegisterServices(Container container)
        {
            Action<CommandRegistrator.CommandHandlerRegistration> register =
                registration =>
                {
                    if (registration.IsDecorator)
                    {
                        container.RegisterDecorator(
                            registration.Interface,
                            registration.Implementation,
                            commandLifestyle);
                    }
                    else
                    {
                        container.Register(
                            registration.Interface,
                            registration.Implementation,
                            commandLifestyle);
                    }
                };

            var registrator = new CommandRegistrator(register, this.dependencyContext);

            registrator.RegisterCommands();

            container.RegisterSingleton<
                ICommandHandlerFactory,
                SimpleInjectorCommandHandlerFactory
            >();

            container.RegisterSingleton<ICommandRouter, CommandRouter>();
        }
    }
}
