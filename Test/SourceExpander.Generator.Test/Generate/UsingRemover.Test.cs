using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class UsingRemoverTest : ExpandGeneratorTestBase
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
                @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[{\""CodeBody\"":\""namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \"",\""Dependencies\"":[],\""FileName\"":\""OtherDependency>C.cs\"",\""TypeNames\"":[\""Other.C\""],\""Usings\"":[]}]"")]"
                + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""2147483647.2147483647.2147483647.2147483647"")]"
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
using static Other.C;
using System.Collections;
using static System.StringSplitOptions;
using Li = System.Collections.Generic.List<int>;

namespace Name 
{
    using System.Collections.Generic;
    using static System.Base64FormattingOptions;
    using E = System.Linq.Enumerable;
    class Program
    {
        static void Main()
        {
            Console.WriteLine(42);
            C.P();
        }
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
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", @"using System.Collections.Generic;
namespace SourceExpander.Expanded{
public static class ExpandedContainer{
public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}
private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{
{""/home/mine/Program.cs"",SourceCode.FromDictionary(new Dictionary<string,object>{{""path"",""/home/mine/Program.cs""},{""code"",""using Other;\r\nusing System;\r\nnamespace Name \r\n{\r\n    class Program\r\n    {\r\n        static void Main()\r\n        {\r\n            Console.WriteLine(42);\r\n            C.P();\r\n        }\r\n    }\r\n}\r\n#region Expanded by https://github.com/naminodarie/SourceExpander\r\nnamespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \r\n#endregion Expanded by https://github.com/naminodarie/SourceExpander\r\n""},})},
};
}}
".ReplaceEOL())
                    }
                }
            };
            await test.RunAsync();
        }
    }
}
