using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class OtherDependencyTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task OtherRaw()
        {
            var embeddedFiles = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestProject>Program.cs",
                    new string[] { "Mine.Program" },
                    new string[] { "using OC = Other.C;" },
                    new string[] { "OtherDependency>C.cs", "TestProject>C.cs" },
                    "namespace Mine{public static class Program{public static void Main(){OC.P();C.P();}}}"
                ),
                new SourceFileInfo
                (
                    "TestProject>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}\",\"Dependencies\":[],\"FileName\":\"TestProject>C.cs\",\"TypeNames\":[\"Mine.C\"],\"Usings\":[]},{\"CodeBody\":\"namespace Mine{public static class Program{public static void Main(){OC.P();C.P();}}}\",\"Dependencies\":[\"OtherDependency>C.cs\",\"TestProject>C.cs\"],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Mine.Program\"],\"Usings\":[\"using OC = Other.C;\"]}]";

            var others = new SourceFileCollection{
                (
                "home/other/C.cs",
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]"
                ),
            };

            var test = new Test
            {
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
                },
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            @"/home/mine/C.cs",
                            @"
namespace Mine{
    public static class C
    {
        public static void P() => System.Console.WriteLine();
    }
}"
                        ),
                        (
                            @"/home/mine/Program.cs",
                            @"
using OC = Other.C;

namespace Mine{
    public static class Program
    {
        public static void Main()
        {
            OC.P();
            C.P();
        }
    }
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("EMBED0010", DiagnosticSeverity.Info).WithSpan("/home/mine/Program.cs", 2, 1, 2, 20),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
                        ),
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

        [Fact]
        public async Task OtherGZip()
        {
            var embeddedFiles = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestProject>Program.cs",
                    new string[] { "Mine.Program" },
                    new string[] { "using OC = Other.C;" },
                    new string[] { "OtherDependency>C.cs", "TestProject>C.cs" },
                    "namespace Mine{public static class Program{public static void Main(){OC.P();C.P();}}}"
                ),
                new SourceFileInfo
                (
                    "TestProject>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}\",\"Dependencies\":[],\"FileName\":\"TestProject>C.cs\",\"TypeNames\":[\"Mine.C\"],\"Usings\":[]},{\"CodeBody\":\"namespace Mine{public static class Program{public static void Main(){OC.P();C.P();}}}\",\"Dependencies\":[\"OtherDependency>C.cs\",\"TestProject>C.cs\"],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Mine.Program\"],\"Usings\":[\"using OC = Other.C;\"]}]";

            var others = new SourceFileCollection{
                (
                "home/other/C.cs",
                "namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                "[assembly: System.Reflection.AssemblyMetadata(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\"," +
                "\"㘅桠ҠҠҠ俶䏂⣂㹆䟗謜熬㔀Ⰳ茡毳窰廸揪㇚ᖭ引㱫焸萍瀾㡣暎㘟牟腱棋厝趼㙩闌䡉偩⎙癠㠂恓䦀砦哂叇㡙襏ꜙ㟰鲅ᯝ呡䰆濜㴞缻筷蝂島彀練䮌抸霣ݮ倉蟶㤥矖⢶觉癁荁趟㪺䡶碊赆瓁㥟圅鮀糏䑖䆷璾穗ᓞ䵫镹癠ҧ\")]"
                ),
            };

            var test = new Test
            {
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
                },
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            @"/home/mine/C.cs",
                            @"
namespace Mine{
    public static class C
    {
        public static void P() => System.Console.WriteLine();
    }
}"
                        ),
                        (
                            @"/home/mine/Program.cs",
                            @"
using OC = Other.C;

namespace Mine{
    public static class Program
    {
        public static void Main()
        {
            OC.P();
            C.P();
        }
    }
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("EMBED0010", DiagnosticSeverity.Info).WithSpan("/home/mine/Program.cs", 2, 1, 2, 20),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
                        ),
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

        [Fact]
        public async Task UsingOlderVersion()
        {
            var embeddedFiles = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestProject>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}\",\"Dependencies\":[],\"FileName\":\"TestProject>C.cs\",\"TypeNames\":[\"Mine.C\"],\"Usings\":[]}]";

            var others = new SourceFileCollection{
                (
                "home/other/C.cs",
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                "[assembly: System.Reflection.AssemblyMetadata(\"SourceExpander.EmbedderVersion\",\"2147483647.2147483647.2147483647.2147483647\")]"
                + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]"
                ),
            };

            var test = new Test
            {
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
                },
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            @"/home/mine/C.cs",
                            @"
namespace Mine{
    public static class C
    {
        public static void P() => System.Console.WriteLine();
    }
}"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("EMBED0002").WithArguments(EmbedderVersion, "Other", "2147483647.2147483647.2147483647.2147483647")
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
                        ),
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

        [Fact]
        public async Task InvalidRaw()
        {
            var embeddedFiles = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestProject>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}\",\"Dependencies\":[],\"FileName\":\"TestProject>C.cs\",\"TypeNames\":[\"Mine.C\"],\"Usings\":[]}]";

            var others = new SourceFileCollection{
                (
                "home/other/C.cs",
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[}"")]"
                ),
            };

            var test = new Test
            {
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
                },
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            @"/home/mine/C.cs",
                            @"
namespace Mine{
    public static class C
    {
        public static void P() => System.Console.WriteLine();
    }
}"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("EMBED0006")
                        .WithArguments("Other", "SourceExpander.EmbeddedSourceCode",
                        "Unexpected character encountered while parsing value: }. Path '', line 1, position 1.")
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
                        ),
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

        [Fact]
        public async Task InvalidGZipBase32768()
        {
            var embeddedFiles = ImmutableArray.Create(
                new SourceFileInfo
                (
                    "TestProject>C.cs",
                    new string[] { "Mine.C" },
                    Array.Empty<string>(),
                    Array.Empty<string>(),
                    "namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"namespace Mine{public static class C{public static void P()=>System.Console.WriteLine();}}\",\"Dependencies\":[],\"FileName\":\"TestProject>C.cs\",\"TypeNames\":[\"Mine.C\"],\"Usings\":[]}]";

            var others = new SourceFileCollection{
                (
                "home/other/C.cs",
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""㘅桠ҠҠҠ俕䎶⣂㹊"")]"
                ),
            };

            var test = new Test
            {
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
                },
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            @"/home/mine/C.cs",
                            @"
namespace Mine{
    public static class C
    {
        public static void P() => System.Console.WriteLine();
    }
}"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("EMBED0006")
                        .WithArguments("Other", "SourceExpander.EmbeddedSourceCode",
                        "Unexpected character encountered while parsing value: 㘅. Path '', line 0, position 0."),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder("using System.Reflection;",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",\"{EmbedderVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedLanguageVersion\",\"{EmbeddedLanguageVersion}\")]",
                        $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode\",{embeddedSourceCode.ToLiteral()})]")
                        ),
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
