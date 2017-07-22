using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyModel;

namespace Patterns
{
    public static class CompileLibrariesExtensions
    {
        public static IEnumerable<Assembly> GetLoadableAssemblies(
            this IEnumerable<CompilationLibrary> compilationLibraries
        )
        {
            return compilationLibraries
                .Where(lib => lib.Assemblies.Any())
                .Select(lib => lib.Assemblies.First())
                .Where(assemblyName => !assemblyName.Contains("/"))
                .Select(assemblyName => {
                    try {
                        var nameToLoad = assemblyName
                            .Replace(".dll", string.Empty);
                        var assembly = Assembly.Load(new AssemblyName(nameToLoad));
                        assembly.GetExportedTypes();
                        return assembly;
                    }
                    catch
                    {
                        return null;
                    }
                })
                .Where(assembly => assembly != null);
        }
    }
}