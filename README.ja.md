# SourceExpander

README languages: 
- [English](README.md) / [日本語](README.ja.md)


<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
**Table of Contents**

- [Packages](#packages)
  - [SourceExpander(library)](#sourceexpanderlibrary)
  - [SourceExpander.Console](#sourceexpanderconsole)
  - [SourceExpander.Generator](#sourceexpandergenerator)
  - [SourceExpander.Embedder](#sourceexpanderembedder)
- [Status](#status)
- [Getting started](#getting-started)
  - [ライブラリ利用者向け](#%E3%83%A9%E3%82%A4%E3%83%96%E3%83%A9%E3%83%AA%E5%88%A9%E7%94%A8%E8%80%85%E5%90%91%E3%81%91)
    - [SourceExpander.Console を使う](#sourceexpanderconsole-%E3%82%92%E4%BD%BF%E3%81%86)
    - [SourceExpander.Generator を使う](#sourceexpandergenerator-%E3%82%92%E4%BD%BF%E3%81%86)
  - [ライブラリ開発者向け](#%E3%83%A9%E3%82%A4%E3%83%96%E3%83%A9%E3%83%AA%E9%96%8B%E7%99%BA%E8%80%85%E5%90%91%E3%81%91)
    - [埋め込みたくない型への対処](#%E5%9F%8B%E3%82%81%E8%BE%BC%E3%81%BF%E3%81%9F%E3%81%8F%E3%81%AA%E3%81%84%E5%9E%8B%E3%81%B8%E3%81%AE%E5%AF%BE%E5%87%A6)
  - [注釈](#%E6%B3%A8%E9%87%88)
- [埋め込まれるデータ](#%E5%9F%8B%E3%82%81%E8%BE%BC%E3%81%BE%E3%82%8C%E3%82%8B%E3%83%87%E3%83%BC%E3%82%BF)
  - [EmbedderVersion](#embedderversion)
  - [EmbeddedLanguageVersion](#embeddedlanguageversion)
  - [EmbeddedAllowUnsafe](#embeddedallowunsafe)
  - [EmbeddedSourceCode](#embeddedsourcecode)
    - [EmbeddedSourceCode.GZipBase32768](#embeddedsourcecodegzipbase32768)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Packages

### SourceExpander(library)

ソースコードをファイルに展開するライブラリです。

### SourceExpander.Console

ソースジェネレーターで埋め込まれたソースコードを展開するコンソールアプリです。

### SourceExpander.Generator

ソースジェネレーターで埋め込まれたソースコードを展開するライブラリです。

### SourceExpander.Embedder

ソースコードを埋め込むライブラリです。

## Status

![build](https://github.com/naminodarie/SourceExpander/workflows/Build-Release-Publish/badge.svg?branch=master)

|Library|NuGet|
|:---|:---|
|SourceExpander|[![NuGet version (SourceExpander)](https://img.shields.io/nuget/v/SourceExpander.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander/)|
|SourceExpander.Core|[![NuGet version (SourceExpander.Core)](https://img.shields.io/nuget/v/SourceExpander.Core.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander.Core/)|
|SourceExpander.Embedder|[![NuGet version (SourceExpander.Embedder)](https://img.shields.io/nuget/v/SourceExpander.Embedder.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander.Embedder/)|
|SourceExpander.Generator|[![NuGet version (SourceExpander.Generator)](https://img.shields.io/nuget/v/SourceExpander.Generator.svg?style=flat-square)](https://www.nuget.org/packages/SourceExpander.Generator/)|

## Getting started

このライブラリはソースジェネレーターを使用するため、 **.NET 5 SDK** または **Visual Studio 16.8** 以降が必須です。

### ライブラリ利用者向け

#### SourceExpander.Console を使う

Install:
```sh
dotnet tool install -g SourceExpander.Console
```

Run:
```sh
# minimum run
dotnet-source-expand Sample/SampleProject2/Program.cs

# specified project
dotnet-source-expand Sample/SampleProject/Put.cs -p Sample/SampleProject2/SampleProject2.csproj
```

#### SourceExpander.Generator を使う

[Sample](/Sample) や https://github.com/naminodarie/ac-library-csharp を参考としてください。

```
Install-Package SourceExpander
Install-Package <ソースコードが埋め込まれたライブラリ>
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

このコードを実行すると, `SourceExpander.Expander.Expand()`でソースコードが結合された下記のようなファイルが出力されます。

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

### ライブラリ開発者向け

`SourceExpander.Embedder` をインストールするだけでOKです。

```
Install-Package SourceExpander.Embedder
```

#### 埋め込みたくない型への対処

`SourceExpander.NotEmbeddingSourceAttribute` を適用した型については埋め込みをスキップします。

### 注釈

`SourceExpander.Embedder`はコンパイル時に実行されるので、埋め込んだソースコードを同一のプロジェクトで利用することはできません。

## 埋め込まれるデータ

`SourceExpander.Embedder` は下記のようなコードを埋め込みます。

```C#
using System.Reflection;
[assembly: AssemblyMetadata("SourceExpander.EmbedderVersion", "2.5.0.101")]
[assembly: AssemblyMetadata("SourceExpander.EmbeddedLanguageVersion", "2")]
[assembly: AssemblyMetadata("SourceExpander.EmbeddedAllowUnsafe", "true")]
[assembly: AssemblyMetadata("SourceExpander.EmbeddedSourceCode.GZipBase32768", "㘅桠ҠҠԀᏕ䴾阺㹈斪筟楸厮嫉盆炚磈臤梽胍㦬竂帙詪煩㔬樄ᗗ踜鲯诇ᠩ珱䪜䐽闾鱏珣茙灸䏙⨧㤄寨砳⬅ស䮙松Ꝉ㥅䱀餯ꃣ虱嫁榏㪰糰蝃技夛䥘谼礞䐿斄禕蚷屔彺㪪賳鱥䝢鰨覶⬴誼⬼獬鞨胒宝䭴摺眚䅗䃝䚏隻嫻痛簴Ꜿ変⇣㇋聼欈Ꭽ墷霶勎嶐窢銖㤁┠䁺⠛缧䋹凬☂䁸栣僼邐䑹瘜蛭諠賿㨚咈鍂ꄱ禱唨毊崨叼緭䥜榄闺䦖麷䘘㨵ᖶ琜鎎ᰇ髎飭㪬採ꅈ㥞盧䢽䃘煃⬘喔渻莖案ᯋ硟ꋛ叝谴缄ꍢ⋗溁ᣒ颂浢ꍈꉭ㑆焤鹠杳煄㾳䴡䂱㙽楯裦鷬梙掫取颤⩑㰑㕋ꂤ碎麓㾕昖啘繅餬簚盎鍣䨽籭詽绑襌硲❞擧ꌥ膩辪聫㭒珥㴟囓䓖焜铽痢ꊆꍼᓥ囦纇維Ⲡ㤬垇螇感縋㼎砾褳強襓瀕樥阵瀭蜺兔峃絻藈萢饑㶬櫊綖嶅鏕㻶坶禵䓓Ⴐ咇詤煑⬐毱㱒獅鐥椳䖑ᙋ冄㴼㗭隯顑命貽职葅苫⢸栚䀹䢳噂槝䲰䰮⇷ᔈ⎙䕪絑㝖垿䞉場珟䉛㰭䵶日憭蕼馣㸩涴䓋䃇懚鹯琥镌ⴊ電萞猛流癊⏔恚Ԉң")]
//[assembly: AssemblyMetadata("SourceExpander.EmbeddedSourceCode", "[{\"CodeBody\":\"namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } \",\"Dependencies\":[],\"FileName\":\"_SampleLibrary>Bit.cs\",\"TypeNames\":[\"SampleLibrary.Bit\"],\"Usings\":[\"using System.Runtime.CompilerServices;\",\"using System.Runtime.Intrinsics.X86;\"]},{\"CodeBody\":\"namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() { Trace.WriteLine(rnd.Next()); } } } \",\"Dependencies\":[\"_SampleLibrary>Xorshift.cs\"],\"FileName\":\"_SampleLibrary>Put.cs\",\"TypeNames\":[\"SampleLibrary.Put\"],\"Usings\":[\"using System.Diagnostics;\"]},{\"CodeBody\":\"namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() { return InternalSample() * (1.0 \\/ uint.MaxValue); } private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } \",\"Dependencies\":[],\"FileName\":\"_SampleLibrary>Xorshift.cs\",\"TypeNames\":[\"SampleLibrary.Xorshift\"],\"Usings\":[\"using System;\"]}]")]
```


### EmbedderVersion

`SourceExpander.Embedder` の AssemblyVersion です。

### EmbeddedLanguageVersion

埋め込まれたソースコードの C# のバージョンです。

### EmbeddedAllowUnsafe

`true` ならば埋め込まれたソースコードは `unsafe` が許可されています。

### EmbeddedSourceCode

実際には埋め込まれませんが、説明用に記述します。

`SourceFileInfo` を JSON シリアライズしたものです。

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

EmbeddedSourceCode の JSON を gzip 圧縮し、[base32768](https://github.com/naminodarie/Base32768/) でエンコードしたものです。
