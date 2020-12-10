using System.Linq;
using Microsoft.CodeAnalysis;

namespace SourceExpander.Embedder.Test
{
    static class TestUtil
    {
        public static readonly MetadataReference[] defaultMetadatas = MetadataUtil.GetDefaulMetadatas().ToArray();
        public static readonly MetadataReference expanderCoreReference = MetadataReference.CreateFromFile(typeof(SourceFileInfo).Assembly.Location);
    }
}
