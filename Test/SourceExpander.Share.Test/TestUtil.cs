using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceExpander.Share
{
    static class TestUtil
    {
        public static readonly MetadataReference[] defaultMetadatas = GetDefaulMetadatas().ToArray();

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
