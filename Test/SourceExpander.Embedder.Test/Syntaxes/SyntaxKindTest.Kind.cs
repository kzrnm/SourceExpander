using System.Collections.Generic;
using System.Collections.Immutable;

namespace SourceExpander.Embedder.Syntaxes
{
    public class ClassFieldTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
class Def
{
    public object obj = null;
    internal static DateTime date = DateTime.Now;
}";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "class Def { public object obj = null; internal static DateTime date = DateTime.Now; }";
        public override string ExpectedMinifyCodeBody => "class Def{public object obj=null;internal static DateTime date=DateTime.Now;}";
    }

    public class StructFieldTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
struct Def
{
    internal static DateTime date = DateTime.Now;
}";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "struct Def { internal static DateTime date = DateTime.Now; }";
        public override string ExpectedMinifyCodeBody => "struct Def{internal static DateTime date=DateTime.Now;}";
    }

    public class RecordFieldTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
record Def(string Foo)
{
    public object obj = null;
    internal static DateTime date = DateTime.Now;
}";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "record Def(string Foo) { public object obj = null; internal static DateTime date = DateTime.Now; }";
        public override string ExpectedMinifyCodeBody => "record Def(string Foo){public object obj=null;internal static DateTime date=DateTime.Now;}";
    }

    public class InterfaceTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
public interface IDef { }
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("IDef");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create<string>();
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public interface IDef { }";
        public override string ExpectedMinifyCodeBody => "public interface IDef{}";
    }

    public class DelegateTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
public delegate UIntPtr Def1();
public delegate void Def2(uint n);
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def1", "Def2");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public delegate UIntPtr Def1(); public delegate void Def2(uint n);";
        public override string ExpectedMinifyCodeBody => "public delegate UIntPtr Def1();public delegate void Def2(uint n);";
    }

    public class EnumTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
enum Def
{
    A, B, C, D, E, F
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create<string>();
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "enum Def { A, B, C, D, E, F }";
        public override string ExpectedMinifyCodeBody => "enum Def{A,B,C,D,E,F}";
    }

    public class NamespaceTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
namespace Foo
{
    using System;
    class Def
    {
        public object obj = null;
        internal static DateTime date = DateTime.Now;
    }
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Foo.Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create<string>();
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "namespace Foo { using System;  class Def { public object obj = null; internal static DateTime date = DateTime.Now; } }";
        public override string ExpectedMinifyCodeBody => "namespace Foo{using System;class Def{public object obj=null;internal static DateTime date=DateTime.Now;}}";
    }

    public class PropertyTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
}";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "class Def { public int Prop1 { get; init; }  public System.Random Prop2 { set; get; }  private IntPtr _Prop3; protected IntPtr Prop3 { set { _Prop3 = value; }  get => _Prop3; }  public ulong Prop4 { get; } = 0; }";
        public override string ExpectedMinifyCodeBody => "class Def{public int Prop1{get;init;}public System.Random Prop2{set;get;}private IntPtr _Prop3;protected IntPtr Prop3{set{_Prop3=value;}get=>_Prop3;}public ulong Prop4{get;}=0;}";
    }

    public class EventTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
}";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "class Def { internal static event EventHandler Event1; internal static event EventHandler Event2 { add { Event1 += value; }  remove => Event1 -= value; }  public void Run() { static void EventHandler(object sender, EventArgs e) { }  Event2 += EventHandler; Event2 -= EventHandler; Event1(null, null); } }";
        public override string ExpectedMinifyCodeBody => "class Def{internal static event EventHandler Event1;internal static event EventHandler Event2{add{Event1+=value;}remove=>Event1-=value;}public void Run(){static void EventHandler(object sender,EventArgs e){}Event2+=EventHandler;Event2-=EventHandler;Event1(null,null);}}";
    }

    public class ConstructorTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Par", "Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Par { public Par(): this(0) { }  public Par(long n) { this.n = n; }  public long n; }  public class Def : Par { public Def(DateTime d, int m): base(d.Ticks * m) { } }";
        public override string ExpectedMinifyCodeBody => "public class Par{public Par():this(0){}public Par(long n){this.n=n;}public long n;}public class Def:Par{public Def(DateTime d,int m):base(d.Ticks*m){}}";
    }

    public class ConversionTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
public class Def
{
    public static implicit operator Int32(Def d) => 1;
    public static explicit operator long(Def d) => 1;
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { public static implicit operator Int32(Def d) => 1; public static explicit operator long (Def d) => 1; }";
        public override string ExpectedMinifyCodeBody => "public class Def{public static implicit operator Int32(Def d)=>1;public static explicit operator long(Def d)=>1;}";
    }

    public class MethodTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def", "Ext");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public partial class Def : ICloneable { public object obj = null; public static uint C() => 0; partial void M(); public partial Int32 M(int n, int m); public partial int M(int n, int m) { M(); return m + n + (int)obj; }  private void Ref(ref int a, in bool b, out uint c) { c = 0; }  public void Call(string name = null) { name.L(); var a = M(n: 1, m: 2); Ref(ref a, true, out var c); }  object ICloneable.Clone() => throw new NotImplementedException(); }  internal static class Ext { public static int L(this string s) => s.Length; }";
        public override string ExpectedMinifyCodeBody => "public partial class Def:ICloneable{public object obj=null;public static uint C()=>0;partial void M();public partial Int32 M(int n,int m);public partial int M(int n,int m){M();return m+n+(int)obj;}private void Ref(ref int a,in bool b,out uint c){c=0;}public void Call(string name=null){name.L();var a=M(n:1,m:2);Ref(ref a,true,out var c);}object ICloneable.Clone()=>throw new NotImplementedException();}internal static class Ext{public static int L(this string s)=>s.Length;}";
    }

    public class IndexerTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public partial class Def { private int[] _arr = new int[32]; public int this[Int16 index] { set => _arr[index] = value; get => _arr[index]; } }";
        public override string ExpectedMinifyCodeBody => "public partial class Def{private int[]_arr=new int[32];public int this[Int16 index]{set=>_arr[index]=value;get=>_arr[index];}}";
    }

    public class AttributesTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System.Diagnostics;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "[DebuggerDisplay(\"Prop\")] public class Def { [DebuggerHidden, DebuggerNonUserCode] public Def() { }  [Conditional(\"DEBUG\"), DebuggerHidden, DebuggerNonUserCode] public void M() { }  [DebuggerHidden, DebuggerNonUserCode] public string Prop { set; [return: System.Diagnostics.CodeAnalysis.NotNull] get; }  [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)] public string field; }";
        public override string ExpectedMinifyCodeBody => "[DebuggerDisplay(\"Prop\")]public class Def{[DebuggerHidden,DebuggerNonUserCode]public Def(){}[Conditional(\"DEBUG\"),DebuggerHidden,DebuggerNonUserCode]public void M(){}[DebuggerHidden,DebuggerNonUserCode]public string Prop{set;[return:System.Diagnostics.CodeAnalysis.NotNull]get;}[DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]public string field;}";
    }

    public class ArrayTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public static class Def { public static object[] Arr = new object[3]; public static uint[] ArrInit = new uint[]{1, 2, 3}; public static int[] ArrInitImplicit = new[]{1, 2, 3}; public static DateTime[, ] Arr2 = new DateTime[3, 6]; public static byte[, ] Arr2Def = new byte[, ]{{1, 2}, {3, 4, }}; public static bool[][] ArrJugged = new bool[2][]; }";
        public override string ExpectedMinifyCodeBody => "public static class Def{public static object[]Arr=new object[3];public static uint[]ArrInit=new uint[]{1,2,3};public static int[]ArrInitImplicit=new[]{1,2,3};public static DateTime[,]Arr2=new DateTime[3,6];public static byte[,]Arr2Def=new byte[,]{{1,2},{3,4,}};public static bool[][]ArrJugged=new bool[2][];}";
    }

    public class AnonymousTypeTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
public class Def
{
    public void M()
    {
        var anonymous = new { Foo = 1, Bar = DateTime.Now };
        var anonymous2 = new { Foo = 1, anonymous };
    }
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { public void M() { var anonymous = new { Foo = 1, Bar = DateTime.Now }  ; var anonymous2 = new { Foo = 1, anonymous }  ; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{public void M(){var anonymous=new{Foo=1,Bar=DateTime.Now};var anonymous2=new{Foo=1,anonymous};}}";
    }

    public class AnonymousMethodTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System.Linq;
public class Def
{
    int[] Arr = new[] { 1, 2, 3 };
    public int M()
        => Arr.Single(delegate (int i) { return i < 2; });
}

";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System.Linq;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { int[] Arr = new[]{1, 2, 3}; public int M() => Arr.Single(delegate (int i) { return i < 2; }); }";
        public override string ExpectedMinifyCodeBody => "public class Def{int[]Arr=new[]{1,2,3};public int M()=>Arr.Single(delegate(int i){return i<2;});}";
    }

    public class LambdaTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System.Linq;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { int[] Arr = new[]{1, 2, 3}; public int M() => Arr.First(n => n < 2) + Arr.First((int n) => n < 2) + Arr.Select((n, index) => n * index).First() + Arr.Select((int n, int index) => n * index).First(); }";
        public override string ExpectedMinifyCodeBody => "public class Def{int[]Arr=new[]{1,2,3};public int M()=>Arr.First(n=>n<2)+Arr.First((int n)=>n<2)+Arr.Select((n,index)=>n*index).First()+Arr.Select((int n,int index)=>n*index).First();}";
    }

    public class AsyncAwaitTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
public class Def
{
    Int32[] Arr = new[] { 1, 2, 3 };
    public async void M()
    {
        await System.Threading.Tasks.Task.Delay(3);
    }
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Int32[] Arr = new[]{1, 2, 3}; public async void M() { await System.Threading.Tasks.Task.Delay(3); } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Int32[]Arr=new[]{1,2,3};public async void M(){await System.Threading.Tasks.Task.Delay(3);}}";
    }

    public class PatternsTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
            IComparable c and long l when c.CompareTo(l) > 1 => l,
            DateTime d and { Ticks: 131 } => d.Ticks,
            _ => 1,
        };
    }
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Int32[] Arr = new[]{1, 2, 3}; public void M() { _ = ((object)DateTime.Now)switch { 1 => 1, int i => i, IComparable c and long l when c.CompareTo(l) > 1 => l, DateTime d and { Ticks: 131 }  => d.Ticks, _ => 1, }  ; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Int32[]Arr=new[]{1,2,3};public void M(){_=((object)DateTime.Now)switch{1=>1,int i=>i,IComparable c and long l when c.CompareTo(l)>1=>l,DateTime d and{Ticks:131}=>d.Ticks,_=>1,};}}";
    }

    public class UnaryExpressionTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { _ = +1; _ = -3; _ = ~4; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){_=+1;_=-3;_=~4;}}";
    }

    public class BinaryExpressionTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { _ = 1 + 2; _ = 1 - 2; _ = 1 * 2; _ = 1 / 2; _ = 1 % 2; _ = 1 << 2; _ = 1 >> 2; _ = 1 & 2; _ = true && false; _ = 1 | 2; _ = true || false; _ = 1 ^ 2; _ = 1 == 2; _ = 1 != 2; _ = 1 < 2; _ = 1 <= 2; _ = 1 > 2; _ = 1 >= 2; _ = Arr[0] is int; _ = Arr[0] as string; _ = Arr[0] ?? 41; }  public static Def operator +(Def a, Def b) => null; }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){_=1+2;_=1-2;_=1*2;_=1/2;_=1%2;_=1<<2;_=1>>2;_=1&2;_=true&&false;_=1|2;_=true||false;_=1^2;_=1==2;_=1!=2;_=1<2;_=1<=2;_=1>2;_=1>=2;_=Arr[0]is int;_=Arr[0]as string;_=Arr[0]??41;}public static Def operator+(Def a,Def b)=>null;}";
    }

    public class ConditionalTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
public class Def
{
    Object[] Arr = new object[] { 1, 2, 3 };
    public void M()
    {
        _ = true ? 1 : 2;
    }
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { _ = true ? 1 : 2; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){_=true?1:2;}}";
    }

    public class TryCatchFinallyTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { try { Console.Write(10); } catch (OverflowException) { throw; } catch (AggregateException e)when (e.InnerException is not null) { throw new Exception(e.Message, e); } catch { } finally { Console.Write(1); } } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){try{Console.Write(10);}catch(OverflowException){throw;}catch(AggregateException e)when(e.InnerException is not null){throw new Exception(e.Message,e);}catch{}finally{Console.Write(1);}}}";
    }

    public class CheckedUncheckedTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Int32[] Arr = new int[]{1, 2, 3}; public void M() { _ = checked((short)(Arr[0] << 20)); _ = unchecked((short)(Arr[0] << 20)); checked { _ = (byte)(Arr[0] << 10); }  unchecked { _ = (byte)(Arr[0] << 10); } } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Int32[]Arr=new int[]{1,2,3};public void M(){_=checked((short)(Arr[0]<<20));_=unchecked((short)(Arr[0]<<20));checked{_=(byte)(Arr[0]<<10);}unchecked{_=(byte)(Arr[0]<<10);}}}";
    }

    public class DefaultTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public long M() { int a = default; var b = default(byte); return a + b; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public long M(){int a=default;var b=default(byte);return a+b;}}";
    }

    public class DocumentationCommentTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public int M(uint i) { return (int)i; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public int M(uint i){return(int)i;}}";
    }

    public class ProcessorTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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

";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { Console.Write(0); Console.Write(true); Console.Write(2U); } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){Console.Write(0);Console.Write(true);Console.Write(2U);}}";
    }

    public class PointerTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { public unsafe object M() { long x = 0; var p = &x; var pi = (Int32*)p; *pi = int.MaxValue; var y = x * *pi; p->CompareTo(2); return pi[1]; }  public object M2(string str) { unsafe { fixed (char* p = str) { var pi = (Int32*)p; *pi = sizeof(DateTime); return pi[1]; } } } }";
        public override string ExpectedMinifyCodeBody => "public class Def{public unsafe object M(){long x=0;var p=&x;var pi=(Int32*)p;*pi=int.MaxValue;var y=x* *pi;p->CompareTo(2);return pi[1];}public object M2(string str){unsafe{fixed(char*p=str){var pi=(Int32*)p;*pi=sizeof(DateTime);return pi[1];}}}}";
    }

    public class ForTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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

";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { for (int i = 0; i < Arr.Length; i++) { Console.Write(Arr[i]); }  for (int i = 0; i < Arr.Length; i++) Console.WriteLine(Arr[i]); } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){for(int i=0;i<Arr.Length;i++){Console.Write(Arr[i]);}for(int i=0;i<Arr.Length;i++)Console.WriteLine(Arr[i]);}}";
    }

    public class ForEachTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public System.Collections.IEnumerable M() { foreach (var a in Arr) { yield return a; }  foreach (var a in Arr) yield return a; yield break; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public System.Collections.IEnumerable M(){foreach(var a in Arr){yield return a;}foreach(var a in Arr)yield return a;yield break;}}";
    }

    public class WhileTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public int M() { int a = 0; while (a < 10) a++; while (a < 20) { a++; }  do { a++; } while (a < 0); return a; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public int M(){int a=0;while(a<10)a++;while(a<20){a++;}do{a++;}while(a<0);return a;}}";
    }

    public class SwitchTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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

";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public int M() { switch ((int)Arr[0]) { case 1: return 1; case 2: return 2; case 3: case 4: return 4; case 5: goto case 2; default: break; }  return 0; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public int M(){switch((int)Arr[0]){case 1:return 1;case 2:return 2;case 3:case 4:return 4;case 5:goto case 2;default:break;}return 0;}}";
    }

    public class FunctionPointersTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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

";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { public unsafe void M(delegate*<int, void> f) { f(10); }  private static void F(int i) => Console.Write(i); public unsafe void M2() => M(&F); }";
        public override string ExpectedMinifyCodeBody => "public class Def{public unsafe void M(delegate*<int,void>f){f(10);}private static void F(int i)=>Console.Write(i);public unsafe void M2()=>M(&F);}";
    }

    public class GotoTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { if (true) goto Label; Label: { } } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){if(true)goto Label;Label:{}}}";
    }

    public class GenericsTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def<T, Q>");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def<T, Q> where T : ICloneable { public void M<R, S, C, U, E, D>() where R : class, ICloneable where S : struct where C : notnull, System.Collections.Generic.Comparer<int>, new() where U : unmanaged where E : struct, Enum where D : Delegate { Console.WriteLine(typeof(Def<, >)); Console.WriteLine(typeof(Def<int[], long>)); } }";
        public override string ExpectedMinifyCodeBody => "public class Def<T,Q>where T:ICloneable{public void M<R,S,C,U,E,D>()where R:class,ICloneable where S:struct where C:notnull,System.Collections.Generic.Comparer<int>,new()where U:unmanaged where E:struct,Enum where D:Delegate{Console.WriteLine(typeof(Def<,>));Console.WriteLine(typeof(Def<int[],long>));}}";
    }

    public class StringTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M(int i) { Console.WriteLine(\"i\"); Console.WriteLine($\"{i}\"); Console.WriteLine($\"{i:0000}\"); } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(int i){Console.WriteLine(\"i\");Console.WriteLine($\"{i}\");Console.WriteLine($\"{i:0000}\");}}";
    }

    public class RangeTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { Console.WriteLine(Arr[1..]); Console.WriteLine(Arr[..1]); Console.WriteLine(Arr[^1..]); Console.WriteLine(Arr[..^1]); Console.WriteLine(Arr[^1]); } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){Console.WriteLine(Arr[1..]);Console.WriteLine(Arr[..1]);Console.WriteLine(Arr[^1..]);Console.WriteLine(Arr[..^1]);Console.WriteLine(Arr[^1]);}}";
    }

    public class TupleTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M() { (int a, byte b) t = (1, 2); var(a, b) = t; Console.WriteLine(t); } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(){(int a,byte b)t=(1,2);var(a,b)=t;Console.WriteLine(t);}}";
    }

    public class UsingTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System.IO;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { byte[] Arr = new byte[]{1, 2, 3}; public void M() { using var ms1 = new MemoryStream(Arr); using (var ms2 = new MemoryStream()) ms2.CopyTo(ms1); using (var ms3 = new MemoryStream()) { ms3.CopyTo(ms1); } } }";
        public override string ExpectedMinifyCodeBody => "public class Def{byte[]Arr=new byte[]{1,2,3};public void M(){using var ms1=new MemoryStream(Arr);using(var ms2=new MemoryStream())ms2.CopyTo(ms1);using(var ms3=new MemoryStream()){ms3.CopyTo(ms1);}}}";
    }

    public class IfTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { Object[] Arr = new object[]{1, 2, 3}; public void M(int n) { if (n < 0) Console.Write(1); else if (n < 10) Console.Write(10); else Console.Write(100); if (n < 0) { Console.WriteLine(1); } else if (n < 10) { Console.WriteLine(10); } else { Console.WriteLine(100); } } }";
        public override string ExpectedMinifyCodeBody => "public class Def{Object[]Arr=new object[]{1,2,3};public void M(int n){if(n<0)Console.Write(1);else if(n<10)Console.Write(10);else Console.Write(100);if(n<0){Console.WriteLine(1);}else if(n<10){Console.WriteLine(10);}else{Console.WriteLine(100);}}}";
    }

    public class LINQTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System.Linq;
public class Def
{
    public object M()
    {
        var obj = from d in Enumerable.Repeat(System.DateTime.Now, 5) where d.DayOfYear < 200 orderby d.Year select d.DayOfWeek;
        return obj.First();
    }
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System.Linq;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { public object M() { var obj = from d in Enumerable.Repeat(System.DateTime.Now, 5) where d.DayOfYear < 200 orderby d.Year select d.DayOfWeek; return obj.First(); } }";
        public override string ExpectedMinifyCodeBody => "public class Def{public object M(){var obj=from d in Enumerable.Repeat(System.DateTime.Now,5)where d.DayOfYear<200 orderby d.Year select d.DayOfWeek;return obj.First();}}";
    }

    public class IncrementTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { public void M(int n) { Int32 a = 0; int b = 2 + ++a + 1; var c = a++ + ++b + ~-1; c = a++ + b; c = a + ++b; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{public void M(int n){Int32 a=0;int b=2+ ++a+1;var c=a++ + ++b+~-1;c=a++ +b;c=a+ ++b;}}";
    }

    public class DecrementTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
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
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { public void M(int n) { Int32 a = 0; int b = 2 - --a - 1; var c = a-- - --b - ~1; c = a-- - b; c = a - --b; } }";
        public override string ExpectedMinifyCodeBody => "public class Def{public void M(int n){Int32 a=0;int b=2- --a-1;var c=a-- - --b-~1;c=a-- -b;c=a- --b;}}";
    }

    public class GenericLambdaTest : EmbedderGeneratorTestBaseWithValue
    {
        public override string Syntax => @"
using System;
public class Def
{
    public int M<T>(T n) where T : IComparable<T> => n.CompareTo(default);
}
";
        public override IEnumerable<string> ExpectedTypeNames => ImmutableArray.Create("Def");
        public override IEnumerable<string> ExpectedUsings => ImmutableArray.Create("using System;");
        public override IEnumerable<string> ExpectedDependencies => ImmutableArray.Create<string>();
        public override string ExpectedCodeBody => "public class Def { public int M<T>(T n) where T : IComparable<T> => n.CompareTo(default); }";
        public override string ExpectedMinifyCodeBody => "public class Def{public int M<T>(T n)where T:IComparable<T> =>n.CompareTo(default);}";
    }
}
