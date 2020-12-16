using System;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Kzrnm.Convert.Base32768;

namespace SourceExpander
{
    public class EmbeddedData
    {
        /// <summary>
        /// name of assembly
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// <see cref="AssemblyMetadataAttribute"/> of assembly
        /// </summary>
        public ImmutableDictionary<string, string> AssemblyMetadatas { get; }

        /// <summary>
        /// embedded source code
        /// </summary>
        public ImmutableArray<SourceFileInfo> SourceFiles { get; }
        private EmbeddedData(
            string assemblyName,
            ImmutableArray<SourceFileInfo> sourceFiles,
            ImmutableDictionary<string, string> assemblyMetadatas)
        {
            AssemblyName = assemblyName;
            SourceFiles = sourceFiles;
            AssemblyMetadatas = assemblyMetadatas;
        }

        /// <summary>
        /// Load data embedded by SourceExpander.Embedder in assembly defining <paramref name="definedType"/>.
        /// </summary>
        public static ValueTask<EmbeddedData> LoadFromAssembly(Type definedType)
            => LoadFromAssembly(definedType.Assembly);

        /// <summary>
        /// Load data embedded by SourceExpander.Embedder in <paramref name="assembly"/>.
        /// </summary>
        public static async ValueTask<EmbeddedData> LoadFromAssembly(Assembly assembly)
        {
            var metadata = LoadAssemblyMetadatas(assembly);
            return new EmbeddedData(assembly.FullName, await LoadSourceFiles(metadata).ConfigureAwait(false), metadata);
        }


        private static ImmutableDictionary<string, string> LoadAssemblyMetadatas(Assembly assembly)
            => assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .ToImmutableDictionary(attr => attr.Key, attr => attr.Value);

        private static async ValueTask<ImmutableArray<SourceFileInfo>> LoadSourceFiles(ImmutableDictionary<string, string> metadata)
        {
            foreach (var p in metadata)
            {
                var keyArray = p.Key.Split('.');
                if (keyArray.Length >= 2
                    && keyArray[1] == "EmbeddedSourceCode")
                {
                    SourceFileInfo[]? embedded;
                    if (Array.IndexOf(keyArray, "GZipBase32768", 2) >= 0)
                    {
                        using var ms = new MemoryStream(Base32768.Decode(p.Value));
                        using var gz = new GZipStream(ms, CompressionMode.Decompress);
                        embedded = await JsonSerializer.DeserializeAsync<SourceFileInfo[]>(gz).ConfigureAwait(false);
                    }
                    else
                        embedded = JsonSerializer.Deserialize<SourceFileInfo[]>(p.Value);
                    if (embedded is not null)
                        return ImmutableArray.Create(embedded);
                }
            }
            return ImmutableArray<SourceFileInfo>.Empty;
        }
    }
}
