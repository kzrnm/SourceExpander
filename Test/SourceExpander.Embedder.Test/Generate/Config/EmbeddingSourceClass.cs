using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Generate.Config
{
    public class EmbeddingSourceClassTest : EmbedderGeneratorTestBase
    {
        [Fact]
        public async Task EmbeddingSourceClassNone()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-source-class"": {
        ""enabled"": false,
        ""if-directive"": ""SOURCEEXPANDER""
    },
    ""minify-level"": ""full""
}
");

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemorySourceText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Debug.Assert(true);
        Console.WriteLine(1);
    }
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task EmbeddingSourceClass()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-source-class"": {
        ""enabled"": true,
        ""class-name"": ""Container""
    },
    ""minify-level"": ""full""
}
");

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemorySourceText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Debug.Assert(true);
        Console.WriteLine(1);
    }
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                            "namespace SourceExpander.Embedded{",
                            "using System;",
                            "using System.Collections.Generic;",
                            "public class Container{",
                            "public class SourceFileInfo{",
                            "  public string FileName{get;set;}",
                            "  public string[] TypeNames{get;set;}",
                            "  public string[] Usings{get;set;}",
                            "  public string[] Dependencies{get;set;}",
                            "  public string CodeBody{get;set;}",
                            "}",
                            "  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{",
                            "    new SourceFileInfo{",
                            "      FileName = \"TestProject>Program.cs\",",
                            "      CodeBody = \"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",",
                            "      TypeNames = new string[]{",
                            "        \"Program\",",
                            "      },",
                            "      Usings = new string[]{",
                            "        \"using System;\",",
                            "        \"using System.Diagnostics;\",",
                            "      },",
                            "      Dependencies = new string[]{",
                            "      },",
                            "    },",
                            "  };",
                            "}",
                            "}")
                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task EmbeddingSourceClassDefault()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-source-class"": {
        ""enabled"": true
    },
    ""minify-level"": ""full""
}
");

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemorySourceText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Debug.Assert(true);
        Console.WriteLine(1);
    }
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                            "namespace SourceExpander.Embedded{",
                            "using System;",
                            "using System.Collections.Generic;",
                            "public class SourceFileInfoContainer{",
                            "public class SourceFileInfo{",
                            "  public string FileName{get;set;}",
                            "  public string[] TypeNames{get;set;}",
                            "  public string[] Usings{get;set;}",
                            "  public string[] Dependencies{get;set;}",
                            "  public string CodeBody{get;set;}",
                            "}",
                            "  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{",
                            "    new SourceFileInfo{",
                            "      FileName = \"TestProject>Program.cs\",",
                            "      CodeBody = \"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",",
                            "      TypeNames = new string[]{",
                            "        \"Program\",",
                            "      },",
                            "      Usings = new string[]{",
                            "        \"using System;\",",
                            "        \"using System.Diagnostics;\",",
                            "      },",
                            "      Dependencies = new string[]{",
                            "      },",
                            "    },",
                            "  };",
                            "}",
                            "}")
                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task EmbeddingSourceClassAlways()
        {
            var additionalText = new InMemorySourceText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/kzrnm/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""embedding-source-class"": {
        ""enabled"": true,
        ""class-name"": """"
    },
    ""minify-level"": ""full""
}
");

            var embeddedNamespaces = ImmutableArray<string>.Empty;
            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray<string>.Empty,
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemorySourceText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        Debug.Assert(true);
        Console.WriteLine(1);
    }
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs",
                        EnvironmentUtil.JoinByStringBuilder(
                            "namespace SourceExpander.Embedded{",
                            "using System;",
                            "using System.Collections.Generic;",
                            "public class SourceFileInfoContainer{",
                            "public class SourceFileInfo{",
                            "  public string FileName{get;set;}",
                            "  public string[] TypeNames{get;set;}",
                            "  public string[] Usings{get;set;}",
                            "  public string[] Dependencies{get;set;}",
                            "  public string CodeBody{get;set;}",
                            "}",
                            "  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{",
                            "    new SourceFileInfo{",
                            "      FileName = \"TestProject>Program.cs\",",
                            "      CodeBody = \"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}\",",
                            "      TypeNames = new string[]{",
                            "        \"Program\",",
                            "      },",
                            "      Usings = new string[]{",
                            "        \"using System;\",",
                            "        \"using System.Diagnostics;\",",
                            "      },",
                            "      Dependencies = new string[]{",
                            "      },",
                            "    },",
                            "  };",
                            "}",
                            "}")                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        using System.Reflection;
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}
