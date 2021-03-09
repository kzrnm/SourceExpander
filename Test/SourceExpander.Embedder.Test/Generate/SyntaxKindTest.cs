using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace SourceExpander.Embedder.Generate.Test
{
    public class SyntaxKindTest : EmbeddingGeneratorTestBase
    {
        public static readonly TestData[] TestTable = new TestData[]
        {
            new TestData("Class.Field", @"
using System;
class Def
{
    public object obj = null;
    internal static DateTime date = DateTime.Now;
}",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "class Def { public object obj = null; internal static DateTime date = DateTime.Now; }",
                "class Def{public object obj=null;internal static DateTime date=DateTime.Now;}"
            ),
            new TestData("Struct.Field", @"
using System;
struct Def
{
    internal static DateTime date = DateTime.Now;
}",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "struct Def { internal static DateTime date = DateTime.Now; }",
                "struct Def{internal static DateTime date=DateTime.Now;}"
            ),
            new TestData("Record.Field", @"
using System;
record Def(string Foo)
{
    public object obj = null;
    internal static DateTime date = DateTime.Now;
}",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "record Def(string Foo) { public object obj = null; internal static DateTime date = DateTime.Now; }",
                "record Def(string Foo){public object obj=null;internal static DateTime date=DateTime.Now;}"
            ),
            new TestData("Interface", @"
using System;
public interface IDef { }
",
                ImmutableArray.Create("IDef"),
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                "public interface IDef { }",
                @"public interface IDef{}"
            ),
            new TestData("Delegate", @"
using System;
public delegate UIntPtr Def1();
public delegate void Def2(uint n);
",
                ImmutableArray.Create("Def1", "Def2"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public delegate UIntPtr Def1(); public delegate void Def2(uint n);",
                "public delegate UIntPtr Def1();public delegate void Def2(uint n);"
            ),
            new TestData("Enum", @"
using System;
enum Def
{
    A, B, C, D, E, F
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                "enum Def { A, B, C, D, E, F }",
                "enum Def{A,B,C,D,E,F}"
            ),
            new TestData("Namespace", @"
namespace Foo
{
    using System;
    class Def
    {
        public object obj = null;
        internal static DateTime date = DateTime.Now;
    }
}
",
                ImmutableArray.Create("Foo.Def"),
                ImmutableArray<string>.Empty,
                ImmutableArray<string>.Empty,
                "namespace Foo { using System;  class Def { public object obj = null; internal static DateTime date = DateTime.Now; } }",
                "namespace Foo{using System;class Def{public object obj=null;internal static DateTime date=DateTime.Now;}}"
            ),
            new TestData("Property", @"
using System;
class Def
{
    public int Prop1 { get; init; }
    public System.Random Prop2 { set; get; }
    private IntPtr _Prop3;
    protected IntPtr Prop3
    {
        set
        {
            _Prop3 = value;
        }
        get => _Prop3;
    }
    public ulong Prop4 { get; } = 0;
}",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "class Def { public int Prop1 { get; init; }  public System.Random Prop2 { set; get; }  private IntPtr _Prop3; protected IntPtr Prop3 { set { _Prop3 = value; }  get => _Prop3; }  public ulong Prop4 { get; } = 0; }",
                "class Def{public int Prop1{get;init;}public System.Random Prop2{set;get;}private IntPtr _Prop3;protected IntPtr Prop3{set{_Prop3=value;}get=>_Prop3;}public ulong Prop4{get;}=0;}"
            ),
            new TestData("Event", @"
using System;
class Def
{
    internal static event EventHandler Event1;
    internal static event EventHandler Event2
    {
        add { Event1 += value; }
        remove=> Event1 -= value;
    }
    public void Run()
    {
        static void EventHandler(object sender, EventArgs e) { }
        Event2 += EventHandler;
        Event2 -= EventHandler;
        Event1(null, null);
    }
}",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "class Def { internal static event EventHandler Event1; internal static event EventHandler Event2 { add { Event1 += value; }  remove => Event1 -= value; }  public void Run() { static void EventHandler(object sender, EventArgs e) { }  Event2 += EventHandler; Event2 -= EventHandler; Event1(null, null); } }",
                "class Def{internal static event EventHandler Event1;internal static event EventHandler Event2{add{Event1+=value;}remove=>Event1-=value;}public void Run(){static void EventHandler(object sender,EventArgs e){}Event2+=EventHandler;Event2-=EventHandler;Event1(null,null);}}"
            ),
            new TestData("Constructor", @"
using System;
public class Par
{
    public Par() : this(0) { }
    public Par(long n)
    {
        this.n = n;
    }
    public long n;
}
public class Def : Par
{
    public Def(DateTime d, int m) : base(d.Ticks * m) { }
}
",
                ImmutableArray.Create("Par", "Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Par { public Par(): this(0) { }  public Par(long n) { this.n = n; }  public long n; }  public class Def : Par { public Def(DateTime d, int m): base(d.Ticks * m) { } }",
                "public class Par{public Par():this(0){}public Par(long n){this.n=n;}public long n;}public class Def:Par{public Def(DateTime d,int m):base(d.Ticks*m){}}"
            ),
            new TestData("Conversion", @"
using System;
public class Def
{
    public static implicit operator Int32(Def d) => 1;
    public static explicit operator long(Def d) => 1;
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { public static implicit operator Int32(Def d) => 1; public static explicit operator long (Def d) => 1; }",
                "public class Def{public static implicit operator Int32(Def d)=>1;public static explicit operator long(Def d)=>1;}"
            ),
            new TestData("Method", @"
using System;
public partial class Def : ICloneable
{
    public object obj = null;
    public static uint C() => 0;
    partial void M();
    public partial Int32 M(int n, int m);
    public partial int M(int n, int m)
    {
        M();
        return m + n + (int)obj;
    }
    private void Ref(ref int a, in bool b, out uint c)
    {
        c = 0;
    }
    public void Call(string name = null)
    {
        name.L();
        var a = M(n: 1, m: 2);
        Ref(ref a, true, out var c);
    }
    object ICloneable.Clone() => throw new NotImplementedException();
}
internal static class Ext
{
    public static int L(this string s) => s.Length;
}
",
                ImmutableArray.Create("Def", "Ext"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public partial class Def : ICloneable { public object obj = null; public static uint C() => 0; partial void M(); public partial Int32 M(int n, int m); public partial int M(int n, int m) { M(); return m + n + (int)obj; }  private void Ref(ref int a, in bool b, out uint c) { c = 0; }  public void Call(string name = null) { name.L(); var a = M(n: 1, m: 2); Ref(ref a, true, out var c); }  object ICloneable.Clone() => throw new NotImplementedException(); }  internal static class Ext { public static int L(this string s) => s.Length; }",
                "public partial class Def:ICloneable{public object obj=null;public static uint C()=>0;partial void M();public partial Int32 M(int n,int m);public partial int M(int n,int m){M();return m+n+(int)obj;}private void Ref(ref int a,in bool b,out uint c){c=0;}public void Call(string name=null){name.L();var a=M(n:1,m:2);Ref(ref a,true,out var c);}object ICloneable.Clone()=>throw new NotImplementedException();}internal static class Ext{public static int L(this string s)=>s.Length;}"
            ),
            new TestData("Indexer", @"
using System;
public partial class Def
{
    private int[] _arr = new int[32];
    public int this[Int16 index]
    {
        set => _arr[index] = value;
        get => _arr[index] ;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public partial class Def { private int[] _arr = new int[32]; public int this[Int16 index] { set => _arr[index] = value; get => _arr[index]; } }",
                "public partial class Def{private int[]_arr=new int[32];public int this[Int16 index]{set=>_arr[index]=value;get=>_arr[index];}}"
            ),
            new TestData("Attributes", @"
using System.Diagnostics;
[DebuggerDisplay(""Prop"")]
public class Def
{
    [DebuggerHidden, DebuggerNonUserCode]
    public Def() { }
    [Conditional(""DEBUG""), DebuggerHidden, DebuggerNonUserCode]
    public void M() { }
    [DebuggerHidden, DebuggerNonUserCode]
    public string Prop { set; [return: System.Diagnostics.CodeAnalysis.NotNull]get; }
    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public string field;
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System.Diagnostics;"),
                ImmutableArray<string>.Empty,
                @"[DebuggerDisplay(""Prop"")] public class Def { [DebuggerHidden, DebuggerNonUserCode] public Def() { }  [Conditional(""DEBUG""), DebuggerHidden, DebuggerNonUserCode] public void M() { }  [DebuggerHidden, DebuggerNonUserCode] public string Prop { set; [return: System.Diagnostics.CodeAnalysis.NotNull] get; }  [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)] public string field; }",
                @"[DebuggerDisplay(""Prop"")]public class Def{[DebuggerHidden,DebuggerNonUserCode]public Def(){}[Conditional(""DEBUG""),DebuggerHidden,DebuggerNonUserCode]public void M(){}[DebuggerHidden,DebuggerNonUserCode]public string Prop{set;[return:System.Diagnostics.CodeAnalysis.NotNull]get;}[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]public string field;}"
            ),
            new TestData("Array", @"
using System;
public static class Def
{
    public static object[] Arr = new object[3];
    public static uint[] ArrInit = new uint[] { 1, 2, 3 };
    public static int[] ArrInitImplicit = new[] { 1, 2, 3 };
    public static DateTime[,] Arr2 = new DateTime[3, 6];
    public static byte[,] Arr2Def = new byte[,]{
        {1 ,2 },
        {3, 4, }
    };
    public static bool[][] ArrJugged = new bool[2][];
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public static class Def { public static object[] Arr = new object[3]; public static uint[] ArrInit = new uint[]{1, 2, 3}; public static int[] ArrInitImplicit = new[]{1, 2, 3}; public static DateTime[, ] Arr2 = new DateTime[3, 6]; public static byte[, ] Arr2Def = new byte[, ]{{1, 2}, {3, 4, }}; public static bool[][] ArrJugged = new bool[2][]; }",
                "public static class Def{public static object[]Arr=new object[3];public static uint[]ArrInit=new uint[]{1,2,3};public static int[]ArrInitImplicit=new[]{1,2,3};public static DateTime[,]Arr2=new DateTime[3,6];public static byte[,]Arr2Def=new byte[,]{{1,2},{3,4,}};public static bool[][]ArrJugged=new bool[2][];}"
            ),
            new TestData("Anonymous type", @"
using System;
public class Def
{
    public void M()
    {
        var anonymous = new { Foo = 1, Bar = DateTime.Now };
        var anonymous2 = new { Foo = 1, anonymous };
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { public void M() { var anonymous = new { Foo = 1, Bar = DateTime.Now }  ; var anonymous2 = new { Foo = 1, anonymous }  ; } }",
                "public class Def{public void M(){var anonymous=new{Foo=1,Bar=DateTime.Now};var anonymous2=new{Foo=1,anonymous};}}"
            ),
            new TestData("Anonymous method", @"
using System.Linq;
public class Def
{
    int[] Arr = new[] { 1, 2, 3 };
    public int M()
        => Arr.Single(delegate (int i) { return i < 2; });
}

",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System.Linq;"),
                ImmutableArray<string>.Empty,
                "public class Def { int[] Arr = new[]{1, 2, 3}; public int M() => Arr.Single(delegate (int i) { return i < 2; }); }",
                "public class Def{int[]Arr=new[]{1,2,3};public int M()=>Arr.Single(delegate(int i){return i<2;});}"
            ),
            new TestData("Lambda", @"
using System.Linq;
public class Def
{
    int[] Arr = new[] { 1, 2, 3 };
    public int M()
        => Arr.First(n => n < 2)
        + Arr.First((int n) => n < 2)
        + Arr.Select((n, index) => n * index).First()
        + Arr.Select((int n, int index) => n * index).First();
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System.Linq;"),
                ImmutableArray<string>.Empty,
                "public class Def { int[] Arr = new[]{1, 2, 3}; public int M() => Arr.First(n => n < 2) + Arr.First((int n) => n < 2) + Arr.Select((n, index) => n * index).First() + Arr.Select((int n, int index) => n * index).First(); }",
                "public class Def{int[]Arr=new[]{1,2,3};public int M()=>Arr.First(n=>n<2)+Arr.First((int n)=>n<2)+Arr.Select((n,index)=>n*index).First()+Arr.Select((int n,int index)=>n*index).First();}"
            ),
            new TestData("async/await", @"
using System;
public class Def
{
    Int32[] Arr = new[] { 1, 2, 3 };
    public async void M()
    {
        await System.Threading.Tasks.Task.Delay(3);
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Int32[] Arr = new[]{1, 2, 3}; public async void M() { await System.Threading.Tasks.Task.Delay(3); } }",
                "public class Def{Int32[]Arr=new[]{1,2,3};public async void M(){await System.Threading.Tasks.Task.Delay(3);}}"
            ),
            new TestData("Patterns", @"
using System;
public class Def
{
    Int32[] Arr = new[] { 1, 2, 3 };
    public void M()
    {
        _ = ((object)DateTime.Now) switch
        {
            1 => 1,
            int i => i,
            IComparable c and long l => l,
            DateTime d and { Ticks: 131 } => d.Ticks,
            _ => 1,
        };
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Int32[] Arr = new[]{1, 2, 3}; public void M() { _ = ((object)DateTime.Now)switch { 1 => 1, int i => i, IComparable c and long l => l, DateTime d and { Ticks: 131 }  => d.Ticks, _ => 1, }  ; } }",
                "public class Def{Int32[]Arr=new[]{1,2,3};public void M(){_=((object)DateTime.Now)switch{1=>1,int i=>i,IComparable c and long l=>l,DateTime d and{Ticks:131}=>d.Ticks,_=>1,};}}"
            ),
            new TestData("UnaryExpression", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        _ = +1;
        _ = -3;
        _ = ~4;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { _ = +1; _ = -3; _ = ~4; } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){_=+1;_=-3;_=~4;}}"
            ),
            new TestData("BinaryExpression", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        _ = 1 + 2;
        _ = 1 - 2;
        _ = 1 * 2;
        _ = 1 / 2;
        _ = 1 % 2;
        _ = 1 << 2;
        _ = 1 >> 2;
        _ = 1 & 2;
        _ = true && false;
        _ = 1 | 2;
        _ = true || false;
        _ = 1 ^ 2;
        _ = 1 == 2;
        _ = 1 != 2;
        _ = 1 < 2;
        _ = 1 <= 2;
        _ = 1 > 2;
        _ = 1 >= 2;
        _ = Arr[0]  is int;
        _ = Arr[0] as string;
        _ = Arr[0] ?? 41;
    }
    public static Def operator +(Def a, Def b) => null;
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { _ = 1 + 2; _ = 1 - 2; _ = 1 * 2; _ = 1 / 2; _ = 1 % 2; _ = 1 << 2; _ = 1 >> 2; _ = 1 & 2; _ = true && false; _ = 1 | 2; _ = true || false; _ = 1 ^ 2; _ = 1 == 2; _ = 1 != 2; _ = 1 < 2; _ = 1 <= 2; _ = 1 > 2; _ = 1 >= 2; _ = Arr[0] is int; _ = Arr[0] as string; _ = Arr[0] ?? 41; }  public static Def operator +(Def a, Def b) => null; }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){_=1+2;_=1-2;_=1*2;_=1/2;_=1%2;_=1<<2;_=1>>2;_=1&2;_=true&&false;_=1|2;_=true||false;_=1^2;_=1==2;_=1!=2;_=1<2;_=1<=2;_=1>2;_=1>=2;_=Arr[0]is int;_=Arr[0]as string;_=Arr[0]??41;}public static Def operator+(Def a,Def b)=>null;}"
            ),
            new TestData("Conditional", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        _ = true ? 1 : 2;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { _ = true ? 1 : 2; } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){_=true?1:2;}}"
            ),
            new TestData("try/catch/finally", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        try
        {
            Console.Write(10);
        }
        catch (OverflowException) { throw; }
        catch (AggregateException e) when (e.InnerException is not null)
        {
            throw new Exception(e.Message, e);
        }
        catch
        {

        }
        finally
        {
            Console.Write(1);
        }
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { try { Console.Write(10); } catch (OverflowException) { throw; } catch (AggregateException e)when (e.InnerException is not null) { throw new Exception(e.Message, e); } catch { } finally { Console.Write(1); } } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){try{Console.Write(10);}catch(OverflowException){throw;}catch(AggregateException e)when(e.InnerException is not null){throw new Exception(e.Message,e);}catch{}finally{Console.Write(1);}}}"
            ),
            new TestData("checked/unchecked", @"
using System;
public class Def
{
    Int32[] Arr = new int[] { 1, 2, 3 };
    public void M()
    {
        _ = checked((short)(Arr[0] << 20));
        _ = unchecked((short)(Arr[0] << 20));
        checked
        {
            _ = (byte)(Arr[0] << 10);
        }
        unchecked
        {
            _ = (byte)(Arr[0] << 10);
        }
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Int32[] Arr = new int[]{1, 2, 3}; public void M() { _ = checked((short)(Arr[0] << 20)); _ = unchecked((short)(Arr[0] << 20)); checked { _ = (byte)(Arr[0] << 10); }  unchecked { _ = (byte)(Arr[0] << 10); } } }",
                "public class Def{Int32[]Arr=new int[]{1,2,3};public void M(){_=checked((short)(Arr[0]<<20));_=unchecked((short)(Arr[0]<<20));checked{_=(byte)(Arr[0]<<10);}unchecked{_=(byte)(Arr[0]<<10);}}}"
            ),
            new TestData("Default", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public long M()
    {
        int a = default;
        var b = default(byte);
        return a + b;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public long M() { int a = default; var b = default(byte); return a + b; } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public long M(){int a=default;var b=default(byte);return a+b;}}"
            ),
            new TestData("DocumentationComment", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    /// <summary>
    /// <paramref name=""i""/> to <see cref=""uint""/>
    /// </summary>
    /// <param name=""i"">uint</param>
    /// <returns>int</returns>
    public int M(uint i)
    {
        return (int)i;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public int M(uint i) { return (int)i; } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public int M(uint i){return(int)i;}}"
            ),
            new TestData("Processor", @"
#define EXPAND
#define EXP
#undef EXP
using System;
public class Def
{
    #region Start
#pragma warning disable CA1050, CA1822, IDE0044, IDE0049, IDE0052

    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
#if EXPAND
        Console.Write(0);
#elif EXP
        Console.Write(1);
#else
        Console.Write(2);
#endif
        Console.Write(true);
#if !EXPAND
        Console.Write(0U);
#elif EXP
        Console.Write(1U);
#else
        Console.Write(2U);
#endif
    }
#pragma warning restore CA1050, CA1822, IDE0044, IDE0049, IDE0052
#line 10
    #endregion End
}

",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { Console.Write(0); Console.Write(true); Console.Write(2U); } }",
                @"public class Def{Object[]Arr=new object[]{1,2,3};public void M(){Console.Write(0);Console.Write(true);Console.Write(2U);}}"
            ),
            new TestData("Pointer", @"
using System;
public class Def
{
    public unsafe object M()
    {
        long x = 0;
        var p = &x;
        var pi = (Int32*)p;
        *pi = int.MaxValue;
        var y = x * *pi;
        p->CompareTo(2);
        return pi[1];
    }
    public object M2(string str)
    {
        unsafe
        {
            fixed (char* p = str)
            {
                var pi = (Int32*)p;
                *pi = sizeof(DateTime);
                return pi[1];
            }
        }
    } 
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { public unsafe object M() { long x = 0; var p = &x; var pi = (Int32*)p; *pi = int.MaxValue; var y = x * *pi; p->CompareTo(2); return pi[1]; }  public object M2(string str) { unsafe { fixed (char *p = str) { var pi = (Int32*)p; *pi = sizeof(DateTime); return pi[1]; } } } }",
                "public class Def{public unsafe object M(){long x=0;var p=&x;var pi=(Int32*)p;*pi=int.MaxValue;var y=x* *pi;p->CompareTo(2);return pi[1];}public object M2(string str){unsafe{fixed(char*p=str){var pi=(Int32*)p;*pi=sizeof(DateTime);return pi[1];}}}}"
            ),
            new TestData("For", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        for (int i = 0; i < Arr.Length; i++)
        {
            Console.Write(Arr[i]);
        }
        for (int i = 0; i < Arr.Length; i++)
            Console.WriteLine(Arr[i]);
    }
}

",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { for (int i = 0; i < Arr.Length; i++) { Console.Write(Arr[i]); }  for (int i = 0; i < Arr.Length; i++) Console.WriteLine(Arr[i]); } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){for(int i=0;i<Arr.Length;i++){Console.Write(Arr[i]);}for(int i=0;i<Arr.Length;i++)Console.WriteLine(Arr[i]);}}"
            ),
            new TestData("ForEach", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public System.Collections.IEnumerable M()
    {
        foreach (var a in Arr)
        {
            yield return a;
        }
        foreach (var a in Arr)
            yield return a;
        yield break;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public System.Collections.IEnumerable M() { foreach (var a in Arr) { yield return a; }  foreach (var a in Arr) yield return a; yield break; } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public System.Collections.IEnumerable M(){foreach(var a in Arr){yield return a;}foreach(var a in Arr)yield return a;yield break;}}"
            ),
            new TestData("While", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public int M()
    {
        int a = 0;
        while (a < 10) a++;
        while (a < 20)
        {
            a++;
        }
        do
        {
            a++;
        } while (a < 0);
        return a;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public int M() { int a = 0; while (a < 10) a++; while (a < 20) { a++; }  do { a++; } while (a < 0); return a; } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public int M(){int a=0;while(a<10)a++;while(a<20){a++;}do{a++;}while(a<0);return a;}}"
            ),
            new TestData("Switch", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public int M()
    {
        switch ((int)Arr[0])
        {
            case 1:
                return 1;
            case 2: return 2;
            case 3:
            case 4: return 4;
            case 5:
                goto case 2;
            default:
                break;
        }
        return 0;
    }
}

",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public int M() { switch ((int)Arr[0]) { case 1: return 1; case 2: return 2; case 3: case 4: return 4; case 5: goto case 2; default: break; }  return 0; } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public int M(){switch((int)Arr[0]){case 1:return 1;case 2:return 2;case 3:case 4:return 4;case 5:goto case 2;default:break;}return 0;}}"
            ),
            new TestData("Function Pointers", @"
using System;
public class Def
{
    public unsafe void M(delegate*<int, void> f)
    {
        f(10);
    }
    private static void F(int i) => Console.Write(i);
    public unsafe void M2() => M(&F);
}

",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { public unsafe void M(delegate *<int, void>f) { f(10); }  private static void F(int i) => Console.Write(i); public unsafe void M2() => M(&F); }",
                "public class Def{public unsafe void M(delegate*<int,void>f){f(10);}private static void F(int i)=>Console.Write(i);public unsafe void M2()=>M(&F);}"
            ),
            new TestData("Goto", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        if (true)
            goto Label;
        Label: { }
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { if (true) goto Label; Label: { } } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){if(true)goto Label;Label:{}}}"
            ),
            new TestData("Generics", @"
using System;
public class Def<T, Q> where T : ICloneable
{
    public void M<R, S, C, U, E, D>()
        where R : class, ICloneable
        where S : struct
        where C : notnull, System.Collections.Generic.Comparer<int>, new()
        where U : unmanaged
        where E : struct, Enum
        where D : Delegate
    {
        Console.WriteLine(typeof(Def<,>));
        Console.WriteLine(typeof(Def<int[], long>));
    }
}
",
                ImmutableArray.Create("Def<T, Q>"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def<T, Q> where T : ICloneable { public void M<R, S, C, U, E, D>() where R : class, ICloneable where S : struct where C : notnull, System.Collections.Generic.Comparer<int>, new() where U : unmanaged where E : struct, Enum where D : Delegate { Console.WriteLine(typeof(Def<, >)); Console.WriteLine(typeof(Def<int[], long>)); } }",
                "public class Def<T,Q>where T:ICloneable{public void M<R,S,C,U,E,D>()where R:class,ICloneable where S:struct where C:notnull,System.Collections.Generic.Comparer<int>,new()where U:unmanaged where E:struct,Enum where D:Delegate{Console.WriteLine(typeof(Def<,>));Console.WriteLine(typeof(Def<int[],long>));}}"
            ),
            new TestData("String", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M(int i)
    {
        Console.WriteLine(""i"");
        Console.WriteLine($""{i}"");
        Console.WriteLine($""{i:0000}"");
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                @"public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M(int i) { Console.WriteLine(""i""); Console.WriteLine($""{i}""); Console.WriteLine($""{i:0000}""); } }",
                @"public class Def{Object[]Arr=new object[]{1,2,3};public void M(int i){Console.WriteLine(""i"");Console.WriteLine($""{i}"");Console.WriteLine($""{i:0000}"");}}"
            ),
            new TestData("Range", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        Console.WriteLine(Arr[1..]);
        Console.WriteLine(Arr[..1]);
        Console.WriteLine(Arr[^1..]);
        Console.WriteLine(Arr[..^1]);
        Console.WriteLine(Arr[^1]);
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { Console.WriteLine(Arr[1..]); Console.WriteLine(Arr[..1]); Console.WriteLine(Arr[^1..]); Console.WriteLine(Arr[..^1]); Console.WriteLine(Arr[^1]); } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){Console.WriteLine(Arr[1..]);Console.WriteLine(Arr[..1]);Console.WriteLine(Arr[^1..]);Console.WriteLine(Arr[..^1]);Console.WriteLine(Arr[^1]);}}"
            ),
            new TestData("Tuple", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        (int a, byte b) t = (1, 2);
        var (a, b) = t;
        Console.WriteLine(t);
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { (int a, byte b) t = (1, 2); var(a, b) = t; Console.WriteLine(t); } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){(int a,byte b)t=(1,2);var(a,b)=t;Console.WriteLine(t);}}"
            ),
            new TestData("Using", @"
using System.IO;
public class Def
{
    byte[] Arr = new byte[] { 1, 2, 3 };
    public void M()
    {
        using var ms1 = new MemoryStream(Arr);
        using (var ms2 = new MemoryStream())
            ms2.CopyTo(ms1);
        using (var ms3 = new MemoryStream())
        {
            ms3.CopyTo(ms1);
        }
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System.IO;"),
                ImmutableArray<string>.Empty,
                "public class Def { byte[] Arr = new byte[]{1, 2, 3}; public void M() { using var ms1 = new MemoryStream(Arr); using (var ms2 = new MemoryStream()) ms2.CopyTo(ms1); using (var ms3 = new MemoryStream()) { ms3.CopyTo(ms1); } } }",
                "public class Def{byte[]Arr=new byte[]{1,2,3};public void M(){using var ms1=new MemoryStream(Arr);using(var ms2=new MemoryStream())ms2.CopyTo(ms1);using(var ms3=new MemoryStream()){ms3.CopyTo(ms1);}}}"
            ),
            new TestData("If", @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M(int n)
    {
        if (n < 0)
            Console.Write(1);
        else if (n < 10)
            Console.Write(10);
        else
            Console.Write(100);
        if (n < 0)
        {
            Console.WriteLine(1);
        }
        else if (n < 10)
        {
            Console.WriteLine(10);
        }
        else
        {
            Console.WriteLine(100);
        }
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M(int n) { if (n < 0) Console.Write(1); else if (n < 10) Console.Write(10); else Console.Write(100); if (n < 0) { Console.WriteLine(1); } else if (n < 10) { Console.WriteLine(10); } else { Console.WriteLine(100); } } }",
                "public class Def{Object[]Arr=new object[]{1,2,3};public void M(int n){if(n<0)Console.Write(1);else if(n<10)Console.Write(10);else Console.Write(100);if(n<0){Console.WriteLine(1);}else if(n<10){Console.WriteLine(10);}else{Console.WriteLine(100);}}}"
            ),
            new TestData("LINQ", @"
using System.Linq;
public class Def
{
    public object M()
    {
        var obj = from d in Enumerable.Repeat(System.DateTime.Now, 5) where d.DayOfYear < 200 orderby d.Year select d.DayOfWeek;
        return obj.First();
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System.Linq;"),
                ImmutableArray<string>.Empty,
                "public class Def { public object M() { var obj = from d in Enumerable.Repeat(System.DateTime.Now, 5) where d.DayOfYear < 200 orderby d.Year select d.DayOfWeek; return obj.First(); } }",
                "public class Def{public object M(){var obj=from d in Enumerable.Repeat(System.DateTime.Now,5)where d.DayOfYear<200 orderby d.Year select d.DayOfWeek;return obj.First();}}"
            ),
            new TestData("Increment", @"
using System;
public class Def
{
    public void M(int n)
    {
        Int32 a = 0;
        int b = 2 + ++a + 1;
        var c = a++ + ++b + ~-1;
        c = a++ + b;
        c = a + ++b;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { public void M(int n) { Int32 a = 0; int b = 2 + ++a + 1; var c = a++ + ++b + ~-1; c = a++ + b; c = a + ++b; } }",
                "public class Def{public void M(int n){Int32 a=0;int b=2+ ++a+1;var c=a++ + ++b+~-1;c=a++ +b;c=a+ ++b;}}"
            ),
            new TestData("Decrement", @"
using System;
public class Def
{
    public void M(int n)
    {
        Int32 a = 0;
        int b = 2 - --a - 1;
        var c = a-- - --b - ~1;
        c = a-- - b;
        c = a - --b;
    }
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { public void M(int n) { Int32 a = 0; int b = 2 - --a - 1; var c = a-- - --b - ~1; c = a-- - b; c = a - --b; } }",
                "public class Def{public void M(int n){Int32 a=0;int b=2- --a-1;var c=a-- - --b-~1;c=a-- -b;c=a- --b;}}"
            ),
            new TestData("Generic and Lambda", @"
using System;
public class Def
{
    public int M<T>(T n) where T : IComparable<T> => n.CompareTo(default);
}
",
                ImmutableArray.Create("Def"),
                ImmutableArray.Create("using System;"),
                ImmutableArray<string>.Empty,
                "public class Def { public int M<T>(T n) where T : IComparable<T> => n.CompareTo(default); }",
                "public class Def{public int M<T>(T n)where T:IComparable<T> =>n.CompareTo(default);}"
            ),
        };

        private static readonly CSharpParseOptions parseOptions
            = new CSharpParseOptions(kind: SourceCodeKind.Regular, documentationMode: DocumentationMode.Parse)
            .WithLanguageVersion(LanguageVersion.CSharp9);

        private static readonly CSharpCompilationOptions compilationOptions
            = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                    .WithAllowUnsafe(true)
                    .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> {
                        {"CS8019",ReportDiagnostic.Suppress },
                    });

        public static readonly IEnumerable<object[]> TestTableArgs = TestTable.Select(d => new object[] { d });

        public static readonly Dictionary<string, TestData> TestTableDic = TestTable.ToDictionary(d => d.Name);
        public static readonly IEnumerable<object[]> TestTableNameArgs = TestTableDic.Keys.Select(n => new object[] { n });

        [Theory]
        [MemberData(nameof(TestTableNameArgs))]
        public void Generate(string testName)
        {
            var testData = TestTableDic[testName];
            var compilation = CreateCompilation(new[] { testData.SyntaxTree }, compilationOptions, new[] { expanderCoreReference });
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator, parseOptions: parseOptions);

            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();

            var reporter = new MockDiagnosticReporter();
            var resolved = new EmbeddingResolver(compilation, parseOptions, reporter, new EmbedderConfig()).ResolveFiles()
                .Should()
                .ContainSingle()
                .Which;
            CreateCompilation(
                new[] { CSharpSyntaxTree.ParseText(resolved.Restore(), parseOptions, "/foo/path.cs", new UTF8Encoding(false)) },
                compilationOptions,
                new[] { expanderCoreReference })
                .GetDiagnostics().Should().BeEmpty();
            resolved
                .Should()
                .BeEquivalentTo(testData.Expected);
            reporter.Diagnostics.Should().BeEmpty();


            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");

            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(testData.Expected);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(testData.Expected);

            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(3);
            gen.Diagnostics.Should().BeEmpty();

            gen.AddedSyntaxTrees
                .Should()
                .ContainSingle(t => t.FilePath.EndsWith("EmbeddedSourceCode.Metadata.cs"))
                .Which
                .ToString()
                .Should()
                .ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",");
        }

        [Theory]
        [MemberData(nameof(TestTableNameArgs))]
        public void Minify(string testName)
        {
            var testData = TestTableDic[testName];
            var compilation = CreateCompilation(new[] { testData.SyntaxTree }, compilationOptions, new[] { expanderCoreReference });
            compilation.GetDiagnostics().Should().BeEmpty();

            var generator = new EmbedderGenerator();
            var gen = RunGenerator(compilation, generator,
                additionalTexts: new[] { enableMinifyJson }, parseOptions: parseOptions);
            gen.Diagnostics.Should().BeEmpty();
            gen.OutputCompilation.GetDiagnostics().Should().BeEmpty();

            var reporter = new MockDiagnosticReporter();
            var resolved = new EmbeddingResolver(compilation, parseOptions, reporter,
                new EmbedderConfig(enableMinify: true)).ResolveFiles()
                .Should()
                .ContainSingle()
                .Which;
            CreateCompilation(
                new[] { CSharpSyntaxTree.ParseText(resolved.Restore(), parseOptions, "/foo/path.cs", new UTF8Encoding(false)) },
                compilationOptions,
                new[] { expanderCoreReference })
                .GetDiagnostics().Should().BeEmpty();
            resolved
                .Should()
                .BeEquivalentTo(testData.ExpectedMinify);
            reporter.Diagnostics.Should().BeEmpty();


            var metadata = gen.OutputCompilation.Assembly.GetAttributes()
                .Where(x => x.AttributeClass?.Name == nameof(System.Reflection.AssemblyMetadataAttribute))
                .ToDictionary(x => (string)x.ConstructorArguments[0].Value, x => (string)x.ConstructorArguments[1].Value);
            metadata.Should().NotContainKey("SourceExpander.EmbeddedSourceCode");
            metadata.Should().ContainKey("SourceExpander.EmbeddedSourceCode.GZipBase32768");

            var embedded = metadata["SourceExpander.EmbeddedSourceCode.GZipBase32768"];
            Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(testData.ExpectedMinify);
            System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(
                SourceFileInfoUtil.FromGZipBase32768(embedded))
                .Should()
                .ContainSingle()
                .Which
                .Should()
                .BeEquivalentTo(testData.ExpectedMinify);

            gen.OutputCompilation.SyntaxTrees.Should().HaveCount(3);
            gen.Diagnostics.Should().BeEmpty();

            gen.AddedSyntaxTrees
                .Should()
                .ContainSingle(t => t.FilePath.EndsWith("EmbeddedSourceCode.Metadata.cs"))
                .Which
                .ToString()
                .Should()
                .ContainAll(
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedSourceCode.GZipBase32768\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbedderVersion\",",
                "[assembly: AssemblyMetadataAttribute(\"SourceExpander.EmbeddedAllowUnsafe\",");
        }

        public class TestData
        {
            public override string ToString() => Name;
            public string Name { get; }
            public CSharpSyntaxTree SyntaxTree { get; }
            public SourceFileInfo Expected { get; }
            public SourceFileInfo ExpectedMinify { get; }
            public TestData(
                string name,
                string syntax,
                ImmutableArray<string> expectedTypeNames,
                ImmutableArray<string> expectedUsings,
                ImmutableArray<string> expectedDependencies,
                string expectedCodeBody,
                string expectedMinifyCodeBody)
            {
                Name = name;
                SyntaxTree = (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(syntax, parseOptions, "/foo/path.cs", new UTF8Encoding(false));
                Expected = new SourceFileInfo(
                    "TestAssembly>path.cs",
                    expectedTypeNames,
                    expectedUsings,
                    expectedDependencies,
                    expectedCodeBody);
                ExpectedMinify = new SourceFileInfo(
                    "TestAssembly>path.cs",
                    expectedTypeNames,
                    expectedUsings,
                    expectedDependencies,
                    expectedMinifyCodeBody);
            }
        }
    }
}
