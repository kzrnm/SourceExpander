using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceExpander.Embedder.Test
{
    class Util
    {
        public static readonly MetadataReference[] defaultMetadatas = GetDefaulMetadatas().ToArray();
        public static readonly MetadataReference expanderCoreReference = MetadataReference.CreateFromFile(typeof(SourceFileInfo).Assembly.Location);

        private static IEnumerable<MetadataReference> GetDefaulMetadatas()
        {
            var directory = Path.GetDirectoryName(typeof(object).Assembly.Location);
            foreach (var file in Directory.EnumerateFiles(directory, "System*.dll"))
            {
                yield return MetadataReference.CreateFromFile(file);
            }
        }

    }
}
