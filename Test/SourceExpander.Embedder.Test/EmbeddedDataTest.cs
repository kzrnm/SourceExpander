using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Embedder.Test
{
    public class EmbeddedDataTest
    {
        [Fact]
        public void Empty()
        {
            EmbeddedData.Create("Empty", new Dictionary<string, string> { })
                .Should()
                .BeEquivalentTo(new EmbeddedData(
                    "Empty",
                    Array.Empty<SourceFileInfo>(),
                    new Version(1, 0, 0),
                    LanguageVersion.CSharp1
                    ));
        }

        [Fact]
        public void Version()
        {
            EmbeddedData.Create("Version", new Dictionary<string, string>
            {
                { "SourceExpander.EmbedderVersion","3.4.0.0" },
            })
                .Should()
                .BeEquivalentTo(new EmbeddedData(
                    "Version",
                    Array.Empty<SourceFileInfo>(),
                    new Version(3, 4, 0, 0),
                    LanguageVersion.CSharp1
                    ));
        }


        [Theory]
        [InlineData("5", LanguageVersion.CSharp5)]
        [InlineData("7.3", LanguageVersion.CSharp7_3)]
        [InlineData("9.0", LanguageVersion.CSharp9)]
        public void CSharpLanguageVersion(string embbeddedVersion, LanguageVersion expectedVersion)
        {
            EmbeddedData.Create("CSharpLanguageVersion", new Dictionary<string, string> {
                { "SourceExpander.EmbeddedLanguageVersion",embbeddedVersion },
            })
                .Should()
                .BeEquivalentTo(new EmbeddedData(
                    "CSharpLanguageVersion",
                    Array.Empty<SourceFileInfo>(),
                    new Version(1, 0, 0),
                    expectedVersion
                    ));
        }

        [Fact]
        public void RawJson()
        {
            string json = "[{\"CodeBody\":\"namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } \",\"Dependencies\":[],\"FileName\":\"_SampleLibrary>Bit.cs\",\"TypeNames\":[\"SampleLibrary.Bit\"],\"Usings\":[\"using System.Runtime.CompilerServices;\",\"using System.Runtime.Intrinsics.X86;\"]},{\"CodeBody\":\"namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } \",\"Dependencies\":[\"_SampleLibrary>Xorshift.cs\"],\"FileName\":\"_SampleLibrary>Put.cs\",\"TypeNames\":[\"SampleLibrary.Put\"],\"Usings\":[\"using System.Diagnostics;\"]},{\"CodeBody\":\"namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 \\/ uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } \",\"Dependencies\":[],\"FileName\":\"_SampleLibrary>Xorshift.cs\",\"TypeNames\":[\"SampleLibrary.Xorshift\"],\"Usings\":[\"using System;\"]}]";
            var expected = new EmbeddedData(
                "RawJson",
                new SourceFileInfo[] {
                    new SourceFileInfo(
                        "_SampleLibrary>Bit.cs",
                        new[]{"SampleLibrary.Bit"},
                        new[] {"using System.Runtime.CompilerServices;", "using System.Runtime.Intrinsics.X86;"},
                        Array.Empty<string>(),
                        "namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } "
                    ),
                    new SourceFileInfo(
                        "_SampleLibrary>Put.cs",
                        new[] {"SampleLibrary.Put"},
                        new[]{"using System.Diagnostics;"},
                        new[]{ "_SampleLibrary>Xorshift.cs" },
                        "namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } "
                    ),
                    new SourceFileInfo
                    (
                        "_SampleLibrary>Xorshift.cs",
                        new[] {"SampleLibrary.Xorshift"},
                        new[] {"using System;"},
                        Array.Empty<string>(),
                        "namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } "
                    ),
                },
                new Version(3, 4, 0, 0),
                LanguageVersion.CSharp7_3
                );
            EmbeddedData.Create("RawJson", new Dictionary<string, string>
            {
                { "SourceExpander.EmbeddedSourceCode", json },
                { "SourceExpander.EmbedderVersion","3.4.0.0" },
                { "SourceExpander.EmbeddedLanguageVersion","7.3" },
            })
                .Should()
                .BeEquivalentTo(expected);
            EmbeddedData.Create("RawJson", new Dictionary<string, string>
            {
                { "SourceExpander.EmbeddedLanguageVersion","7.3" },
                { "SourceExpander.EmbedderVersion","3.4.0.0" },
                { "SourceExpander.EmbeddedSourceCode", json },
            })
                .Should()
                .BeEquivalentTo(expected);
        }

        [Fact]
        public void GZipBase32768()
        {
            string gzipBase32768 = "㘅桠ҠҠԀᆖ䏚⢃㹈䝗錛㽸㨇栂祯嵔傅患踑⛩柀炁㢡垀鞽瑜䬌睜翶ꚹ㺻許ᖝᯌ劑㿯喁姬▿粻䣋鲣珳ᯀ堽羉譁䔹纼㻳纍薬卟眵䯀ꈰ疩ឝ焐疏呁猕㽿䩻勁㹡歖傠冩憅椓觇瀌鞱ꑶ⭟ᅀ篕┆盐诪ᗊ檲葷䏑唱嬻亜䞨ꂙ嫽邍韆媞害䀀ꊠҠƟ";
            var expected = new EmbeddedData(
                "GZipBase32768",
                new SourceFileInfo[] {
                    new SourceFileInfo(
                        "_SampleLibrary2>Put2.cs",
                        new[]{"SampleLibrary.Put2"},
                        Array.Empty<string>(),
                        new[] {"_SampleLibrary>Put.cs"},
                        "namespace SampleLibrary { public static class Put2 { public static void Write() => Put.WriteRandom(); } } "
                    ),
                },
                new Version(3, 4, 0, 0),
                LanguageVersion.CSharp1
                );
            EmbeddedData.Create("GZipBase32768", new Dictionary<string, string>
            {
                { "SourceExpander.EmbeddedSourceCode.GZipBase32768", gzipBase32768 },
                { "SourceExpander.EmbedderVersion","3.4.0.0" },
            })
                .Should()
                .BeEquivalentTo(expected);
            EmbeddedData.Create("GZipBase32768", new Dictionary<string, string>
            {
                { "SourceExpander.EmbedderVersion","3.4.0.0" },
                { "SourceExpander.EmbeddedLanguageVersion","1" },
                { "SourceExpander.EmbeddedSourceCode.GZipBase32768", gzipBase32768 },
            })
                .Should()
                .BeEquivalentTo(expected);
        }
    }
}
