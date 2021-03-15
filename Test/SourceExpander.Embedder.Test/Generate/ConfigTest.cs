using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class ConfigTest : EmbeddingGeneratorTestBase
    {
        public static TheoryData ParseErrorJsons = new TheoryData<InMemoryAdditionalText>
        {
            {
                new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""exclude-attributes"": 1
}
")
            },
            {
                new InMemoryAdditionalText(
                "/foo/bar/sourceExpander.embedder.config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""exclude-attributes"": 1
}
")
            },
        };

        [Theory]
        [MemberData(nameof(ParseErrorJsons))]
        public async Task ParseErrorTest(InMemoryAdditionalText additionalText)
        {
            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

[DebuggerDisplay(""Name"")]
class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional(""TEST"")]
    static void T() => Console.WriteLine(2);
}
"
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerError("EMBED0003"),
                    }
                }
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task NotEnabled()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""enabled"": false,
    ""exclude-attributes"": [
        ""System.Diagnostics.DebuggerDisplayAttribute""
    ]
}
");

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;

class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }
}
"
                        ),
                    },
                }
            };
            await test.RunAsync();
        }

        [Fact]
        public async Task ExcludeAttributes()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""exclude-attributes"": [
        ""System.Diagnostics.DebuggerDisplayAttribute""
    ],
    ""enable-minify"": true
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));
            const string embeddedSourceCode = "㘅桠ҠҠҠ俶䏖⣂㹈㞟鬙焈件棵⠯撶㭡♫玢顱璲䢩㚎⢥ꄏ觃霜⎋槾旵乧匜蜲琯忨ᦜ區靏敉偽䊔㙎疏ᒬ煔殎责譽搥鼗⠸ሓ烁䎤褌谯ލ銩脇ⲱ⤐泎⪤ꁇ岝ڮ䭼皻㸓莪琒ꎑ䗜䦠顸堆ꛀ坉䇐國魮叹䑴ᦈ譐潅✶削络峢耒㽯闎鱄橁ꏠ墇禋疐ꜝ挋顰㶑湚ᘤ髀婠䙐ҧ";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

[DebuggerDisplay(""Name"")]
class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional(""TEST"")]
    static void T() => Console.WriteLine(2);
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task EmbeddingRaw()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-type"": ""Raw"",
    ""enable-minify"": true
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"[DebuggerDisplay(""Name"")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);}"
                 ));
            const string embeddedSourceCode = "[{\"CodeBody\":\"[DebuggerDisplay(\\\"Name\\\")]class Program{static void Main(){Console.WriteLine(1);}[System.Diagnostics.Conditional(\\\"TEST\\\")]static void T()=>Console.WriteLine(2);}\",\"Dependencies\":[],\"FileName\":\"TestProject>Program.cs\",\"TypeNames\":[\"Program\"],\"Usings\":[\"using System;\",\"using System.Diagnostics;\"]}]";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
                    },
                    Sources = {
                        (
                            "/home/source/Program.cs",
                            @"
using System;
using System.Diagnostics;

[DebuggerDisplay(""Name"")]
class Program
{
    static void Main()
    {
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional(""TEST"")]
    static void T() => Console.WriteLine(2);
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode"",{embeddedSourceCode.ToLiteral()})]
"),
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
        public async Task ConditionalNone()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""enable-minify"": true
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "㘅桠ҠҠҠ侖䏂⢂䙈䟗輜邢ቲ葶构瑵䛣ሼ屽睌芇憋䉵⚘鶶鼈懟癜貙蘴◯剐嫊桮彪襤戛䔔丛檾抩嫕颱先㢤鞐燜┽㔌嫞慷噹䕓㙷岍姷㠇氬楽龆㬱ᯔ䬶婪泲㽲䎤躳䏫蚬恬篥鲡ꃀꃽ駖⛑讣刑喗ᣁ崡暿岣㨑屽煴䑣摖䣐䗜ꅢ䭌巷蛐㸐賆機滋ꕮ■ң";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task Conditional()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""remove-conditional"": [
        ""DEBUG"", ""DEBUG2""
    ],
    ""enable-minify"": true
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){T();Console.WriteLine(1);}[System.Diagnostics.Conditional(""TEST"")]static void T()=>Console.WriteLine(2);[Conditional(""DEBUG2"")]static void T4()=>Console.WriteLine(4);[System.Diagnostics.Conditional(""DEBUG2"")][Conditional(""Test"")]static void T8()=>Console.WriteLine(8);}"
                 ));
            const string embeddedSourceCode = "㘅桠ҠҠҠ偶䟚鄣㹈䝗鼝狸㥃䥠棨嫧袔爻䭨灰撘䋄脭⤭ᑗ糬ᙝ鱃滎礔┛过蘬趪喻╈缻捍坣侕恙崞箺笼ꂨ䌋㦎䷻䑵茊捪礎䳱阸鄬閍ᐾ♁ꏓ置慶ꑧڍ熩則䌁⏑徙噳賿繈☊舋砤䋫ᓮ絍擾ᚦ䟋阘嘖解铑䈣泍擩ꗟ攟⛟镥䒃ᔊ釙錕浈㺁浳ꎥ樄䒅朋㩲抰泷锐慽偡总魼不緎䀡誴舝㥣㽃橖ᔐ䓘咚蓟ᒉ鋷开貇襚䊍䝼爏虮␌并噤ҡ";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
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
        T();
        T4();
        T8();
        Console.WriteLine(1);
    }

    [System.Diagnostics.Conditional(""TEST"")]
    static void T() => Console.WriteLine(2);
    [Conditional(""DEBUG2"")]
    static void T4() => Console.WriteLine(4);
    [System.Diagnostics.Conditional(""DEBUG2"")]
    [Conditional(""Test"")]
    static void T8() => Console.WriteLine(8);
}
"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task EmbeddingSourceClassNone()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-source-class"": {
        ""enabled"": false,
        ""if-directive"": ""SOURCEEXPANDER""
    },
    ""enable-minify"": true
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "㘅桠ҠҠҠ侖䏂⢂䙈䟗輜邢ቲ葶构瑵䛣ሼ屽睌芇憋䉵⚘鶶鼈懟癜貙蘴◯剐嫊桮彪襤戛䔔丛檾抩嫕颱先㢤鞐燜┽㔌嫞慷噹䕓㙷岍姷㠇氬楽龆㬱ᯔ䬶婪泲㽲䎤躳䏫蚬恬篥鲡ꃀꃽ駖⛑讣刑喗ᣁ崡暿岣㨑屽煴䑣摖䣐䗜ꅢ䭌巷蛐㸐賆機滋ꕮ■ң";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
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
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task EmbeddingSourceClass()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-source-class"": {
        ""enabled"": true,
        ""class-name"": ""Container""
    },
    ""enable-minify"": true
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "㘅桠ҠҠҠ侖䏂⢂䙈䟗輜邢ቲ葶构瑵䛣ሼ屽睌芇憋䉵⚘鶶鼈懟癜貙蘴◯剐嫊桮彪襤戛䔔丛檾抩嫕颱先㢤鞐燜┽㔌嫞慷噹䕓㙷岍姷㠇氬楽龆㬱ᯔ䬶婪泲㽲䎤躳䏫蚬恬篥鲡ꃀꃽ駖⛑讣刑喗ᣁ崡暿岣㨑屽煴䑣摖䣐䗜ꅢ䭌巷蛐㸐賆機滋ꕮ■ң";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
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
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs", @"namespace SourceExpander.Embedded{
using System;
using System.Collections.Generic;
public class Container{
public class SourceFileInfo{
  public string FileName{get;set;}
  public string[] TypeNames{get;set;}
  public string[] Usings{get;set;}
  public string[] Dependencies{get;set;}
  public string CodeBody{get;set;}
}
  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{
    new SourceFileInfo{
      FileName = ""TestProject>Program.cs"",
      CodeBody = ""class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"",
      TypeNames = new string[]{
        ""Program"",
      },
      Usings = new string[]{
        ""using System;"",
        ""using System.Diagnostics;"",
      },
      Dependencies = new string[]{
      },
    },
  };
}
}
"
                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task EmbeddingSourceClassDefault()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-source-class"": {
        ""enabled"": true
    },
    ""enable-minify"": true
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "㘅桠ҠҠҠ侖䏂⢂䙈䟗輜邢ቲ葶构瑵䛣ሼ屽睌芇憋䉵⚘鶶鼈懟癜貙蘴◯剐嫊桮彪襤戛䔔丛檾抩嫕颱先㢤鞐燜┽㔌嫞慷噹䕓㙷岍姷㠇氬楽龆㬱ᯔ䬶婪泲㽲䎤躳䏫蚬恬篥鲡ꃀꃽ駖⛑讣刑喗ᣁ崡暿岣㨑屽煴䑣摖䣐䗜ꅢ䭌巷蛐㸐賆機滋ꕮ■ң";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
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
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs", @"namespace SourceExpander.Embedded{
using System;
using System.Collections.Generic;
public class SourceFileInfoContainer{
public class SourceFileInfo{
  public string FileName{get;set;}
  public string[] TypeNames{get;set;}
  public string[] Usings{get;set;}
  public string[] Dependencies{get;set;}
  public string CodeBody{get;set;}
}
  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{
    new SourceFileInfo{
      FileName = ""TestProject>Program.cs"",
      CodeBody = ""class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"",
      TypeNames = new string[]{
        ""Program"",
      },
      Usings = new string[]{
        ""using System;"",
        ""using System.Diagnostics;"",
      },
      Dependencies = new string[]{
      },
    },
  };
}
}
"
                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }

        [Fact]
        public async Task EmbeddingSourceClassAlways()
        {
            var additionalText = new InMemoryAdditionalText(
                "/foo/bar/SourceExpander.Embedder.Config.json", @"
{
    ""$schema"": ""https://raw.githubusercontent.com/naminodarie/SourceExpander/master/schema/embedder.schema.json"",
    ""embedding-source-class"": {
        ""enabled"": true,
        ""class-name"": """"
    },
    ""enable-minify"": true
}
");

            var embeddedFiles = ImmutableArray.Create(
                 new SourceFileInfo
                 (
                     "TestProject>Program.cs",
                     new string[] { "Program" },
                     ImmutableArray.Create("using System;", "using System.Diagnostics;"),
                     ImmutableArray.Create<string>(),
                     @"class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"
                 ));
            const string embeddedSourceCode = "㘅桠ҠҠҠ侖䏂⢂䙈䟗輜邢ቲ葶构瑵䛣ሼ屽睌芇憋䉵⚘鶶鼈懟癜貙蘴◯剐嫊桮彪襤戛䔔丛檾抩嫕颱先㢤鞐燜┽㔌嫞慷噹䕓㙷岍姷㠇氬楽龆㬱ᯔ䬶婪泲㽲䎤躳䏫蚬恬篥鲡ꃀꃽ駖⛑讣刑喗ᣁ崡暿岣㨑屽煴䑣摖䣐䗜ꅢ䭌巷蛐㸐賆機滋ꕮ■ң";

            var test = new Test
            {
                TestState =
                {
                    AdditionalFiles =
                    {
                        additionalText,
                        new InMemoryAdditionalText("/foo/bar/SourceExpander.Notmatch.json", "notmatch"),
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
                        (typeof(EmbedderGenerator), "EmbeddingSourceClass.cs", @"namespace SourceExpander.Embedded{
using System;
using System.Collections.Generic;
public class SourceFileInfoContainer{
public class SourceFileInfo{
  public string FileName{get;set;}
  public string[] TypeNames{get;set;}
  public string[] Usings{get;set;}
  public string[] Dependencies{get;set;}
  public string CodeBody{get;set;}
}
  public static readonly IReadOnlyList<SourceFileInfo> Files = new SourceFileInfo[]{
    new SourceFileInfo{
      FileName = ""TestProject>Program.cs"",
      CodeBody = ""class Program{static void Main(){Debug.Assert(true);Console.WriteLine(1);}}"",
      TypeNames = new string[]{
        ""Program"",
      },
      Usings = new string[]{
        ""using System;"",
        ""using System.Diagnostics;"",
      },
      Dependencies = new string[]{
      },
    },
  };
}
}
"
                        ),
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbedderVersion"",""{EmbedderVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedLanguageVersion"",""{EmbeddedLanguageVersion}"")]
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedSourceCode.GZipBase32768"",""{embeddedSourceCode}"")]
"),
                    }
                }
            };
            await test.RunAsync();
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embeddedSourceCode))
                .Should()
                .BeEquivalentTo(embeddedFiles);
        }
    }
}
