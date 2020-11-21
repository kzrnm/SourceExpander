using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SourceExpander
{
    [DebuggerDisplay("{" + nameof(FileName) + "}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DataContract]
    public class SourceFileInfo
    {
        [DataMember]
        public string? FileName { get; set; }
        [DataMember]
        public IEnumerable<string>? TypeNames { get; set; }
        [DataMember]
        public IEnumerable<string>? Usings { get; set; }
        [DataMember]
        public IEnumerable<string>? Dependencies { get; set; }
        [DataMember]
        public string? CodeBody { get; set; }

        public string Restore() => string.Join("\n", (Usings ?? Array.Empty<string>()).Append(CodeBody));
    }
}
