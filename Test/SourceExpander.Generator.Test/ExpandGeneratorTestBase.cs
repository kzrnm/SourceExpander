using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander;

public class ExpandGeneratorTestBase : GeneratorTestBase<ExpandGenerator>
{
    public class Test : TestBase
    {
        public Test()
        {
            AnalyzerConfigOptions.Add(
                typeof(ExpandConfig.Builder).GetProperties()
                    .Where(p => p.Name is not "SourceText")
                    .Select(p => KeyValuePair.Create($"build_property.SourceExpander_Embedder_{p.Name}", "")));
            ReferenceAssemblies = ReferenceAssemblies.AddPackages(DefaultPackages);
        }
    }

    private static readonly ImmutableArray<PackageIdentity> DefaultPackages = [new PackageIdentity("SourceExpander.Core", "2.6.0")];
    protected internal override ImmutableArray<PackageIdentity> Packages => DefaultPackages;
    public static string ExpanderVersion => typeof(ExpandGenerator).Assembly.GetName().Version.ToString();
}
