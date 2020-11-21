using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace SourceExpander
{
    [DebuggerDisplay("{" + nameof(FileName) + "}")]
    [DataContract]
    public class SourceFileInfo
    {
        public SourceFileInfo(
        string? fileName,
        IEnumerable<string>? typeNames,
        IEnumerable<string>? usings,
        IEnumerable<string>? dependencies,
        string? codeBody)
        {
            FileName = fileName ?? "";
            TypeNames = typeNames ?? Array.Empty<string>();
            Usings = usings ?? Array.Empty<string>();
            Dependencies = dependencies ?? Array.Empty<string>();
            CodeBody = codeBody ?? "";
        }

        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public IEnumerable<string> TypeNames { get; set; }
        [DataMember]
        public IEnumerable<string> Usings { get; set; }
        [DataMember]
        public IEnumerable<string> Dependencies { get; set; }
        [DataMember]
        public string CodeBody { get; set; }

        public string Restore() => string.Join("\n", (Usings ?? Array.Empty<string>()).Append(CodeBody));
    }
}
