﻿using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate.Config
{
    public class ExpandingByGroupTest : ExpandGeneratorTestBase
    {
        [Fact]
        public async Task ExpandingByGroup()
        {
            var others1 = new SourceFileCollection{
                (
                "/home/other/C.cs",
                "namespace Other{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ),
                (
                "/home/other/AssemblyInfo.cs",
                EnvironmentUtil.JoinByStringBuilder(
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedSourceCode", "[{\"CodeBody\":\"namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \",\"Dependencies\":[],\"FileName\":\"OtherDependency>C.cs\",\"TypeNames\":[\"Other.C\"],\"Usings\":[]}]")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedNamespaces", "Other")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedLanguageVersion","7.2")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbedderVersion","1.1.1.1")]""")
                ),
            };
            var others2 = new SourceFileCollection{
                (
                "/home/other2/C.cs",
                "namespace Other2{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ),
                (
                "/home/other/AssemblyInfo.cs",
                EnvironmentUtil.JoinByStringBuilder(
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedSourceCode", "[{\"CodeBody\":\"namespace Other2 { public static class C { public static void P() => System.Console.WriteLine(); } } \",\"Dependencies\":[],\"FileName\":\"OtherDependency2>C.cs\",\"TypeNames\":[\"Other2.C\"],\"Usings\":[]}]")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedNamespaces", "Other2")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedLanguageVersion","7.2")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbedderVersion","1.1.1.1")]""")
                ),
            };

            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Generator.Config.json", """
{
    "$schema": "https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/expander.schema.json",
    "expanding-by-group": true
}
""");
            var test = new Test
            {
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                        solution = CreateOtherReference(solution, projectId, others1, otherName:"Other1",otherAssemblyName:"OtherAssembly1");
                        return CreateOtherReference(solution, projectId, others2, otherName:"Other2",otherAssemblyName:"OtherAssembly2");
                    },
                },
                TestState =
                {
                    AdditionalFiles = { additionalText },
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
                        (
                            "/home/mine/Program2.cs",
                            """
using System;
using Other2;

class Program2
{
    static void M()
    {
        C.P();
    }
}
"""
                        ),
                        (
                            "/home/mine/Program3.cs",
                            """
class Program3
{
    static void M()
    {
        Other.C.P();
        Other2.C.P();
    }
}
"""
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                         "using System.Reflection;",
                         $$$"""[assembly: AssemblyMetadataAttribute("SourceExpander.ExpanderVersion","{{{ExpanderVersion}}}")]"""
                         )),
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", $$$"""
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
#region Assembly:OtherDependency
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Assembly:OtherDependency
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/Program2.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program2.cs"},{"code",{{{"""
using Other2;
class Program2
{
    static void M()
    {
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
#region Assembly:OtherDependency2
namespace Other2 { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Assembly:OtherDependency2
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/Program3.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program3.cs"},{"code",{{{"""
class Program3
{
    static void M()
    {
        Other.C.P();
        Other2.C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
#region Assembly:OtherDependency2
namespace Other2 { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Assembly:OtherDependency2
#region Assembly:OtherDependency
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Assembly:OtherDependency
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

        [Fact]
        public async Task ExpandingByGroupProperty()
        {
            var others1 = new SourceFileCollection{
                (
                "/home/other/C.cs",
                "namespace Other{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ),
                (
                "/home/other/AssemblyInfo.cs",
                EnvironmentUtil.JoinByStringBuilder(
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedSourceCode", "[{\"CodeBody\":\"namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } \",\"Dependencies\":[],\"FileName\":\"OtherDependency>C.cs\",\"TypeNames\":[\"Other.C\"],\"Usings\":[]}]")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedNamespaces", "Other")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedLanguageVersion","7.2")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbedderVersion","1.1.1.1")]""")
                ),
            };
            var others2 = new SourceFileCollection{
                (
                "/home/other2/C.cs",
                "namespace Other2{public static class C{public static void P()=>System.Console.WriteLine();}}"
                ),
                (
                "/home/other/AssemblyInfo.cs",
                EnvironmentUtil.JoinByStringBuilder(
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedSourceCode", "[{\"CodeBody\":\"namespace Other2 { public static class C { public static void P() => System.Console.WriteLine(); } } \",\"Dependencies\":[],\"FileName\":\"OtherDependency2>C.cs\",\"TypeNames\":[\"Other2.C\"],\"Usings\":[]}]")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedNamespaces", "Other2")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbeddedLanguageVersion","7.2")]""",
                    """[assembly: System.Reflection.AssemblyMetadata("SourceExpander.EmbedderVersion","1.1.1.1")]""")
                ),
            };

            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Generator_ExpandingByGroup", "true" },
            };
            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                SolutionTransforms =
                {
                    (solution, projectId) =>
                    {
                        solution = CreateOtherReference(solution, projectId, others1, otherName:"Other1",otherAssemblyName:"OtherAssembly1");
                        return CreateOtherReference(solution, projectId, others2, otherName:"Other2",otherAssemblyName:"OtherAssembly2");
                    },
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
                        (
                            "/home/mine/Program2.cs",
                            """
using System;
using Other2;

class Program2
{
    static void M()
    {
        C.P();
    }
}
"""
                        ),
                        (
                            "/home/mine/Program3.cs",
                            """
class Program3
{
    static void M()
    {
        Other.C.P();
        Other2.C.P();
    }
}
"""
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                    },
                    GeneratedSources =
                    {
                        (typeof(ExpandGenerator), "SourceExpander.Metadata.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                         "using System.Reflection;",
                         $$$"""[assembly: AssemblyMetadataAttribute("SourceExpander.ExpanderVersion","{{{ExpanderVersion}}}")]"""
                         )),
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", $$$"""
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
#region Assembly:OtherDependency
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Assembly:OtherDependency
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/Program2.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program2.cs"},{"code",{{{"""
using Other2;
class Program2
{
    static void M()
    {
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
#region Assembly:OtherDependency2
namespace Other2 { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Assembly:OtherDependency2
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/Program3.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program3.cs"},{"code",{{{"""
class Program3
{
    static void M()
    {
        Other.C.P();
        Other2.C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
#region Assembly:OtherDependency2
namespace Other2 { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Assembly:OtherDependency2
#region Assembly:OtherDependency
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Assembly:OtherDependency
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
