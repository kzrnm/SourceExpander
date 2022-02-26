using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class AllowUnsafeTest : ExpandGeneratorTestBase
    {
        [Fact]
        public async Task Allow()
        {
            var others = new SourceFileCollection{
                (
                @"/home/other/C.cs",
                "namespace Other{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                EnvironmentUtil.JoinByStringBuilder(
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedNamespaces"", ""Other"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedAllowUnsafe"",""true"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""1.1.1.1"")]")
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
                    Sources = {
                        (
                            @"/home/mine/Program.cs",
                            @"using System;
using Other;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        C.P();
    }
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("EXPAND0006").WithArguments("Other"),
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                        "using System.Collections.Generic;" ,
                        "namespace SourceExpander.Expanded{" ,
                        "public static class ExpandedContainer{" ,
                        "public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}" ,
                        "private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{" ,
                        "{\"/home/mine/Program.cs\",SourceCode.FromDictionary(new Dictionary<string,object>{{\"path\",\"/home/mine/Program.cs\"},{\"code\","
                        + EnvironmentUtil.JoinByStringBuilder(
                            "using Other;" ,
                            "using System;" ,
                            "class Program" ,
                            "{" ,
                            "    static void Main()" ,
                            "    {" ,
                            "        Console.WriteLine(42);" ,
                            "        C.P();" ,
                            "    }" ,
                            "}",
                            "#region Expanded by https://github.com/kzrnm/SourceExpander",
                            "namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } ",
                            "#endregion Expanded by https://github.com/kzrnm/SourceExpander").ToLiteral()
                        + "},})}," ,
                        "};" ,
                        "}}"))
                    }
                }
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NotAllow()
        {
            var others = new SourceFileCollection{
                (
                @"/home/other/C.cs",
                "namespace Other{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ),
                (
                @"/home/other/AssemblyInfo.cs",
                EnvironmentUtil.JoinByStringBuilder(
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedNamespaces"", ""Other"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedAllowUnsafe"",""false"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""1.1.1.1"")]")
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
                    Sources = {
                        (
                            @"/home/mine/Program.cs",
                            @"using System;
using Other;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        C.P();
    }
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                        "using System.Collections.Generic;" ,
                        "namespace SourceExpander.Expanded{" ,
                        "public static class ExpandedContainer{" ,
                        "public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}" ,
                        "private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{" ,
                        "{\"/home/mine/Program.cs\",SourceCode.FromDictionary(new Dictionary<string,object>{{\"path\",\"/home/mine/Program.cs\"},{\"code\","
                        + EnvironmentUtil.JoinByStringBuilder(
                            "using Other;" ,
                            "using System;" ,
                            "class Program" ,
                            "{" ,
                            "    static void Main()" ,
                            "    {" ,
                            "        Console.WriteLine(42);" ,
                            "        C.P();" ,
                            "    }" ,
                            "}",
                            "#region Expanded by https://github.com/kzrnm/SourceExpander",
                            "namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } ",
                            "#endregion Expanded by https://github.com/kzrnm/SourceExpander").ToLiteral()
                        + "},})}," ,
                        "};" ,
                        "}}"))
                    }
                }
            };
            await test.RunAsync();
        }
    }
}
