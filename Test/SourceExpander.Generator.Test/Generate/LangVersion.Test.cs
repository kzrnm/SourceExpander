using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class LangVersionTest : ExpandGeneratorTestBase
    {
        readonly SourceFileCollection others = new()
        {
            (
                @"/home/other/C.cs",
                "namespace Other{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ),
            (
                @"/home/other/AssemblyInfo.cs",
                EnvironmentUtil.JoinByStringBuilder(
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedNamespaces"", ""Other"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedLanguageVersion"",""7.2"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""1.1.1.1"")]")
                ),
        };

        [Theory]
        [InlineData(LanguageVersion.CSharp7_2)]
        [InlineData(LanguageVersion.CSharp8)]
        [InlineData(LanguageVersion.CSharp9)]
        [InlineData(LanguageVersion.CSharp10)]
        public async Task Success(LanguageVersion version)
        {
            var test = new Test
            {
                ParseOptions = new(version),
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
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", (@"using System.Collections.Generic;
namespace SourceExpander.Expanded{
public static class ExpandedContainer{
public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}
private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{
{""/home/mine/Program.cs"",SourceCode.FromDictionary(new Dictionary<string,object>{{""path"",""/home/mine/Program.cs""},{""code"","
+ @"using Other;
using System;
class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Expanded by https://github.com/kzrnm/SourceExpander".ReplaceEOL().ToLiteral()
+ @"},})},
};
}}").ReplaceEOL())
                    }
                }
            };
            await test.RunAsync();
        }

        [Theory]
        [InlineData(LanguageVersion.CSharp4)]
        [InlineData(LanguageVersion.CSharp5)]
        [InlineData(LanguageVersion.CSharp6)]
        [InlineData(LanguageVersion.CSharp7)]
        [InlineData(LanguageVersion.CSharp7_1)]
        public async Task Failure(LanguageVersion version)
        {
            var test = new Test
            {
                ParseOptions = new(version),
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
                           DiagnosticResult.CompilerWarning("EXPAND0005").WithArguments(version.ToDisplayString(), "Other", "7.2"),
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", (@"using System.Collections.Generic;
namespace SourceExpander.Expanded{
public static class ExpandedContainer{
public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}
private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{
{""/home/mine/Program.cs"",SourceCode.FromDictionary(new Dictionary<string,object>{{""path"",""/home/mine/Program.cs""},{""code"","
+ @"using Other;
using System;
class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Expanded by https://github.com/kzrnm/SourceExpander".ReplaceEOL().ToLiteral()
+ @"},})},
};
}}").ReplaceEOL())
                    }
                }
            };
            await test.RunAsync();
        }

        [Theory]
        [InlineData(LanguageVersion.CSharp1)]
        [InlineData(LanguageVersion.CSharp2)]
        [InlineData(LanguageVersion.CSharp3)]
        public async Task FailureWithCSharp3OrOlder(LanguageVersion version)
        {
            var test = new Test
            {
                ParseOptions = new(version),
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
                        new DiagnosticResult("EXPAND0004", DiagnosticSeverity.Info),
                    }
}
            };
            await test.RunAsync();
        }
    }
}
