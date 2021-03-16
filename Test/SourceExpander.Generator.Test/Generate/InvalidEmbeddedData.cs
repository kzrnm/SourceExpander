﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generator.Generate.Test
{
    public class InvalidEmbeddedData : ExpandGeneratorTestBase
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
                @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedSourceCode"", ""[-]"")]"
                + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbedderVersion"",""1.1.1.1"")]"
                + @"[assembly: System.Reflection.AssemblyMetadata(""SourceExpander.EmbeddedLanguageVersion"",""7.2"")]"
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
                        (
                            @"/home/mine/Program2.cs",
                            @"using System;
using Other;

class Program2
{
    static void M()
    {
        C.P();
    }
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        new DiagnosticResult("EXPAND0003", DiagnosticSeverity.Info),
                        DiagnosticResult.CompilerWarning("EXPAND0008").WithArguments("Other", "SourceExpander.EmbeddedSourceCode", "Expecting state 'Element'.. Encountered 'Text'  with name '', namespace ''."),
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", @"using System.Collections.Generic;
namespace SourceExpander.Expanded{
public static class ExpandedContainer{
public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}
private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{
{""/home/mine/Program.cs"",SourceCode.FromDictionary(new Dictionary<string,object>{{""path"",""/home/mine/Program.cs""},{""code"",""using Other;\r\nusing System;\r\nclass Program\r\n{\r\n    static void Main()\r\n    {\r\n        Console.WriteLine(42);\r\n        C.P();\r\n    }\r\n}\r\n#region Expanded by https://github.com/naminodarie/SourceExpander\r\n#endregion Expanded by https://github.com/naminodarie/SourceExpander\r\n""},})},
{""/home/mine/Program2.cs"",SourceCode.FromDictionary(new Dictionary<string,object>{{""path"",""/home/mine/Program2.cs""},{""code"",""using Other;\r\nclass Program2\r\n{\r\n    static void M()\r\n    {\r\n        C.P();\r\n    }\r\n}\r\n#region Expanded by https://github.com/naminodarie/SourceExpander\r\n#endregion Expanded by https://github.com/naminodarie/SourceExpander\r\n""},})},
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