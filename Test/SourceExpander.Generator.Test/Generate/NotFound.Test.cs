using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
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
                            @"/home/mine/Program.cs",
                            @"using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
    }
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("EXPAND0003", DiagnosticSeverity.Info),
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", @"using System.Collections.Generic;
namespace SourceExpander.Expanded{
public static class ExpandedContainer{
public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}
private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{
{""/home/mine/Program.cs"",SourceCode.FromDictionary(new Dictionary<string,object>{{""path"",""/home/mine/Program.cs""},{""code"",""using System;\r\nclass Program\r\n{\r\n    static void Main()\r\n    {\r\n        Console.WriteLine(42);\r\n    }\r\n}\r\n#region Expanded by https://github.com/naminodarie/SourceExpander\r\n#endregion Expanded by https://github.com/naminodarie/SourceExpander\r\n""},})},
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
