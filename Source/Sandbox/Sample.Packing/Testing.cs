using System.IO;
using System.Runtime.CompilerServices;

namespace Sample
{
    [SourceExpander.NotEmbeddingSource]
    public class Testing
    {
        public static string PackingProjectDirectory => field ??= PackingProjectDirectoryImpl();
        static string PackingProjectDirectoryImpl([CallerFilePath] string file = "") => Path.GetDirectoryName(file);
    }
}
