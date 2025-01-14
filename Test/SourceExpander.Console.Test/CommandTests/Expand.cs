using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander;

public partial class CommandTests
{
    [Fact]
    public async Task Expand()
    {
        using var sw = new StringWriter();
        var target = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs");
        await new SourceExpanderCommand { Stdout = sw }.Expand(target, cancellationToken: TestContext.Current.CancellationToken);
        sw.ToString().ReplaceLineEndings().Should().Be("""
using AtCoder;
using AtCoder.Internal;
using SampleLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var uf = new UnionFind(3);
            uf.Merge(1, 2);
            Console.WriteLine(uf.Leader(2));
        }
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace AtCoder{public class Dsu{public readonly int[]_ps;public Dsu(int n){_ps=new int[n];_ps.AsSpan().Fill(-1);}[MethodImpl(256)]public int Merge(int a,int b){int x=Leader(a),y=Leader(b);if(x==y)return x;if(-_ps[x]<-_ps[y])(x,y)=(y,x);_ps[x]+=_ps[y];_ps[y]=x;return x;}[MethodImpl(256)]public bool Same(int a,int b){return Leader(a)==Leader(b);}[MethodImpl(256)]public int Leader(int a){if(_ps[a]<0)return a;while(0<=_ps[_ps[a]]){(a,_ps[a])=(_ps[a],_ps[_ps[a]]);}return _ps[a];}[MethodImpl(256)]public int Size(int a){return-_ps[Leader(a)];}[MethodImpl(256)]public int[][]Groups(){int n=_ps.Length;int[]leaderBuf=new int[n];int[]id=new int[n];var resultList=new SimpleList<int[]>(n);for(int i=0;i<leaderBuf.Length;i++){leaderBuf[i]=Leader(i);if(i==leaderBuf[i]){id[i]=resultList.Count;resultList.Add(new int[-_ps[i]]);}}var result=resultList.ToArray();int[]ind=new int[result.Length];for(int i=0;i<leaderBuf.Length;i++){var leaderID=id[leaderBuf[i]];result[leaderID][ind[leaderID]]=i;ind[leaderID]++;}return result;}}}
namespace AtCoder.Internal{public class SimpleList<T>:IList<T>,IReadOnlyList<T>{public T[]data;private const int DefaultCapacity=2;public SimpleList(){data=new T[DefaultCapacity];}public SimpleList(int capacity){data=new T[Math.Max(capacity,DefaultCapacity)];}public SimpleList(IEnumerable<T>collection){if(collection is ICollection<T>col){data=new T[col.Count];col.CopyTo(data,0);Count=col.Count;}else{data=new T[DefaultCapacity];foreach(var item in collection)Add(item);}}[MethodImpl(256)]public Memory<T>AsMemory()=>new Memory<T>(data,0,Count);[MethodImpl(256)]public Span<T>AsSpan()=>new Span<T>(data,0,Count);public ref T this[int index]{[MethodImpl(256)]get{if((uint)index>=(uint)Count)ThrowIndexOutOfRangeException();return ref data[index];}}public int Count{get;private set;}[MethodImpl(256)]public void Add(T item){if((uint)Count>=(uint)data.Length)Array.Resize(ref data,data.Length<<1);data[Count++]=item;}[MethodImpl(256)]public void RemoveLast(){if( --Count<0)ThrowIndexOutOfRangeException();}[MethodImpl(256)]public void RemoveLast(int size){if((Count-=size)<0)ThrowIndexOutOfRangeException();}[MethodImpl(256)]public SimpleList<T>Reverse(){Array.Reverse(data,0,Count);return this;}[MethodImpl(256)]public SimpleList<T>Reverse(int index,int count){Array.Reverse(data,index,count);return this;}[MethodImpl(256)]public SimpleList<T>Sort(){Array.Sort(data,0,Count);return this;}[MethodImpl(256)]public SimpleList<T>Sort(IComparer<T>comparer){Array.Sort(data,0,Count,comparer);return this;}[MethodImpl(256)]public SimpleList<T>Sort(int index,int count,IComparer<T>comparer){Array.Sort(data,index,count,comparer);return this;}[MethodImpl(256)]public void Clear()=>Count=0;[MethodImpl(256)]public bool Contains(T item)=>IndexOf(item)>=0;[MethodImpl(256)]public int IndexOf(T item)=>Array.IndexOf(data,item,0,Count);[MethodImpl(256)]public void CopyTo(T[]array,int arrayIndex)=>Array.Copy(data,0,array,arrayIndex,Count);[MethodImpl(256)]public T[]ToArray()=>AsSpan().ToArray();bool ICollection<T>.IsReadOnly=>false;T IList<T>.this[int index]{get=>data[index];set=>data[index]=value;}T IReadOnlyList<T>.this[int index]{get=>data[index];}void IList<T>.Insert(int index,T item)=>throw new NotSupportedException();bool ICollection<T>.Remove(T item)=>throw new NotSupportedException();void IList<T>.RemoveAt(int index)=>throw new NotSupportedException();IEnumerator IEnumerable.GetEnumerator()=>((IEnumerable<T>)this).GetEnumerator();IEnumerator<T>IEnumerable<T>.GetEnumerator(){for(int i=0;i<Count;i++)yield return data[i];}[MethodImpl(256)]public Span<T>.Enumerator GetEnumerator()=>AsSpan().GetEnumerator();private static void ThrowIndexOutOfRangeException()=>throw new IndexOutOfRangeException();}}
namespace SampleLibrary { public partial class UnionFind : Dsu { public UnionFind(int n) : base(n) { Foo(); }  void Foo() => Bar(); public bool Try(out string? text) { if (this.Size(0) == 1) { text = "Single"; return true; }  text = null; return false; }  partial void Bar(); } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander

""".ReplaceLineEndings());
    }

    [Fact]
    public async Task ExpandAnotherProject()
    {
        using var sw = new StringWriter();
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        var target = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs");
        await new SourceExpanderCommand { Stdout = sw }.Expand(target, project: project, cancellationToken: TestContext.Current.CancellationToken);
        sw.ToString().ReplaceLineEndings().Should().Be("""
using AtCoder;
using SampleLibrary;
using System;
namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var uf = new UnionFind(3);
            uf.Merge(1, 2);
            Console.WriteLine(uf.Leader(2));
        }
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace SampleLibrary { public partial class UnionFind : Dsu { public UnionFind(int n) : base(n) { Foo(); }  void Foo() => Bar(); public bool Try(out string? text) { if (this.Size(0) == 1) { text = "Single"; return true; }  text = null; return false; }  partial void Bar(); } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander

""".ReplaceLineEndings());
    }

    [Fact]
    public async Task ExpandWithStaticEmbedding()
    {
        using var sw = new StringWriter();
        var target = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs");
        await new SourceExpanderCommand { Stdout = sw }.Expand(target, staticEmbedding: "/* Wow! 🦖 */", cancellationToken: TestContext.Current.CancellationToken);
        sw.ToString().ReplaceLineEndings().Should().Be("""
using AtCoder;
using AtCoder.Internal;
using SampleLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var uf = new UnionFind(3);
            uf.Merge(1, 2);
            Console.WriteLine(uf.Leader(2));
        }
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
/* Wow! 🦖 */
namespace AtCoder{public class Dsu{public readonly int[]_ps;public Dsu(int n){_ps=new int[n];_ps.AsSpan().Fill(-1);}[MethodImpl(256)]public int Merge(int a,int b){int x=Leader(a),y=Leader(b);if(x==y)return x;if(-_ps[x]<-_ps[y])(x,y)=(y,x);_ps[x]+=_ps[y];_ps[y]=x;return x;}[MethodImpl(256)]public bool Same(int a,int b){return Leader(a)==Leader(b);}[MethodImpl(256)]public int Leader(int a){if(_ps[a]<0)return a;while(0<=_ps[_ps[a]]){(a,_ps[a])=(_ps[a],_ps[_ps[a]]);}return _ps[a];}[MethodImpl(256)]public int Size(int a){return-_ps[Leader(a)];}[MethodImpl(256)]public int[][]Groups(){int n=_ps.Length;int[]leaderBuf=new int[n];int[]id=new int[n];var resultList=new SimpleList<int[]>(n);for(int i=0;i<leaderBuf.Length;i++){leaderBuf[i]=Leader(i);if(i==leaderBuf[i]){id[i]=resultList.Count;resultList.Add(new int[-_ps[i]]);}}var result=resultList.ToArray();int[]ind=new int[result.Length];for(int i=0;i<leaderBuf.Length;i++){var leaderID=id[leaderBuf[i]];result[leaderID][ind[leaderID]]=i;ind[leaderID]++;}return result;}}}
namespace AtCoder.Internal{public class SimpleList<T>:IList<T>,IReadOnlyList<T>{public T[]data;private const int DefaultCapacity=2;public SimpleList(){data=new T[DefaultCapacity];}public SimpleList(int capacity){data=new T[Math.Max(capacity,DefaultCapacity)];}public SimpleList(IEnumerable<T>collection){if(collection is ICollection<T>col){data=new T[col.Count];col.CopyTo(data,0);Count=col.Count;}else{data=new T[DefaultCapacity];foreach(var item in collection)Add(item);}}[MethodImpl(256)]public Memory<T>AsMemory()=>new Memory<T>(data,0,Count);[MethodImpl(256)]public Span<T>AsSpan()=>new Span<T>(data,0,Count);public ref T this[int index]{[MethodImpl(256)]get{if((uint)index>=(uint)Count)ThrowIndexOutOfRangeException();return ref data[index];}}public int Count{get;private set;}[MethodImpl(256)]public void Add(T item){if((uint)Count>=(uint)data.Length)Array.Resize(ref data,data.Length<<1);data[Count++]=item;}[MethodImpl(256)]public void RemoveLast(){if( --Count<0)ThrowIndexOutOfRangeException();}[MethodImpl(256)]public void RemoveLast(int size){if((Count-=size)<0)ThrowIndexOutOfRangeException();}[MethodImpl(256)]public SimpleList<T>Reverse(){Array.Reverse(data,0,Count);return this;}[MethodImpl(256)]public SimpleList<T>Reverse(int index,int count){Array.Reverse(data,index,count);return this;}[MethodImpl(256)]public SimpleList<T>Sort(){Array.Sort(data,0,Count);return this;}[MethodImpl(256)]public SimpleList<T>Sort(IComparer<T>comparer){Array.Sort(data,0,Count,comparer);return this;}[MethodImpl(256)]public SimpleList<T>Sort(int index,int count,IComparer<T>comparer){Array.Sort(data,index,count,comparer);return this;}[MethodImpl(256)]public void Clear()=>Count=0;[MethodImpl(256)]public bool Contains(T item)=>IndexOf(item)>=0;[MethodImpl(256)]public int IndexOf(T item)=>Array.IndexOf(data,item,0,Count);[MethodImpl(256)]public void CopyTo(T[]array,int arrayIndex)=>Array.Copy(data,0,array,arrayIndex,Count);[MethodImpl(256)]public T[]ToArray()=>AsSpan().ToArray();bool ICollection<T>.IsReadOnly=>false;T IList<T>.this[int index]{get=>data[index];set=>data[index]=value;}T IReadOnlyList<T>.this[int index]{get=>data[index];}void IList<T>.Insert(int index,T item)=>throw new NotSupportedException();bool ICollection<T>.Remove(T item)=>throw new NotSupportedException();void IList<T>.RemoveAt(int index)=>throw new NotSupportedException();IEnumerator IEnumerable.GetEnumerator()=>((IEnumerable<T>)this).GetEnumerator();IEnumerator<T>IEnumerable<T>.GetEnumerator(){for(int i=0;i<Count;i++)yield return data[i];}[MethodImpl(256)]public Span<T>.Enumerator GetEnumerator()=>AsSpan().GetEnumerator();private static void ThrowIndexOutOfRangeException()=>throw new IndexOutOfRangeException();}}
namespace SampleLibrary { public partial class UnionFind : Dsu { public UnionFind(int n) : base(n) { Foo(); }  void Foo() => Bar(); public bool Try(out string? text) { if (this.Size(0) == 1) { text = "Single"; return true; }  text = null; return false; }  partial void Bar(); } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander

""".ReplaceLineEndings());
    }

    [Fact]
    public async Task ExpandToFile()
    {
        var output = Path.Combine(Path.GetTempPath(), "SourceExpander.Console.Test.ExpandToFile.csx");
        var project = Path.Combine(TestUtil.TestProjectDirectory, "tools", "SampleAppSkipAtcoder.csproj");
        var target = Path.Combine(TestUtil.SourceDirectory, "Sandbox", "SampleApp", "Program.cs");
        await new SourceExpanderCommand().Expand(target, output: output, project: project, cancellationToken: TestContext.Current.CancellationToken);
        (await File.ReadAllTextAsync(output, TestContext.Current.CancellationToken)).ReplaceLineEndings().Should().Be("""
using AtCoder;
using SampleLibrary;
using System;
namespace SampleApp
{
    class Program
    {
        static void Main()
        {
            var uf = new UnionFind(3);
            uf.Merge(1, 2);
            Console.WriteLine(uf.Leader(2));
        }
    }
}
#region Expanded by https://github.com/kzrnm/SourceExpander
namespace SampleLibrary { public partial class UnionFind : Dsu { public UnionFind(int n) : base(n) { Foo(); }  void Foo() => Bar(); public bool Try(out string? text) { if (this.Size(0) == 1) { text = "Single"; return true; }  text = null; return false; }  partial void Bar(); } }
#endregion Expanded by https://github.com/kzrnm/SourceExpander

""".ReplaceLineEndings());
    }
}
