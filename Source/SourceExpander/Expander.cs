using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace SourceExpander
{
    public static class Expander
    {
        public static void Expand([CallerFilePath] string inputFilePath = null, string outputFilePath = null)
        {

        }
    }
    public enum ExpandMethod
    {
        /// <summary>
        /// Write all embeded type(fast)
        /// </summary>
        All,
        /// <summary>
        /// Found type name by SyntaxTree(slow)
        /// </summary>
        NameSyntax,
        /// <summary>
        /// Found type name by Compilation(too slow)
        /// </summary>
        Strict,
    }
}
