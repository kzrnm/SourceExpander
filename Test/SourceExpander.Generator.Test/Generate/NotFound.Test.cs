using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class NotFoundTest : ExpandGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            var test = new Test
            {
                TestState =
                {
                    Sources = {
                        (
                            "/home/mine/Program.cs",
                            """
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
    }
}
"""
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("EXPAND0003", DiagnosticSeverity.Info),
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Metadata.cs",
                        $$"""
                         using System.Reflection;
                         [assembly: AssemblyMetadataAttribute("SourceExpander.ExpanderVersion","{{ExpanderVersion}}")]
                         
                         """),
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", $$$"""
using System.Collections.Generic;
namespace SourceExpander.Expanded{
public static class ExpandedContainer{
public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}
private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{
{"/home/mine/Program.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program.cs"},{"code",{{{"""
using System;
class Program
{
    static void Main()
    {
        Console.WriteLine(42);
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
};
}}
""".ReplaceEOL())
                    }
                }
            };
            await test.RunAsync();
        }
    }
}
