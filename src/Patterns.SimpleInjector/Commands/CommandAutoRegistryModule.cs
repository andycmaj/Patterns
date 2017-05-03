using SimpleInjector;
using Patterns.Commands;

namespace Patterns.SimpleInjector.Commands
{
    public class CommandAutoRegistryModule : IModule
    {
        private readonly Lifestyle commandLifestyle;

        public CommandAutoRegistryModule() : this(Lifestyle.Transient) { }

        public CommandAutoRegistryModule(Lifestyle commandLifestyle)
        {
            this.commandLifestyle = commandLifestyle;
        }
        
        public void RegisterServices(Container container)
        {
            void Register(CommandRegistrator.CommandHandlerRegistration registration)
            {
                if (registration.IsDecorator)
                {
                    container.RegisterDecorator(registration.Interface, registration.Implementation, commandLifestyle);
                }
                else
                {
                    container.Register(registration.Interface, registration.Implementation, commandLifestyle);
                }
            }

            var registrator = new CommandRegistrator(Register);

            registrator.RegisterCommands();

            container.RegisterSingleton<
                ICommandHandlerFactory,
                SimpleInjectorCommandHandlerFactory
            >();

            container.RegisterSingleton<ICommandRouter, CommandRouter>();
        }
    }
}
