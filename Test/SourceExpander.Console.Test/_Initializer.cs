using System.Runtime.Loader;
using Microsoft.Build.Locator;

namespace SourceExpander;

public static class Initializer
{
    public const string CommandTests = "CommandTests";

    [Before(Assembly)]
    public static async Task Setup(CancellationToken cancellationToken)
    {
        InitializeMSBuildLocator();
    }

    static void InitializeMSBuildLocator()
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
