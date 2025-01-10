using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Microsoft.Build.Locator;

namespace SourceExpander;

public partial class CommandTests
{
    [ModuleInitializer]
    internal static void InitializeMSBuildLocator()
    {
        var instance = MSBuildLocator.RegisterDefaults();
        AssemblyLoadContext.Default.Resolving += (assemblyLoadContext, assemblyName) =>
        {
            var path = Path.Combine(instance.MSBuildPath, assemblyName.Name + ".dll");
            if (File.Exists(path))
            {
                return assemblyLoadContext.LoadFromAssemblyPath(path);
            }

            return null;
        };
    }
}
