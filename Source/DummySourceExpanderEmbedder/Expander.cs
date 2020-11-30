using System.Diagnostics;

namespace SourceExpander
{
    public class Expander
    {
        [Conditional("EXPANDER")]
        public static void Expand(string inputFilePath = null, string outputFilePath = null, bool ignoreAnyError = true) { }
        public static string ExpandString(string inputFilePath = null, bool ignoreAnyError = true) { return ""; }
    }
}
