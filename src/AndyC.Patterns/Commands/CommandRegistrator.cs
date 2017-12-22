using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace AndyC.Patterns.Commands
{
    public class CommandRegistrator
    {
        private readonly Action<CommandHandlerRegistration> registerCommand;
        private readonly DependencyContext dependencyContext;

        public CommandRegistrator(Action<CommandHandlerRegistration> registerCommand, DependencyContext dependencyContext = null)
        {
            this.registerCommand = registerCommand;
            this.dependencyContext = dependencyContext ?? DependencyContext.Default;
        }

        public void RegisterCommands()
        {
            // Bind all ICommand implementations to the generic ICommand<,> interface they implement.
            var registrations =
                GetDefaultAssembliesToSearch(this.dependencyContext).SelectMany(FindRegistrations);

            Console.WriteLine("Command Registrations: {0}", registrations.Count());
            foreach (var registration in registrations)
            {
                registerCommand(registration);
            }
        }

        private static IEnumerable<CommandHandlerRegistration> FindRegistrations(Assembly assembly)
        {
            return 
                from type in assembly.GetExportedTypes()
                where type.GetInterfaces().Any(IsCommandHandler) && !type.GetTypeInfo().IsAbstract
                let commandInterface = type.GetInterfaces().Single(IsCommandHandler)
                let registrationInterface = commandInterface.GetTypeInfo().ContainsGenericParameters ? commandInterface.GetGenericTypeDefinition() : commandInterface
                select new CommandHandlerRegistration
                {
                    Interface = registrationInterface,
                    Implementation = type,
                    IsDecorator = type.GetTypeInfo().GetCustomAttribute<CommandDecorator>() != null
                };
        }

        private static bool IsCommandHandler(Type interfaceType)
        {
            return typeof(ICommandHandler).IsAssignableFrom(interfaceType)
                && typeof(ICommandHandler) != interfaceType;
        }

        private IList<Assembly> GetDefaultAssembliesToSearch(DependencyContext dependencyContext)
        {
            return dependencyContext
                .CompileLibraries
                .GetLoadableAssemblies()
                .ToList();
        }

        /// <summary>
        /// HACK to workaround the fact that the dependency graph may include some assemblies
        /// that are never actually loaded or deployed. GetExportedTypes throws for assembiles
        /// that are not deployed.
        /// </summary>
        private static bool CanLoad(Assembly assembly)
        {
            if (assembly == null || assembly.IsDynamic)
            {
                return false;
            }

            try
            {
                assembly.GetExportedTypes();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public class CommandHandlerRegistration
        {
            public Type Interface { get; set; }
            public Type Implementation { get; set; }
            public bool IsDecorator { get; set; }
        }
    }
}
