using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate
{
    public class ComplicatedDependenciesTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace TestProject{public struct Bar{public StaticModIntFenwickTree<Mod998244353>fenwickTree;public static Foo2 GetFoo2()=>new Foo2();}}\",\"Dependencies\":[\"AtCoderLibrary>DataStructure/Wrappers/StaticModIntFenwickTree.cs\",\"AtCoderLibrary>Math/StaticModInt.cs\",\"TestProject>Foo2.cs\"],\"FileName\":\"TestProject>Bar.cs\",\"TypeNames\":[\"TestProject.Bar\"],\"Usings\":[\"using AtCoder;\"]},{\"CodeBody\":\"namespace TestProject{public static class Foo1{public static McfGraphInt GetMcfGraph()=>new McfGraphInt(10);public static Foo2 GetFoo2()=>new Foo2();}}\",\"Dependencies\":[\"AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs\",\"TestProject>Foo2.cs\"],\"FileName\":\"TestProject>Foo1.cs\",\"TypeNames\":[\"TestProject.Foo1\"],\"Usings\":[\"using AtCoder;\"]},{\"CodeBody\":\"namespace TestProject{public class Foo2{public McfGraphInt Graph=>new Foo3().Graph;}}\",\"Dependencies\":[\"AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs\",\"TestProject>Foo3.cs\"],\"FileName\":\"TestProject>Foo2.cs\",\"TypeNames\":[\"TestProject.Foo2\"],\"Usings\":[\"using AtCoder;\"]},{\"CodeBody\":\"namespace TestProject{using static Foo1;public class Foo3{public McfGraphInt Graph=>GetMcfGraph();}}\",\"Dependencies\":[\"AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs\",\"TestProject>Foo1.cs\"],\"FileName\":\"TestProject>Foo3.cs\",\"TypeNames\":[\"TestProject.Foo3\"],\"Usings\":[\"using AtCoder;\"]}]";
            var embeddedNamespaces = ImmutableArray.Create("TestProject");
            var embeddedFiles = ImmutableArray.Create([
                new SourceFileInfo
                (
                    "TestProject>Bar.cs",
                    ["TestProject.Bar"],
                    ["using AtCoder;"],
                    ["AtCoderLibrary>DataStructure/Wrappers/StaticModIntFenwickTree.cs", "AtCoderLibrary>Math/StaticModInt.cs", "TestProject>Foo2.cs"],
                    "namespace TestProject{public struct Bar{public StaticModIntFenwickTree<Mod998244353>fenwickTree;public static Foo2 GetFoo2()=>new Foo2();}}"
                ),
                new SourceFileInfo
                (
                    "TestProject>Foo1.cs",
                    ["TestProject.Foo1"],
                    ["using AtCoder;"],
                    ["AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs", "TestProject>Foo2.cs"],
                    "namespace TestProject{public static class Foo1{public static McfGraphInt GetMcfGraph()=>new McfGraphInt(10);public static Foo2 GetFoo2()=>new Foo2();}}"
                ),
                new SourceFileInfo
                (
                    "TestProject>Foo2.cs",
                    ["TestProject.Foo2"],
                    ["using AtCoder;"],
                    ["AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs", "TestProject>Foo3.cs"],
                    "namespace TestProject{public class Foo2{public McfGraphInt Graph=>new Foo3().Graph;}}"
                ),
                new SourceFileInfo
                (
                    "TestProject>Foo3.cs",
                    ["TestProject.Foo3"],
                    ["using AtCoder;"],
                    ["AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs", "TestProject>Foo1.cs"],
                    "namespace TestProject{using static Foo1;public class Foo3{public McfGraphInt Graph=>GetMcfGraph();}}"
                ),
            ]);

            var test = new Test
            {
                ReferenceAssemblies = ReferenceAssemblies.Net.Net50
                    .AddPackages(Packages.Add(new PackageIdentity("ac-library-csharp", "1.4.4"))),
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            "/home/mine/Foo1.cs",
                            """
using AtCoder;

namespace TestProject
{
    public static class Foo1
    {
        public static McfGraphInt GetMcfGraph() => new McfGraphInt(10);
        public static Foo2 GetFoo2() => new Foo2();
    }
}
"""
                        ),
                        (
                            "/home/mine/Foo2.cs",
                            """
using AtCoder;

namespace TestProject
{
    public class Foo2
    {
        public McfGraphInt Graph => new Foo3().Graph;
    }
}
"""
                        ),
                        (
                            "/home/mine/Foo3.cs",
                            """
using AtCoder;

namespace TestProject
{
    using static Foo1;
    public class Foo3
    {
        public McfGraphInt Graph => GetMcfGraph();
    }
}
"""
                        ),
                        (
                            "/home/mine/Bar.cs",
                            """
using AtCoder;

namespace TestProject
{
    public struct Bar
    {
        public StaticModIntFenwickTree<Mod998244353> fenwickTree;
        public static Foo2 GetFoo2() => new Foo2();
    }
}
"""
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        // <auto-generated/>
                        #pragma warning disable
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .ShouldBeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .ShouldBeEquivalentTo(embeddedFiles);
        }
    }
}
