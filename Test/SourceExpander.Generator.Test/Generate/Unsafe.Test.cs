﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class UnsafeTest : ExpandGeneratorTestBase
    {
        [Fact]
        public async Task Allow() => await Impl(true, []);
        [Fact]
        public async Task NotAllow() => await Impl(false, [DiagnosticResult.CompilerWarning("EXPAND0010").WithSpan("/home/mine/Program.cs", 1, 1, 1, 1)]);

        static async Task Impl(bool allowUnsafe, DiagnosticResult[] expectedDiagnostics)
        {
            var others = new SourceFileCollection{
                (
                    "/home/other/C.cs",
                    "namespace Other{public static class C{public static void P()=>U.P();}}"
                ),
                (
                    "/home/other/U.cs",
                    "namespace Other{public static class U{public static unsafe void P()=>System.Console.WriteLine();}}"
                ),
                (
                "/home/other/AssemblyInfo.cs",
                EnvironmentUtil.JoinByStringBuilder(
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedSourceCode", "[{\"CodeBody\":\"namespace Other { public static class C { public static void P() => U.P(); } }\",\"Dependencies\":[\"OtherDependency>U.cs\"],\"FileName\":\"OtherDependency>C.cs\",\"TypeNames\":[\"Other.C\"],\"Usings\":[]},{\"CodeBody\":\"namespace Other { public static class U { public static unsafe void P() => System.Console.WriteLine(); } }\",\"Dependencies\":[],\"FileName\":\"OtherDependency>U.cs\",\"TypeNames\":[\"Other.U\"],\"Usings\":[]}]")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedNamespaces", "Other")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedAllowUnsafe","true")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbedderVersion","1.1.1.1")]""")
                ),
            };
            var test = new Test
            {
                CompilationOptions = new(OutputKind.ConsoleApplication, allowUnsafe: allowUnsafe),
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others,
                    compilationOptions: new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true)),
                },
                TestState =
                {
                    Sources = {
                        (
                            "/home/mine/Program.cs",
                            """
using System;
using Other;

class Program
{
    static void Main()
    {
        Console.WriteLine(42);
        C.P();
    }
}
"""
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                         "using System.Reflection;",
                         $$$"""[assembly: AssemblyMetadataAttribute("SourceExpander.ExpanderVersion","{{{ExpanderVersion}}}")]"""
                         )),
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs",$$$"""
using System.Collections.Generic;
namespace SourceExpander.Expanded{
public static class ExpandedContainer{
public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}
private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{
{"/home/mine/Program.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program.cs"},{"code",{{{"""
using Other;
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
namespace Other { public static class C { public static void P() => U.P(); } }
namespace Other { public static class U { public static unsafe void P() => System.Console.WriteLine(); } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
};
}}
""".ReplaceEOL()),
                    }
                }
            };

            test.ExpectedDiagnostics.AddRange(expectedDiagnostics);

            await test.RunAsync();
        }
    }
}
