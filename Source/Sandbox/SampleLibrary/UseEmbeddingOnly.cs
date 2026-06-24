using System;
using System.Diagnostics;

namespace SampleLibrary
{
    internal class UseEmbeddingOnly
    {
#if SOURCE_EMBEDDING
        [Conditional("SOURCE_EMBEDDING")]
#endif
        public static void EmbeddingExample()
        {
#if !SOURCE_EMBEDDING
            Debug.Assert(true);
            Console.WriteLine("SOURCE_EMBEDDING symbol will be defined when embedding.");
            Console.WriteLine("This code is not embedded.");
#endif
        }
    }
}
