using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;

namespace SourceExpander;

[DebuggerDisplay("{" + nameof(FileName) + "}")]
[method: JsonConstructor]
public class SourceFileInfo(
    string? fileName, ImmutableArray<string> typeNames, ImmutableArray<string> usings, ImmutableArray<string> dependencies, string? codeBody)
{
    public string FileName { get; } = fileName ?? "";
    public ImmutableArray<string> TypeNames { get; } = typeNames;
    public ImmutableArray<string> Usings { get; } = usings;
    public ImmutableArray<string> Dependencies { get; } = dependencies;
    public string CodeBody { get; } = codeBody ?? "";

    public string Restore() => string.Join("\n", Usings.Append(CodeBody));
}
