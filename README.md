# SourceExpander

README languages: 
- [English](README.md) / [日本語](README.ja.md)


<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**

- [Packages](#packages)
  - [SourceExpander(library)](#sourceexpanderlibrary)
  - [SourceExpander.Generator](#sourceexpandergenerator)
  - [SourceExpander.Embedder](#sourceexpanderembedder)
- [Status](#status)
- [Getting started](#getting-started)
  - [For library user](#for-library-user)
  - [For library developer](#for-library-developer)
    - [Analyzer(optional)](#analyzeroptional)
  - [Notes](#notes)
- [Embedded data](#embedded-data)
  - [EmbedderVersion](#embedderversion)
  - [EmbeddedLanguageVersion](#embeddedlanguageversion)
  - [EmbeddedAllowUnsafe](#embeddedallowunsafe)
  - [EmbeddedSourceCode](#embeddedsourcecode)
    - [EmbeddedSourceCode.GZipBase32768](#embeddedsourcecodegzipbase32768)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Packages

### SourceExpander(library)

Library that expand embedded source codes.


### SourceExpander.Generator

Source generator that expand embedded source codes.


### SourceExpander.Embedder

Source generator that embed source codes.

## Status

![build](https://github.com/naminodarie/SourceExpander/workflows/Build-Release-Publish/badge.svg?branch=master)

|Library|NuGet|
|:---|:---|
|SourceExpander|[![NuGet version (SourceExpander)](https://img.shields.io/nuget/v/SourceExpander.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander/)|
|SourceExpander.Core|[![NuGet version (SourceExpander.Core)](https://img.shields.io/nuget/v/SourceExpander.Core.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander.Core/)|
|SourceExpander.Embedder|[![NuGet version (SourceExpander.Embedder)](https://img.shields.io/nuget/v/SourceExpander.Embedder.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander.Embedder/)|
|SourceExpander.Embedder.Analyzer|[![NuGet version (SourceExpander.Embedder.Analyzer)](https://img.shields.io/nuget/v/SourceExpander.Embedder.Analyzer.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander.Embedder.Analyzer/)|
|SourceExpander.Generator|[![NuGet version (SourceExpander.Generator)](https://img.shields.io/nuget/v/SourceExpander.Generator.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander.Generator/)|

## Getting started

This library require **.NET 5 SDK** or **Visual Studio 16.8** or later because this library use Source Generators.

### For library user

see [Sample](/Sample) or https://github.com/naminodarie/ac-library-csharp

```
Install-Package SourceExpander
Install-Package <A library with embedded source>
```

```C#
using System;
class Program
{
    static void Main()
    {
        SourceExpander.Expander.Expand();
        // Your code
    }
}
```

When you run the code, `SourceExpander.Expander.Expand()` create new file that combined library code.

```C#
using System;
class Program
{
    static void Main()
    {
        SourceExpander.Expander.Expand();
        // Your code
    }
}

#region Expanded by https://github.com/naminodarie/SourceExpander
namespace SourceExpander { public class Expander { [Conditional("EXPANDER")] public static void Expand(string inputFilePath = null, string outputFilePath = null, bool ignoreAnyError = true) { } public static string ExpandString(string inputFilePath = null, bool ignoreAnyError = true) { return ""; } } } 
// library code
#endregion Expanded by https://github.com/naminodarie/SourceExpander
```

### For library developer

It's easy, just install `SourceExpander.Embedder`.

```
Install-Package SourceExpander.Embedder
```

#### Avoid embedding some type

Embedding is skipped for type that have `SourceExpander.NotEmbeddingSourceAttribute`.

#### Analyzer(optional)

```
Install-Package SourceExpander.Embedder.Analyzer
```

### Notes
Because `SourceExpander.Embedder` run at compile time, the embedded source code cannot be used in the same project.

## Embedded data

`SourceExpander.Embedder` embed some data like below.

```C#
using System.Reflection;
[assembly: AssemblyMetadata("SourceExpander.EmbedderVersion", "2.5.0.101")]
[assembly: AssemblyMetadata("SourceExpander.EmbeddedLanguageVersion", "2")]
[assembly: AssemblyMetadata("SourceExpander.EmbeddedAllowUnsafe", "true")]
[assembly: AssemblyMetadata("SourceExpander.EmbeddedSourceCode.GZipBase32768", "㘅桠ҠҠԀᏕ䴾阺㹈斪筟楸厮嫉盆炚磈臤梽胍㦬竂帙詪煩㔬樄ᗗ踜鲯诇ᠩ珱䪜䐽闾鱏珣茙灸䏙⨧㤄寨砳⬅ស䮙松Ꝉ㥅䱀餯ꃣ虱嫁榏㪰糰蝃技夛䥘谼礞䐿斄禕蚷屔彺㪪賳鱥䝢鰨覶⬴誼⬼獬鞨胒宝䭴摺眚䅗䃝䚏隻嫻痛簴Ꜿ変⇣㇋聼欈Ꭽ墷霶勎嶐窢銖㤁┠䁺⠛缧䋹凬☂䁸栣僼邐䑹瘜蛭諠賿㨚咈鍂ꄱ禱唨毊崨叼緭䥜榄闺䦖麷䘘㨵ᖶ琜鎎ᰇ髎飭㪬採ꅈ㥞盧䢽䃘煃⬘喔渻莖案ᯋ硟ꋛ叝谴缄ꍢ⋗溁ᣒ颂浢ꍈꉭ㑆焤鹠杳煄㾳䴡䂱㙽楯裦鷬梙掫取颤⩑㰑㕋ꂤ碎麓㾕昖啘繅餬簚盎鍣䨽籭詽绑襌硲❞擧ꌥ膩辪聫㭒珥㴟囓䓖焜铽痢ꊆꍼᓥ囦纇維Ⲡ㤬垇螇感縋㼎砾褳強襓瀕樥阵瀭蜺兔峃絻藈萢饑㶬櫊綖嶅鏕㻶坶禵䓓Ⴐ咇詤煑⬐毱㱒獅鐥椳䖑ᙋ冄㴼㗭隯顑命貽职葅苫⢸栚䀹䢳噂槝䲰䰮⇷ᔈ⎙䕪絑㝖垿䞉場珟䉛㰭䵶日憭蕼馣㸩涴䓋䃇懚鹯琥镌ⴊ電萞猛流癊⏔恚Ԉң")]
//[assembly: AssemblyMetadata("SourceExpander.EmbeddedSourceCode", "[{\"CodeBody\":\"namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } \",\"Dependencies\":[],\"FileName\":\"_SampleLibrary>Bit.cs\",\"TypeNames\":[\"SampleLibrary.Bit\"],\"Usings\":[\"using System.Runtime.CompilerServices;\",\"using System.Runtime.Intrinsics.X86;\"]},{\"CodeBody\":\"namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() { Trace.WriteLine(rnd.Next()); } } } \",\"Dependencies\":[\"_SampleLibrary>Xorshift.cs\"],\"FileName\":\"_SampleLibrary>Put.cs\",\"TypeNames\":[\"SampleLibrary.Put\"],\"Usings\":[\"using System.Diagnostics;\"]},{\"CodeBody\":\"namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() { return InternalSample() * (1.0 \\/ uint.MaxValue); } private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } \",\"Dependencies\":[],\"FileName\":\"_SampleLibrary>Xorshift.cs\",\"TypeNames\":[\"SampleLibrary.Xorshift\"],\"Usings\":[\"using System;\"]}]")]
```


### EmbedderVersion

AssemblyVersion of `SourceExpander.Embedder`.

### EmbeddedLanguageVersion

C# version of embbeded source code.

### EmbeddedAllowUnsafe

if `true`, embbeded source code allow unsafe code.

### EmbeddedSourceCode

Actually, this metadata does not embedded. for explanation.

json seriarized array of `SourceFileInfo`.

```C#
public class SourceFileInfo
{
    /// <summary>
    /// Unique name of file
    /// </summary>
    public string FileName { get; set; }
    /// <summary>
    /// Defined types like class, struct, record, enum, delegate
    /// </summary>
    public IEnumerable<string> TypeNames { get; set; }
    /// <summary>
    /// Using directives
    /// </summary>
    public IEnumerable<string> Usings { get; set; }
    /// <summary>
    /// FileNames that the this depending on
    /// </summary>
    public IEnumerable<string> Dependencies { get; set; }
    /// <summary>
    /// Code body that removed using directives
    /// </summary>
    public string CodeBody { get; set; }
}
```

#### EmbeddedSourceCode.GZipBase32768

gzip and [base32768](https://github.com/naminodarie/Base32768/) encoded json.

