using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class OlderVersionTest : ExpandGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
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
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedLanguageVersion"",""7.2"")]",
                    @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""2147483647.2147483647.2147483647.2147483647"")]")
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
                        DiagnosticResult.CompilerWarning("EXPAND0002").WithArguments(ExpanderVersion, "Other", "2147483647.2147483647.2147483647.2147483647"),
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                         "using System.Reflection;",
                         $"[assembly: AssemblyMetadataAttribute(\"SourceExpander.ExpanderVersion\",\"{ExpanderVersion}\")]"
                         )),
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
    }
}
