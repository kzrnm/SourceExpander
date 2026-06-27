using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SourceExpander.Share;

public class EmbeddedDataTest
{
    [Test]
    public async Task Empty()
    {
        var (data, errors) = EmbeddedData.Create("Empty", ImmutableDictionary.Create<string, string>());
        await errors.Should().BeEmpty();
        await data.Should().BeEqualTo(new EmbeddedData(
                "Empty",
                [],
                new Version(1, 0, 0),
                LanguageVersion.CSharp1.ToDisplayString(),
                false,
                ImmutableArray<string>.Empty
                ), TestUtil.EmbeddedDataEqualityComparer);
    }

    [Test]
    public async Task Version()
    {
        var (data, errors) = EmbeddedData.Create("Version",
            ImmutableDictionary.Create<string, string>().Add("SourceExpander.EmbedderVersion", "3.4.0.0"));
        await errors.Should().BeEmpty();
        await data.Should().BeEqualTo(new EmbeddedData(
                "Version",
                [],
                new Version(3, 4, 0, 0),
                LanguageVersion.CSharp1.ToDisplayString(),
                false,
                ImmutableArray<string>.Empty
                ), TestUtil.EmbeddedDataEqualityComparer);
    }


    [Test]
    [Arguments("5", LanguageVersion.CSharp5)]
    [Arguments("7.3", LanguageVersion.CSharp7_3)]
    [Arguments("9.0", LanguageVersion.CSharp9)]
    public async Task CSharpLanguageVersion(string embbeddedVersion, LanguageVersion expectedVersion)
    {
        var (data, errors) = EmbeddedData.Create("CSharpLanguageVersion",
            ImmutableDictionary.Create<string, string>().Add("SourceExpander.EmbeddedLanguageVersion", embbeddedVersion));
        await errors.Should().BeEmpty();
        await data.Should().BeEqualTo(new EmbeddedData(
            "CSharpLanguageVersion",
            [],
            new Version(1, 0, 0),
            expectedVersion.ToDisplayString(),
            false,
            ImmutableArray<string>.Empty
        ), TestUtil.EmbeddedDataEqualityComparer);
    }

    [Test]
    public async Task RawJson()
    {
        string json = "[{\"CodeBody\":\"namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } \",\"Dependencies\":[],\"FileName\":\"_SampleLibrary>Bit.cs\",\"TypeNames\":[\"SampleLibrary.Bit\"],\"Usings\":[\"using System.Runtime.CompilerServices;\",\"using System.Runtime.Intrinsics.X86;\"]},{\"CodeBody\":\"namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } \",\"Dependencies\":[\"_SampleLibrary>Xorshift.cs\"],\"FileName\":\"_SampleLibrary>Put.cs\",\"TypeNames\":[\"SampleLibrary.Put\"],\"Usings\":[\"using System.Diagnostics;\"]},{\"CodeBody\":\"namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 \\/ uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } \",\"Dependencies\":[],\"FileName\":\"_SampleLibrary>Xorshift.cs\",\"TypeNames\":[\"SampleLibrary.Xorshift\"],\"Usings\":[\"using System;\"]}]";
        var expected = new EmbeddedData(
            "RawJson",
            [
                new SourceFileInfo(
                    "_SampleLibrary>Bit.cs",
                    ["SampleLibrary.Bit"],
                    ["using System.Runtime.CompilerServices;", "using System.Runtime.Intrinsics.X86;"],
                    Array.Empty<string>(),
                    "namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } "
                ),
                new SourceFileInfo(
                    "_SampleLibrary>Put.cs",
                    ["SampleLibrary.Put"],
                    ["using System.Diagnostics;"],
                    ["_SampleLibrary>Xorshift.cs"],
                    "namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } "
                ),
                new SourceFileInfo
                (
                    "_SampleLibrary>Xorshift.cs",
                    ["SampleLibrary.Xorshift"],
                    ["using System;"],
                    Array.Empty<string>(),
                    "namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } "
                )
            ],
            new Version(3, 4, 0, 0),
            LanguageVersion.CSharp7_3.ToDisplayString(),
            false,
            ["SampleLibrary"]);
        var (data, errors) = EmbeddedData.Create("RawJson",
                       ImmutableDictionary.Create<string, string>()
                       .Add("SourceExpander.EmbeddedSourceCode", json)
                       .Add("SourceExpander.EmbeddedNamespaces", "SampleLibrary")
                       .Add("SourceExpander.EmbedderVersion", "3.4.0.0")
                       .Add("SourceExpander.EmbeddedLanguageVersion", "7.3"));
        await errors.Should().BeEmpty();
        await data.Should().BeEqualTo(expected, TestUtil.EmbeddedDataEqualityComparer);

        (data, errors) = EmbeddedData.Create("RawJson",
            ImmutableDictionary.Create<string, string>()
            .Add("SourceExpander.EmbeddedLanguageVersion", "7.3")
            .Add("SourceExpander.EmbeddedNamespaces", "SampleLibrary")
            .Add("SourceExpander.EmbedderVersion", "3.4.0.0")
            .Add("SourceExpander.EmbeddedSourceCode", json));
        await errors.Should().BeEmpty();
        await data.Should().BeEqualTo(expected, TestUtil.EmbeddedDataEqualityComparer);
    }

    [Test]
    public async Task RawJsonError()
    {
        string json = "[{]}]";
        var (data, errors) = EmbeddedData.Create("RawJson",
                       ImmutableDictionary.Create<string, string>()
                       .Add("SourceExpander.EmbeddedSourceCode", json)
                       .Add("SourceExpander.EmbedderVersion", "3.4.0.0")
                       .Add("SourceExpander.EmbeddedNamespaces", "SampleLibrary")
                       .Add("SourceExpander.EmbeddedLanguageVersion", "7.3"));
        await errors.Should().BeEquivalentTo([("SourceExpander.EmbeddedSourceCode", "Invalid property identifier character: ]. Path '[0]', line 1, position 2.")]);
        await data.Should().BeEqualTo(
            new EmbeddedData("RawJson",
            ImmutableArray<SourceFileInfo>.Empty,
            new(3, 4, 0, 0),
            LanguageVersion.CSharp7_3.ToDisplayString(),
            false,
            ["SampleLibrary"]), TestUtil.EmbeddedDataEqualityComparer);

        (data, errors) = EmbeddedData.Create("RawJson",
                      ImmutableDictionary.Create<string, string>()
                     .Add("SourceExpander.EmbeddedLanguageVersion", "7.3")
                     .Add("SourceExpander.EmbeddedNamespaces", "SampleLibrary")
                     .Add("SourceExpander.EmbedderVersion", "3.4.0.0")
                     .Add("SourceExpander.EmbeddedSourceCode", json));

        await errors.Should().BeEquivalentTo([("SourceExpander.EmbeddedSourceCode", "Invalid property identifier character: ]. Path '[0]', line 1, position 2.")]);
        await data.Should().BeEqualTo(
            new EmbeddedData("RawJson",
            ImmutableArray<SourceFileInfo>.Empty,
            new(3, 4, 0, 0),
            LanguageVersion.CSharp7_3.ToDisplayString(),
            false,
            ["SampleLibrary"]), TestUtil.EmbeddedDataEqualityComparer);
    }
    [Test]
    public async Task GZipBase32768()
    {
        string gzipBase32768 = "㘅桠ҠҠԀᆖ䏚⢃㹈䝗錛㽸㨇栂祯嵔傅患踑⛩柀炁㢡垀鞽瑜䬌睜翶ꚹ㺻許ᖝᯌ劑㿯喁姬▿粻䣋鲣珳ᯀ堽羉譁䔹纼㻳纍薬卟眵䯀ꈰ疩ឝ焐疏呁猕㽿䩻勁㹡歖傠冩憅椓觇瀌鞱ꑶ⭟ᅀ篕┆盐诪ᗊ檲葷䏑唱嬻亜䞨ꂙ嫽邍韆媞害䀀ꊠҠƟ";
        var expected = new EmbeddedData(
            "GZipBase32768",
            [
                new SourceFileInfo(
                    "_SampleLibrary2>Put2.cs",
                    ["SampleLibrary.Put2"],
                    Array.Empty<string>(),
                    ["_SampleLibrary>Put.cs"],
                    "namespace SampleLibrary { public static class Put2 { public static void Write() => Put.WriteRandom(); } } "
                )
            ],
            new Version(3, 4, 0, 0),
            LanguageVersion.CSharp1.ToDisplayString(),
            false,
            ["SampleLibrary"]
            );

        var (data, errors) = EmbeddedData.Create("GZipBase32768", ImmutableDictionary.Create<string, string>()
                       .Add("SourceExpander.EmbeddedSourceCode.GZipBase32768", gzipBase32768)
                       .Add("SourceExpander.EmbeddedNamespaces", "SampleLibrary")
                       .Add("SourceExpander.EmbedderVersion", "3.4.0.0")
                   );
        await errors.Should().BeEmpty();
        await data.Should().BeEqualTo(expected, TestUtil.EmbeddedDataEqualityComparer);
        (data, errors) = EmbeddedData.Create("GZipBase32768", ImmutableDictionary.Create<string, string>()
                      .Add("SourceExpander.EmbedderVersion", "3.4.0.0")
                      .Add("SourceExpander.EmbeddedLanguageVersion", "1")
                      .Add("SourceExpander.EmbeddedNamespaces", "SampleLibrary")
                      .Add("SourceExpander.EmbeddedSourceCode.GZipBase32768", gzipBase32768)
                  );
        await errors.Should().BeEmpty();
        await data.Should().BeEqualTo(expected, TestUtil.EmbeddedDataEqualityComparer);
    }

    [Test]
    public async Task ToJson()
    {
        var jsonSerializerOptions = new System.Text.Json.JsonSerializerOptions()
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        var jsonSerializerSettings = new JsonSerializerSettings()
        {
            Formatting = Formatting.None,
            StringEscapeHandling = StringEscapeHandling.Default,
        };

        var data = new EmbeddedData(
            "RawJson",
            [
                new SourceFileInfo(
                    "_SampleLibrary>Bit.cs",
                    ["SampleLibrary.Bit"],
                    ["using System.Runtime.CompilerServices;", "using System.Runtime.Intrinsics.X86;"],
                    Array.Empty<string>(),
                    "namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } "
                ),
                new SourceFileInfo(
                    "_SampleLibrary>Put.cs",
                    ["SampleLibrary.Put"],
                    ["using System.Diagnostics;"],
                    ["_SampleLibrary>Xorshift.cs"],
                    "namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } "
                ),
                new SourceFileInfo
                (
                    "_SampleLibrary>Xorshift.cs",
                    ["SampleLibrary.Xorshift"],
                    ["using System;"],
                    Array.Empty<string>(),
                    "namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } "
                )
            ],
            new Version(3, 4, 0, 0),
            LanguageVersion.CSharp7_3.ToDisplayString(),
            false,
            ["SampleLibrary"]);


        using (Assert.Multiple())
        {
            foreach (var json in new[]
            {
                JsonConvert.SerializeObject(data, jsonSerializerSettings),
                System.Text.Json.JsonSerializer.Serialize(data, jsonSerializerOptions),
            })
                using (Assert.Multiple())
                {
                    await json.Should().BeEqualTo(
                        """
                        {
                            "AssemblyName": "RawJson",
                            "Sources": [
                                {
                                    "CodeBody": "namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } ",
                                    "Dependencies": [],
                                    "FileName": "_SampleLibrary>Bit.cs",
                                    "TypeNames": [
                                        "SampleLibrary.Bit"
                                    ],
                                    "Usings": [
                                        "using System.Runtime.CompilerServices;",
                                        "using System.Runtime.Intrinsics.X86;"
                                    ]
                                },
                                {
                                    "CodeBody": "namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } ",
                                    "Dependencies": [
                                        "_SampleLibrary>Xorshift.cs"
                                    ],
                                    "FileName": "_SampleLibrary>Put.cs",
                                    "TypeNames": [
                                        "SampleLibrary.Put"
                                    ],
                                    "Usings": [
                                        "using System.Diagnostics;"
                                    ]
                                },
                                {
                                    "CodeBody": "namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } ",
                                    "Dependencies": [],
                                    "FileName": "_SampleLibrary>Xorshift.cs",
                                    "TypeNames": [
                                        "SampleLibrary.Xorshift"
                                    ],
                                    "Usings": [
                                        "using System;"
                                    ]
                                }
                            ],
                            "EmbedderVersion": "3.4.0.0",
                            "CSharpVersion": "7.3",
                            "AllowUnsafe": false,
                            "EmbeddedNamespaces": [
                                "SampleLibrary"
                            ]
                        }
                        """, TestUtil.JsonEqualityComparer);
                    await JsonConvert.DeserializeObject<EmbeddedData>(json, jsonSerializerSettings)
                        .Should().BeEqualTo(data, TestUtil.EmbeddedDataEqualityComparer);
                    await System.Text.Json.JsonSerializer.Deserialize<EmbeddedData>(json, jsonSerializerOptions)
                        .Should().BeEqualTo(data, TestUtil.EmbeddedDataEqualityComparer);
                }
        }

        data = data with
        {
            AllowUnsafe = true,
            CSharpVersion = "preview",
            AssemblyName = "Assembly2",
            EmbedderVersion = new Version(1, 2, 3, 4),
            EmbeddedNamespaces = ["SampleLibrary", "SampleLibrary.Put"],
        };

        using (Assert.Multiple())
        {
            foreach (var json in new[]
            {
                JsonConvert.SerializeObject(data, jsonSerializerSettings),
                System.Text.Json.JsonSerializer.Serialize(data, jsonSerializerOptions),
            })
                using (Assert.Multiple())
                {
                    await json.Should().BeEqualTo(
                        """
                        {
                            "AssemblyName": "Assembly2",
                            "Sources": [
                                {
                                    "CodeBody": "namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } ",
                                    "Dependencies": [],
                                    "FileName": "_SampleLibrary>Bit.cs",
                                    "TypeNames": [
                                        "SampleLibrary.Bit"
                                    ],
                                    "Usings": [
                                        "using System.Runtime.CompilerServices;",
                                        "using System.Runtime.Intrinsics.X86;"
                                    ]
                                },
                                {
                                    "CodeBody": "namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } ",
                                    "Dependencies": [
                                        "_SampleLibrary>Xorshift.cs"
                                    ],
                                    "FileName": "_SampleLibrary>Put.cs",
                                    "TypeNames": [
                                        "SampleLibrary.Put"
                                    ],
                                    "Usings": [
                                        "using System.Diagnostics;"
                                    ]
                                },
                                {
                                    "CodeBody": "namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } ",
                                    "Dependencies": [],
                                    "FileName": "_SampleLibrary>Xorshift.cs",
                                    "TypeNames": [
                                        "SampleLibrary.Xorshift"
                                    ],
                                    "Usings": [
                                        "using System;"
                                    ]
                                }
                            ],
                            "EmbedderVersion": "1.2.3.4",
                            "CSharpVersion": "preview",
                            "AllowUnsafe": true,
                            "EmbeddedNamespaces": [
                                "SampleLibrary",
                                "SampleLibrary.Put"
                            ]
                        }
                        """, TestUtil.JsonEqualityComparer);

                    await JsonConvert.DeserializeObject<EmbeddedData>(json, jsonSerializerSettings)
                        .Should().BeEqualTo(data, TestUtil.EmbeddedDataEqualityComparer);
                    await System.Text.Json.JsonSerializer.Deserialize<EmbeddedData>(json, jsonSerializerOptions)
                        .Should().BeEqualTo(data, TestUtil.EmbeddedDataEqualityComparer);
                }
        }
    }
}
