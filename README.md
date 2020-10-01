# AtCoderLibrary.Expander

AtCoderLibrary を提出できる形式に加工するライブラリ。

## 使用方法

`Main` メソッドの中で `AtCoder.Expander.Expand();` を呼び出します。

`Main` メソッドがあるファイルと同じ階層に `Combined.csx` が生成されます。

```C#
class Program
{
    static void Main(string[] args)
    {
        Expander.Expand();
        var fw = new FenwickTree(5);
        for (int i = 0; i < 5; i++) fw.Add(i, i + 1);
        Console.WriteLine(fw.Sum(0, 5));
    }
}
```


```C#
// ExpandMethod.All
// AtCoderLibrary のすべての型を書き出す。比較的高速に動作する。
Expander.Expand(expandMethod: ExpandMethod.All);

// 引数なし
// ExpandMethod.All と同じ
Expander.Expand();

// ExpandMethod.NameSyntax
// Roslyn で NameSyntax を検索して、AtCoderLibrary の型と一致する名称があったらその型を書き出す。
// Roslyn のDLL読み込みのため少し時間がかかる。
Expander.Expand(expandMethod: ExpandMethod.NameSyntax);

// ExpandMethod.Strict
// Roslyn でコンパイルして、AtCoderLibrary の型を厳密に検索する。
// Roslyn のDLL読み込みのため時間に加えて、コンパイル時間もかかるので非常に遅い。
Expander.Expand(expandMethod: ExpandMethod.Strict);
```

**注意事項**

`Combined.csx` はビルドしたときに `Main` メソッドが記載されていたファイルのパスに出力されます。

ビルド済みDLLを移動して実行すると想定外の結果につながるので、開発環境での実行を推奨します。

### その他の使用方法

#### 別ファイルへ書き出す

```C#
Expander.Expand(@"foo/bar.cs", @"foo/baz.cs", expandMethod: ExpandMethod.Strict);
```

のようにすると `foo/bar.cs` を元に `foo/baz.cs` を作成します。

#### コード上で処理する

```C#
string outputCode = CodeExpander.Expand(File.ReadAllText(path), expandMethod: ExpandMethod.Strict);
```

のように、`string`→`string` の変換にも対応しています。



## ファイル構成

### Expander

ライブラリ本体

#### Samples

Expander を使用する際のサンプル。

## Lisence

本ライブラリは MIT Licenseで提供しています。

ただし、`Sample` ディレクトリ以下のファイルは CC0 ライセンスで提供しています。