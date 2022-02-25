using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace SourceExpander.Embedder.Testing
{
    public class EmbeddedDataTest
    {
        [Fact]
        public async Task GZipBase32768()
        {
            var dllPath = GetTestDataPath("SampleLibrary2.dll");
            var embedded = await GetEmbeddedDataAsync(dllPath);
            embedded.AssemblyMetadatas.Should()
                .ContainKeys(
                "SourceExpander.EmbeddedLanguageVersion",
                "SourceExpander.EmbedderVersion",
                "SourceExpander.EmbeddedNamespaces",
                "SourceExpander.EmbeddedSourceCode.GZipBase32768")
                .And.HaveCount(4);
            embedded.EmbeddedLanguageVersion.Should().Be("6");
            embedded.AssemblyMetadatas["SourceExpander.EmbeddedLanguageVersion"].Should().Be("6");
            embedded.EmbedderVersion.Should().Be("4.0.2.100");
            embedded.AssemblyMetadatas["SourceExpander.EmbedderVersion"].Should().Be("4.0.2.100");
            embedded.EmbeddedNamespaces.Should().BeEquivalentTo("MathLibrary.Double", "SampleLibrary");
            embedded.AssemblyMetadatas["SourceExpander.EmbeddedNamespaces"].Should().Be("MathLibrary.Double,SampleLibrary");

            embedded.SourceFiles.Should().BeEquivalentTo(new SourceFileInfo[]
            {
                new SourceFileInfo(
                    "_SampleLibrary2>Put2.cs",
                    new []{"SampleLibrary.Put2"},
                    null,
                   new[]{"_SampleLibrary>Put.cs"},
                   "namespace SampleLibrary{public static class Put2{public static void Write()=>Put.WriteRandom();}}"),
                new SourceFileInfo(
                    "_SampleLibrary2>Math.cs",
                    new []{"Ep", "MathLibrary.Double.Pi"},
                    null,
                   new string[0],
                   "namespace MathLibrary.Double{public static class Pi{public const double PI=System.Math.PI;}}public static class Ep{public const double EP=System.Math.E;}")
            });
        }

        [Fact]
        public async Task Raw()
        {
            var dllPath = GetTestDataPath("SampleLibrary.Old.dll");
            var embedded = await GetEmbeddedDataAsync(dllPath);
            embedded.AssemblyMetadatas.Should().ContainKey("SourceExpander.EmbeddedSourceCode")
                .And.HaveCount(1);
            embedded.SourceFiles.Should().BeEquivalentTo(new SourceFileInfo[]
            {
                new SourceFileInfo(
                    "_SampleLibrary>Bit.cs",
                    new[]{"SampleLibrary.Bit"},
                    new[]{"using System.Runtime.CompilerServices;", "using System.Runtime.Intrinsics.X86;"},
                    null,
                    "namespace SampleLibrary { public static class Bit { [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int ExtractLowestSetBit(int n) { if (Bmi1.IsSupported) { return (int)Bmi1.ExtractLowestSetBit((uint)n); } return n & -n; } } } "),
                new SourceFileInfo(
                    "_SampleLibrary>Put.cs",
                    new[]{"SampleLibrary.Put"},
                    new[]{"using System.Diagnostics;"},
                    new[]{"_SampleLibrary>Xorshift.cs"},
                    "namespace SampleLibrary { public static class Put { private static readonly Xorshift rnd = new Xorshift(); public static void WriteRandom() => Trace.WriteLine(rnd.Next()); } } "),
                new SourceFileInfo(
                    "_SampleLibrary>Xorshift.cs",
                    new[]{"SampleLibrary.Xorshift"},
                    new[]{"using System;"},
                    null,
                    "namespace SampleLibrary { public class Xorshift : Random { private uint x = 123456789; private uint y = 362436069; private uint z = 521288629; private uint w; private static readonly Random rnd = new Random(); public Xorshift() : this(rnd.Next()) { } public Xorshift(int seed) { w = (uint)seed; } protected override double Sample() => InternalSample() * (1.0 / uint.MaxValue); private uint InternalSample() { uint t = x ^ (x << 11); x = y; y = z; z = w; return w = (w ^ (w >> 19)) ^ (t ^ (t >> 8)); } } } "),
            });
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

        private static readonly string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
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
