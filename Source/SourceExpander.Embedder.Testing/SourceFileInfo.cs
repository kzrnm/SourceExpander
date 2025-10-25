using System.Collections.Immutable;
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
        ImmutableArray<string> typeNames,
        ImmutableArray<string> usings,
        ImmutableArray<string> dependencies,
        string? codeBody)
        {
            FileName = fileName ?? "";
            TypeNames = typeNames;
            Usings = usings;
            Dependencies = dependencies;
            CodeBody = codeBody ?? "";
        }

        public string FileName { get; }
        public ImmutableArray<string> TypeNames { get; }
        public ImmutableArray<string> Usings { get; }
        public ImmutableArray<string> Dependencies { get; }
        public string CodeBody { get; }

        public string Restore() => string.Join("\n", Usings.Append(CodeBody));
    }
}
