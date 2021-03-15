using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class AllowUnsafeTest : EmbeddingGeneratorTestBase
    {
        [Fact]
        public async Task Generate()
        {
            const string embeddedSourceCode = "㘅桠ҠҠҠ傶仺阃㹈䟿鞼瞔沥鱿悘岔⣩Ꮡ凊褖鋰㑕稵㯱漖髱歿鸼撅瞅ᰅ魤疸ꂘ馚鱇娅㻫鞣偽甈㲵⪳傀䫔㔅耝㯢㺃㽗惇蛵雳⫮㵿呂⣼轎圅曫茬潸㤁掁諍㓷贘㘨㣳ᨲ螺輖瞐赺韹歄誾脨㻦柕寏ꊝ賴䛓䝁咠灮榇䰵䣖獾睔砼鴟荬笅蜾頯楂缾芦慆ҹ䈂长▝罆▭藠ꄟ䃳貮ꙛ㵛疭攟鲧⫺綸覜徾㿂䰦讁玃⚙麝㨟韜靽㷱ꆍ䗴睦鋋頭愧㼭⠄錆ݼ宦騝譩瞸緣觉訋飫䌫侳䞔荱働㸿剘ጺ台嚿瘹繢世耀趬㢩㑨懼怟晆䊵䎰抩㦿谲䯜笽⠍ⳋ眤楊堘䌥饶餒᧿穛埊酔䱑喆任醦䙆櫵哿鯄㥝貫ᑰ㽸氥㔟誑謋殃彿狹虶浕㒲☆赘凡鞞僛貟灜鲉臓剚疻砶唙䂵譮㼟郎㖓鰑攁ꇭᙀ䒤婌ဒ⠇爯魺羾焉ꂂᕄ繽車㘛䓜畍額ᙆ宫葆锧稈鶣ꑨ㣂崇ꍲ饘膑轼䓸ᔷ涀㷩ꗓ⪿硒軄酭湪Ҡ";
            var embeddedFiles
                = ImmutableArray.Create(
                    new SourceFileInfo
                    (
                        "TestProject>F/N.cs",
                        new string[] { "Test.F.N" },
                        new string[] { "using System;", "using System.Diagnostics;", "using static System.Console;" },
                        new string[] { "TestProject>F/NumType.cs", "TestProject>Put.cs" },
                        "namespace Test.F{class N{public static void WriteN(){Console.Write(NumType.Zero);Write(\"N\");Trace.Write(\"N\");Put.Nested.Write(\"N\");}}}"
                    ), new SourceFileInfo
                    (
                        "TestProject>F/NumType.cs",
                        new string[] { "Test.F.NumType" },
                        ImmutableArray.Create<string>(),
                        ImmutableArray.Create<string>(),
                        "namespace Test.F{public enum NumType{Zero,Pos,Neg,}}"
                    ), new SourceFileInfo
                    (
                        "TestProject>I/D.cs",
                        new string[] { "Test.I.IntRecord", "Test.I.D<T>" },
                        new string[] { "using System.Diagnostics;", "using System;", "using System.Collections.Generic;" },
                        new string[] { "TestProject>Put.cs" },
                        @"namespace Test.I{public record IntRecord(int n);[System.Diagnostics.DebuggerDisplay(""TEST"")]class D<T>:IComparer<T>{public int Compare(T x,T y)=>throw new NotImplementedException();[System.Diagnostics.Conditional(""TEST"")]public static void WriteType(){Console.Write(typeof(T).FullName);Trace.Write(typeof(T).FullName);Put.Nested.Write(typeof(T).FullName);}}}"
                    ), new SourceFileInfo
                    (
                        "TestProject>Put.cs",
                        new string[] { "Test.Put", "Test.Put.Nested" },
                        new string[] { "using System.Diagnostics;" },
                        ImmutableArray.Create<string>(),
                        "namespace Test{static class Put{public class Nested{public static void Write(string v){Debug.WriteLine(v);}}}}"
                    ));

            var test = new Test
            {
                CompilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true),
                TestState =
                {
                    AdditionalFiles =
                    {
                        enableMinifyJson,
                    },
                    Sources = {
                        (
                            "/home/source/Put.cs",
                            @"using System.Diagnostics;namespace Test{static class Put{public class Nested{ public static void Write(string v){Debug.WriteLine(v);}}}}"
                        ),
                        (
                            "/home/source/I/D.cs",
                            @"using System.Diagnostics;
        using System; // used 
        using System.Threading.Tasks;// unused
        using System.Collections.Generic;
        namespace Test.I
        {
            using System.Collections;
            public record IntRecord(int n);
            [System.Diagnostics.DebuggerDisplay(""TEST"")]
            class D<T> : IComparer<T>
            {
                public int Compare(T x,T y) => throw new NotImplementedException();
                [System.Diagnostics.Conditional(""TEST"")]
                public static void WriteType()
                {
                    Console.Write(typeof(T).FullName);
                    Trace.Write(typeof(T).FullName);
                    Put.Nested.Write(typeof(T).FullName);
                }
            }
        }"
                        ),
                        (
                            "/home/source/F/N.cs",
                            @"using System;
        using System.Diagnostics;
        using static System.Console;

        namespace Test.F
        {
            class N
            {
                public static void WriteN()
                {
                    Console.Write(NumType.Zero);
                    Write(""N"");
                    Trace.Write(""N"");
                    Put.Nested.Write(""N"");
                }
            }
        }"
                        ),
                        (
                            "/home/source/F/NumType.cs",
                            @"
        namespace Test.F
        {
            public enum NumType
            {
                Zero,
                Pos,
                Neg,
            }
        }"
                        ),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs", @$"using System.Reflection;
[assembly: AssemblyMetadataAttribute(""SourceExpander.EmbeddedAllowUnsafe"",""true"")]
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
