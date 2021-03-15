﻿using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class OtherDependencyTest : EmbeddingGeneratorTestBase
    {
        static Solution CreateOtherReference(Solution solution,
            ProjectId projectId,
            SourceFileCollection documents,
            CSharpCompilationOptions compilationOptions = null)
        {
            if (compilationOptions is null)
                compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);

            var targetProject = solution.GetProject(projectId);

            var project = solution.AddProject("Other", "Other", "C#")
                .WithMetadataReferences(targetProject.MetadataReferences)
                .WithCompilationOptions(compilationOptions);
            foreach (var (filename, content) in documents)
            {
                project = project.AddDocument(Path.GetFileNameWithoutExtension(filename), content, filePath: filename).Project;
            }

            return project.Solution.AddProjectReference(projectId, new(project.Id));
        }

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
            const string embeddedSourceCode = "㘅桠ҠҠҠ傶䗂⣂㹆䟗鬜熬㖀ⲣ輡毳䍠韀ꐪ⨺ᘭ嫕㱫浈葍禤䣇緦炼燻ꜜ楧蝫㑭企墓ᣒ⢪烥崴ᓝꖞ毶粎䭠咼窪㝓㘪赕䛫嫟⩅瀇㤃毥䃲㤹䖨訕誁箦彔㕉貸㟔驪㟠炔䌲ᓲ哑⢑㜼椹䄑舺┴獤兪ꂍ鏧綿祯䠢靣韈䦭䷁朇꜡塑㢴崒ꋲ包䅨鵹嚠ꀿ䧏速征従䆗缞麿禯柏襳鞫ꀌ虄霍噖㬋ᆈ鉢雰䠷ꌮ甈䤿扠■ƃ";

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
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
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
            const string embeddedSourceCode = "㘅桠ҠҠҠ傶䗂⣂㹆䟗鬜熬㖀ⲣ輡毳䍠韀ꐪ⨺ᘭ嫕㱫浈葍禤䣇緦炼燻ꜜ楧蝫㑭企墓ᣒ⢪烥崴ᓝꖞ毶粎䭠咼窪㝓㘪赕䛫嫟⩅瀇㤃毥䃲㤹䖨訕誁箦彔㕉貸㟔驪㟠炔䌲ᓲ哑⢑㜼椹䄑舺┴獤兪ꂍ鏧綿祯䠢靣韈䦭䷁朇꜡塑㢴崒ꋲ包䅨鵹嚠ꀿ䧏速征従䆗缞麿禯柏襳鞫ꀌ虄霍噖㬋ᆈ鉢雰䠷ꌮ甈䤿扠■ƃ";

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
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
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
            const string embeddedSourceCode = "㘅桠ҠҠҠ俕䎶⣂㹊旅呴韄硕ᆈ㒆璊Ꮥ樉仉楁㢌疠嵪睚鸳┇䠓淓ꊿ曈䓼丈蝄檪佰蔦㜦懧跈沑歹㿔⣂䥟㑻ꆗ䦘吪攙礥㑼犃藰艅泄㠏茳⠝ꀀ䞑蟭ᓏ瞈柢药烮ᒱ揓謬荎瀐畞泵㇀毙噂骊寗䆡ꗡᄙ恳缷臈㥯虤㼈们蕠ҠƟ";

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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
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
            const string embeddedSourceCode = "㘅桠ҠҠҠ俕䎶⣂㹊旅呴韄硕ᆈ㒆璊Ꮥ樉仉楁㢌疠嵪睚鸳┇䠓淓ꊿ曈䓼丈蝄檪佰蔦㜦懧跈沑歹㿔⣂䥟㑻ꆗ䦘吪攙礥㑼犃藰艅泄㠏茳⠝ꀀ䞑蟭ᓏ瞈柢药烮ᒱ揓謬荎瀐畞泵㇀毙噂骊寗䆡ꗡᄙ恳缷臈㥯虤㼈们蕠ҠƟ";

            var others = new SourceFileCollection{
                (
                "home/other/C.cs",
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""{-}"")]"
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
                        DiagnosticResult.CompilerWarning("EMBED0006").WithArguments("SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. The token '\"' was expected but found '-'.")
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
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
            const string embeddedSourceCode = "㘅桠ҠҠҠ俕䎶⣂㹊旅呴韄硕ᆈ㒆璊Ꮥ樉仉楁㢌疠嵪睚鸳┇䠓淓ꊿ曈䓼丈蝄檪佰蔦㜦懧跈沑歹㿔⣂䥟㑻ꆗ䦘吪攙礥㑼犃藰艅泄㠏茳⠝ꀀ䞑蟭ᓏ瞈柢药烮ᒱ揓謬荎瀐畞泵㇀毙噂骊寗䆡ꗡᄙ恳缷臈㥯虤㼈们蕠ҠƟ";

            var others = new SourceFileCollection{
                (
                "home/other/C.cs",
                @"namespace Other{public static class C{public static void P() => System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode.GZipBase32768"", ""㘅桠ҠҠҠ俕䎶⣂㹊"")]"
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
                        DiagnosticResult.CompilerWarning("EMBED0006").WithArguments("SourceExpander.EmbeddedSourceCode.GZipBase32768", "Expecting element 'root' from namespace ''.. Encountered 'None'  with name '', namespace ''."),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}
