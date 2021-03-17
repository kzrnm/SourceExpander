using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class ComplicatedDependenciesTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            var embeddedFiles = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestProject>Foo1.cs",
                    new string[] { "TestProject.Foo1" },
                    new string[] { "using AtCoder;" },
                    new string[] { "AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs", "TestProject>Foo2.cs" },
                    "namespace TestProject{public static class Foo1{public static McfGraphInt GetMcfGraph()=>new McfGraphInt(10);public static Foo2 GetFoo2()=>new Foo2();}}"
                ),
                new SourceFileInfo
                (
                    "TestProject>Foo2.cs",
                    new string[] { "TestProject.Foo2" },
                    new string[] { "using AtCoder;" },
                    new string[] { "AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs", "TestProject>Foo3.cs" },
                    "namespace TestProject{public class Foo2{public McfGraphInt Graph=>new Foo3().Graph;}}"
                ),
                new SourceFileInfo
                (
                    "TestProject>Foo3.cs",
                    new string[] { "TestProject.Foo3" },
                    new string[] { "using AtCoder;" },
                    new string[] { "AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs", "TestProject>Foo1.cs" },
                    "namespace TestProject{using static Foo1;public class Foo3{public McfGraphInt Graph=>GetMcfGraph();}}"
                ),
                new SourceFileInfo
                (
                    "TestProject>Bar.cs",
                    new string[] { "TestProject.Bar" },
                    new string[] { "using AtCoder;" },
                    new string[] { "AtCoderLibrary>DataStructure/Wrappers/StaticModIntFenwickTree.cs", "AtCoderLibrary>Math/StaticModInt.cs", "TestProject>Foo2.cs" },
                    "namespace TestProject{public struct Bar{public StaticModIntFenwickTree<Mod998244353>fenwickTree;public static Foo2 GetFoo2()=>new Foo2();}}"
                )
            );
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace TestProject{public struct Bar{public StaticModIntFenwickTree<Mod998244353>fenwickTree;public static Foo2 GetFoo2()=>new Foo2();}}\",\"Dependencies\":[\"AtCoderLibrary>DataStructure/Wrappers/StaticModIntFenwickTree.cs\",\"AtCoderLibrary>Math/StaticModInt.cs\",\"TestProject>Foo2.cs\"],\"FileName\":\"TestProject>Bar.cs\",\"TypeNames\":[\"TestProject.Bar\"],\"Usings\":[\"using AtCoder;\"]},{\"CodeBody\":\"namespace TestProject{public static class Foo1{public static McfGraphInt GetMcfGraph()=>new McfGraphInt(10);public static Foo2 GetFoo2()=>new Foo2();}}\",\"Dependencies\":[\"AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs\",\"TestProject>Foo2.cs\"],\"FileName\":\"TestProject>Foo1.cs\",\"TypeNames\":[\"TestProject.Foo1\"],\"Usings\":[\"using AtCoder;\"]},{\"CodeBody\":\"namespace TestProject{public class Foo2{public McfGraphInt Graph=>new Foo3().Graph;}}\",\"Dependencies\":[\"AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs\",\"TestProject>Foo3.cs\"],\"FileName\":\"TestProject>Foo2.cs\",\"TypeNames\":[\"TestProject.Foo2\"],\"Usings\":[\"using AtCoder;\"]},{\"CodeBody\":\"namespace TestProject{using static Foo1;public class Foo3{public McfGraphInt Graph=>GetMcfGraph();}}\",\"Dependencies\":[\"AtCoderLibrary>Graph/Wrappers/MinCostFlowWrapper.cs\",\"TestProject>Foo1.cs\"],\"FileName\":\"TestProject>Foo3.cs\",\"TypeNames\":[\"TestProject.Foo3\"],\"Usings\":[\"using AtCoder;\"]}]";

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
                            @"/home/mine/Foo1.cs",
                            @"
using AtCoder;

namespace TestProject
{
    public static class Foo1
    {
        public static McfGraphInt GetMcfGraph() => new McfGraphInt(10);
        public static Foo2 GetFoo2() => new Foo2();
    }
}"
                        ),
                        (
                            @"/home/mine/Foo2.cs",
                            @"
using AtCoder;

namespace TestProject
{
    public class Foo2
    {
        public McfGraphInt Graph => new Foo3().Graph;
    }
}"
                        ),
                        (
                            @"/home/mine/Foo3.cs",
                            @"
using AtCoder;

namespace TestProject
{
    using static Foo1;
    public class Foo3
    {
        public McfGraphInt Graph => GetMcfGraph();
    }
}
"
                        ),
                        (
                            @"/home/mine/Bar.cs",
                            @"
using AtCoder;

namespace TestProject
{
    public struct Bar
    {
        public StaticModIntFenwickTree<Mod998244353> fenwickTree;
        public static Foo2 GetFoo2() => new Foo2();
    }
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}
