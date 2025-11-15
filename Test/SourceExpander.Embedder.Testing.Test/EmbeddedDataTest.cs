using System;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.Loader;
using System.Threading.Tasks;

namespace SourceExpander.Embedder.Testing
{
    public class EmbeddedDataTest
    {
        [Test]
        public async Task GZipBase32768()
        {
            var dllPath = GetTestDataPath("SampleLibrary2.dll");
            var embedded = await GetEmbeddedDataAsync(dllPath);
            embedded.AssemblyMetadatas.Keys.ShouldBe([
                "SourceExpander.EmbeddedLanguageVersion",
                "SourceExpander.EmbeddedSourceCode.GZipBase32768",
                "SourceExpander.EmbedderVersion",
                "SourceExpander.EmbeddedNamespaces",
            ], ignoreOrder: true);
            embedded.EmbeddedLanguageVersion.ShouldBe("6");
            embedded.AssemblyMetadatas["SourceExpander.EmbeddedLanguageVersion"].ShouldBe("6");
            embedded.EmbedderVersion.ShouldBe("4.0.2.100");
            embedded.AssemblyMetadatas["SourceExpander.EmbedderVersion"].ShouldBe("4.0.2.100");
            embedded.EmbeddedNamespaces.ShouldBe(["MathLibrary.Double", "SampleLibrary"], ignoreOrder: true);
            embedded.AssemblyMetadatas["SourceExpander.EmbeddedNamespaces"].ShouldBe("MathLibrary.Double,SampleLibrary");

            embedded.SourceFiles.ShouldBeEquivalentTo(ImmutableArray.Create<SourceFileInfo>([
                new(
                    "_SampleLibrary2>Math.cs",
                    ["Ep", "MathLibrary.Double.Pi"],
                    [],
                    [],
                   "namespace MathLibrary.Double{public static class Pi{public const double PI=System.Math.PI;}}public static class Ep{public const double EP=System.Math.E;}"),
                new(
                    "_SampleLibrary2>Put2.cs",
                    ["SampleLibrary.Put2"],
                    [],
                    ["_SampleLibrary>Put.cs"],
                   "namespace SampleLibrary{public static class Put2{public static void Write()=>Put.WriteRandom();}}"),
            ]));
        }

        [Test]
        public async Task Raw()
        {
            var dllPath = GetTestDataPath("SampleLibrary.Old.dll");
            var embedded = await GetEmbeddedDataAsync(dllPath);
            embedded.AssemblyMetadatas.Keys.ShouldBe(["SourceExpander.EmbeddedSourceCode"]);
            embedded.SourceFiles.ShouldBeEquivalentTo(ImmutableArray.Create<SourceFileInfo>([
                new(
                    "_SampleLibrary>Bit.cs",
                    ["SampleLibrary.Bit"],
                    ["using System.Runtime.CompilerServices;", "using System.Runtime.Intrinsics.X86;"],
                    [],
                    "namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } "),
                new(
                    "_SampleLibrary>Put.cs",
                    ["SampleLibrary.Put"],
                    ["using System.Diagnostics;"],
                    ["_SampleLibrary>Xorshift.cs"],
                    "namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } "),
                new(
                    "_SampleLibrary>Xorshift.cs",
                    ["SampleLibrary.Xorshift"],
                    ["using System;"],
                    [],
                    "namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } "),
            ]));
        }

        private static ValueTask<EmbeddedData> GetEmbeddedDataAsync(string assemblyPath)
        {
            var alc = new AssemblyLoadContext("GetEmbeddedData", true);
            try
            {
                return EmbeddedData.LoadFromAssembly(alc.LoadFromAssemblyPath(Path.GetFullPath(assemblyPath)));
            }
            finally
            {
                alc.Unload();
            }
        }

        private static readonly string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        public static string GetTestDataPath(params string[] paths)
        {
            var withDir = new string[paths.Length + 2];
            withDir[0] = dir;
            withDir[1] = "testdata";
            Array.Copy(paths, 0, withDir, 2, paths.Length);
            return Path.Combine(withDir);
        }
    }
}
