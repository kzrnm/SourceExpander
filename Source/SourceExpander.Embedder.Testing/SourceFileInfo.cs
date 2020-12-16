using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace SourceExpander
{
    [DebuggerDisplay("{" + nameof(FileName) + "}")]
    public class SourceFileInfo
    {
        [JsonConstructor]
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

        public string FileName { get; }
        public IEnumerable<string> TypeNames { get; }
        public IEnumerable<string> Usings { get; }
        public IEnumerable<string> Dependencies { get; }
        public string CodeBody { get; }

        public string Restore() => string.Join("\n", (Usings ?? Array.Empty<string>()).Append(CodeBody));
    }
}
