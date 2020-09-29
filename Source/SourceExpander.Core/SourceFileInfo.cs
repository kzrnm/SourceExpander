using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace SourceExpander
{
    [DebuggerDisplay("{" + nameof(FileName) + "}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SourceFileInfo
    {
        public string? FileName { get; set; }
        public IEnumerable<string>? TypeNames { get; set; }
        public IEnumerable<string>? Usings { get; set; }
        public IEnumerable<string>? Dependencies { get; set; }
        public string? CodeBody { get; set; }
        public string RestoredCode => string.Join("\n", (Usings ?? Array.Empty<string>()).Append(CodeBody));
    }
}
