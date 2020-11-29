using System.Diagnostics;

namespace SourceExpander
{
    public static class Expander
    {
        [Conditional("EXPANDER")]
        public static void Expand(string inputFilePath = null, string outputFilePath = null) { }
        public static string ExpandString(string inputFilePath = null) { return ""; }
    }
}
