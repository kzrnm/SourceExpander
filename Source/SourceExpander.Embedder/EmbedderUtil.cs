using System.Runtime.CompilerServices;

namespace SourceExpander.Embedder
{
    public static class EmbedderUtil
    {
        public static string CurrentFilePath([CallerFilePath] string path = "") => path;
    }
}
