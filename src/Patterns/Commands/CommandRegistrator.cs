using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Patterns.Commands
{
    public class CommandRegistrator
    {
        private readonly Action<CommandHandlerRegistration> registerCommand;

        public CommandRegistrator(Action<CommandHandlerRegistration> registerCommand)
        {
            this.registerCommand = registerCommand;
        }

        public void RegisterCommands()
        {
            // Bind all ICommand implementations to the generic ICommand<,> interface they implement.
            var registrations =
                GetDefaultAssembliesToSearch().SelectMany(FindRegistrations);

            foreach (var registration in registrations)
            {
                registerCommand(registration);
            }
        }

        private static IEnumerable<CommandHandlerRegistration> FindRegistrations(Assembly assembly)
        {
            return assembly
                .GetExportedTypes()
                .Where(type => type.GetInterfaces().Any(IsCommandHandler))
                .Where(type => !type.GetTypeInfo().IsAbstract)
                .Select(type => new CommandHandlerRegistration
                {
                    Interface = type.GetInterfaces().Single(IsCommandHandler),
                    Implementation = type,
                    IsDecorator = type.GetTypeInfo().GetCustomAttribute<CommandDecorator>() != null,
                });
        }

        private static bool IsCommandHandler(Type interfaceType)
        {
            return typeof(ICommandHandler).IsAssignableFrom(interfaceType)
                && typeof(ICommandHandler) != interfaceType;
        }

        private static IList<Assembly> GetDefaultAssembliesToSearch()
        {
            return DependencyContext.Default
                .CompileLibraries
                .Select(lib =>
                {
                    if (File.Exists(lib.Path))
                    {
                        // dotnet core platform assemblies are not actually deployed with app
                        return null;
                    }

                    try
                    {
                        return Assembly.Load(new AssemblyName(lib.Name));
                    }
                    catch (FileNotFoundException)
                    {
                        return null;
                    }
                })
                .Where(CanLoad)
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
