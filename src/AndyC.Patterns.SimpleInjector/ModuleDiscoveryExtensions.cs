using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using SimpleInjector;

// This class is placed in the root namespace to allow users to start using these extension methods after
// adding the assembly reference, without find and add the correct namespace.
namespace AndyC.Patterns.SimpleInjector
{
    /// <summary>
    /// Extension methods for working with packages.
    /// </summary>
    public static class ModuleDiscoveryExtensions
    {
        public static IList<Type> FindTypes<T>(IEnumerable<Assembly> assemblies)
        {
            return (
                from assembly in assemblies
                from type in GetExportedTypesFrom(assembly)
                let typeInfo = type.GetTypeInfo()
                where typeof(T).IsAssignableFrom(type)
                where !typeInfo.IsAbstract
                where !typeInfo.IsGenericTypeDefinition
                select type
            ).ToList();
        }

        /// <summary>
        /// Loads all <see cref="IModule"/> implementations from the given set of
        /// <paramref name="assemblies"/> and, optionally, calls <paramref name="registerModule"/> to register them.
        /// Note that only publicly exposed classes that contain a public default constructor will be loaded.
        /// </summary>
        /// <param name="container">The container to which the packages will be applied to.</param>
        /// <param name="assemblies">The assemblies that will be searched for packages.</param>
        /// <param name="registerModule"></param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="container"/> is a null
        /// reference.</exception>
        public static IList<TModule> RegisterModules<TModule>(
            this Container container,
            IEnumerable<Assembly> assemblies,
            Action<Container, TModule> registerModule
        )
        {
            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            if (assemblies == null)
            {
                throw new ArgumentNullException(nameof(assemblies));
            }

            var moduleTypes = FindTypes<TModule>(assemblies);

            RequireDefaultConstructorForModules(moduleTypes);

            var modules = (
                from type in moduleTypes
                select CreateModule<TModule>(type)
            ).ToArray();

            foreach (var module in modules)
            {
                registerModule?.Invoke(container, module);
            }

            return modules.ToList();
        }

        private static IEnumerable<Type> GetExportedTypesFrom(Assembly assembly)
        {
            try
            {
                return assembly.GetExportedTypes();
            }
            catch (NotSupportedException)
            {
                // A type load exception would typically happen on an Anonymously Hosted DynamicMethods
                // Assembly and it would be safe to skip this exception.
                return Enumerable.Empty<Type>();
            }
        }

        private static void RequireDefaultConstructorForModules(IList<Type> packageTypes)
        {
            var invalidModuleType =
                packageTypes.FirstOrDefault(type => type.GetConstructor(Type.EmptyTypes) == null);

            if (invalidModuleType != null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture,
                    "The type {0} does not contain a default (public parameterless) constructor. Modules " +
                    "must have a default constructor.", invalidModuleType.FullName));
            }
        }

        private static TModule CreateModule<TModule>(Type packageType)
        {
            try
            {
                return (TModule)Activator.CreateInstance(packageType);
            }
            catch (Exception ex)
            {
                string message = string.Format(CultureInfo.InvariantCulture,
                    "The creation of package type {0} failed. {1}", packageType.FullName, ex.Message);

                throw new InvalidOperationException(message, ex);
            }
        }
    }
}
