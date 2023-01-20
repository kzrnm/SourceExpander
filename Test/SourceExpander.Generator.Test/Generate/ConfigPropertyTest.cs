using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Generate
{
    public class ConfigPropertyTest : ExpandGeneratorTestBase
    {
        [Fact]
        public async Task NotEnabled()
        {
            var others = new SourceFileCollection{
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
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Generator_Enabled", "false" },
            };

            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
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
using Other;

class Program2
{
    static void M()
    {
        C.P();
    }
}
"""
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                    },
                }
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task IgnoreFile()
        {
            var others = new SourceFileCollection{
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


            // language=regex
            var pattern = @"mine/Program\d+\.cs$";
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Generator_IgnoreFilePatternRegex", pattern },
            };

            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
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
                            "/home/mine/X/Program2.cs",
                            """
using System;
using Other;

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
                            "/home/mine/Program1.cs",
                            """
using System;
using Other;

class Program1
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
using System;
using Other;

class Program3
{
    static void M()
    {
        C.P();
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
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/X/Program2.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/X/Program2.cs"},{"code",{{{"""
using Other;
class Program2
{
    static void M()
    {
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
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
        public async Task Whitelist()
        {
            var others = new SourceFileCollection{
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
            var pattern = "mine/Program.cs";
            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Generator_MatchFilePattern", pattern },
            };

            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
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
using Other;

class Program2
{
    static void M()
    {
        C.P();
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
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
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
        public async Task StaticEmbeddingText()
        {
            var others = new SourceFileCollection{
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

            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Generator_StaticEmbeddingText", "/* Static Embedding Text */" },
            };
            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
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
using Other;

class Program2
{
    static void M()
    {
        C.P();
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
/* Static Embedding Text */
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/Program2.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program2.cs"},{"code",{{{"""
using Other;
class Program2
{
    static void M()
    {
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
/* Static Embedding Text */
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
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
        public async Task MetadataExpandingFileNotFound()
        {
            var others = new SourceFileCollection{
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

            var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Generator_StaticEmbeddingText", "/* Static Embedding Text */" },
                { "build_property.SourceExpander_Generator_MetadataExpandingFile", "Program0.cs" },
            };
            var test = new Test
            {
                AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
                SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
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
using Other;

class Program2
{
    static void M()
    {
        C.P();
    }
}
"""
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerWarning("EXPAND0009").WithArguments("Program0.cs"),
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
/* Static Embedding Text */
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/Program2.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program2.cs"},{"code",{{{"""
using Other;
class Program2
{
    static void M()
    {
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
/* Static Embedding Text */
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
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
public async Task MetadataExpandingFile()
{
    var others = new SourceFileCollection{
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

    var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Generator_StaticEmbeddingText", "/* Static Embedding Text */" },
                { "build_property.SourceExpander_Generator_MetadataExpandingFile", "Program.cs" },
            };
    var test = new Test
    {
        AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
        SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others),
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
using Other;

class Program2
{
    static void M()
    {
        C.P();
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
                        (typeof(ExpandGenerator), "SourceExpander.Metadata.cs", $$$"""
using System.Reflection;
[assembly: AssemblyMetadataAttribute("SourceExpander.Expanded.Default",{{{"""
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
/* Static Embedding Text */
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}})]
[assembly: AssemblyMetadataAttribute("SourceExpander.ExpanderVersion","{{{ExpanderVersion}}}")]
""".ReplaceEOL()),
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
/* Static Embedding Text */
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/Program2.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program2.cs"},{"code",{{{"""
using Other;
class Program2
{
    static void M()
    {
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
/* Static Embedding Text */
namespace Other { public static class C { public static void P() => System.Console.WriteLine(); } } 
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
public async Task IgnoreAssemblies()
{
    var others = new SourceFileCollection{
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

    var analyzerConfigOptionsProvider = new DummyAnalyzerConfigOptionsProvider
            {
                { "build_property.SourceExpander_Generator_StaticEmbeddingText", "/* Static Embedding Text */" },
                { "build_property.SourceExpander_Generator_IgnoreAssemblies", "OtherAssembly;ExAssembly" },
            };
    var test = new Test
    {
        AnalyzerConfigOptionsProvider = analyzerConfigOptionsProvider,
        SolutionTransforms =
                {
                    (solution, projectId)
                    => CreateOtherReference(solution, projectId, others, otherAssemblyName:"OtherAssembly"),
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
using Other;

class Program2
{
    static void M()
    {
        C.P();
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
                        (typeof(ExpandGenerator), "SourceExpander.Metadata.cs", ($$$"""
using System.Reflection;
[assembly: AssemblyMetadataAttribute("SourceExpander.ExpanderVersion","{{{ExpanderVersion}}}")]
""").ReplaceEOL()),
                        (typeof(ExpandGenerator), "SourceExpander.Expanded.cs", ($$$"""
using System.Collections.Generic;
namespace SourceExpander.Expanded{
public static class ExpandedContainer{
public static IReadOnlyDictionary<string, SourceCode> Files {get{ return _Files; }}
private static Dictionary<string, SourceCode> _Files = new Dictionary<string, SourceCode>{
{"/home/mine/Program.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program.cs"},{"code",{{{
"""
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
/* Static Embedding Text */
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
{"/home/mine/Program2.cs",SourceCode.FromDictionary(new Dictionary<string,object>{{"path","/home/mine/Program2.cs"},{"code",{{{
"""
using Other;
class Program2
{
    static void M()
    {
        C.P();
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
/* Static Embedding Text */
#endregion Expanded by https://github.com/kzrnm/SourceExpander
""".ReplaceEOL().ToLiteral()}}}},})},
};
}}
""").ReplaceEOL())
                    }
                }
    };
    await test.RunAsync();
}
    }
}
