using System.Collections.Immutable;

namespace SourceExpander.Embedder;

public class PackJsonTest
{
    public record Target(string Name, bool IsPacked)
    {
        public override string ToString() => Name;
    }

    public static ImmutableArray<Target> Targets = [
        new(
            IsPacked: true,
            Name: "net10.0"),
        new(
            IsPacked: false,
            Name: "netstandard2.1"),
        new(
            IsPacked: false,
            Name: "netstandard2.0"),
    ];

    static readonly string PackingProjectResult = Path.Combine(Sample.Testing.PackingProjectDirectory, "bin");

    [Test]
    public async Task PackingProjectDirectories()
    {
        var dir = new DirectoryInfo(PackingProjectResult);
        await Assert.That(dir).Exists();
        await dir.EnumerateDirectories().Select(d => d.Name).Should().BeEquivalentTo(Targets.Select(t => t.Name));
    }

    [Test]
    [MethodDataSource(nameof(Targets))]
    public async Task Packed(Target target)
    {
        var dir = new DirectoryInfo(Path.Combine(PackingProjectResult, target.Name, "SourceExpander.build"));
        if (target.IsPacked)
        {
            await Assert.That(dir).Exists();
            using (Assert.Multiple())
            {
                await File.ReadAllText(Path.Combine(dir.FullName, "Sample.Packing.props"))
                    .Should().BeEqualTo("""
                    <Project>
                      <PropertyGroup>
                        <Sample_Packing_Source>$(MSBuildThisFileDirectory)Sample.Packing_SourceExpander.Embedded.json</Sample_Packing_Source>
                        <Sample_Packing_Source_Visible>false</Sample_Packing_Source_Visible>
                      </PropertyGroup>
                    </Project>

                    """);
                await File.ReadAllText(Path.Combine(dir.FullName, "Sample.Packing.targets"))
                    .Should().BeEqualTo("""
                    <Project>
                      <ItemGroup Condition="'$(SourceExpander_Generator)'=='true' And Exists('$(Sample_Packing_Source)')">
                        <AdditionalFiles LinkBase="Properties/SourceExpander.Embedded"
                          Include="$(Sample_Packing_Source)"
                          Visible="$(Sample_Packing_Source_Visible)" />
                      </ItemGroup>
                    </Project>

                    """);
                await File.ReadAllText(Path.Combine(dir.FullName, "Sample.Packing_SourceExpander.Embedded.json"))
                    .Should().BeEqualTo("""
                    {"AssemblyName":"Sample.Packing","Sources":[{"CodeBody":"namespace Sample { class P { public static int Num => 10; } }","Dependencies":[],"FileName":"Sample.Packing>Packing.cs","TypeNames":["Sample.P"],"Usings":[]}],"EmbedderVersion":"$$EmbedderVersion$$","CSharpVersion":"14.0","AllowUnsafe":false,"EmbeddedNamespaces":["Sample"]}
                    """.Replace("$$EmbedderVersion$$", typeof(EmbedderGenerator).Assembly.GetName().Version.ToString()));
            }
        }
        else
        {
            await Assert.That(dir).DoesNotExist();
        }
    }
}
