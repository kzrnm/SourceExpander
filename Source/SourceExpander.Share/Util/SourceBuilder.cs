using System;
using System.Text;

namespace SourceExpander;

class SourceBuilder
{
    StringBuilder StringBuilder { get; }
    int Level { get; }
    string IndentSpace { get; }
    private SourceBuilder(StringBuilder stringBuilder, int level)
    {
        StringBuilder = stringBuilder;
        Level = level;
        IndentSpace = new string(' ', level * 4);
    }
    public SourceBuilder(int level) : this(new(), level) { }
    public SourceBuilder() : this(new(), 0) { }

    public override string ToString() => StringBuilder.ToString().Replace("\r\n", "\n");

    public void Indent(Action<SourceBuilder> inner) => Indent(1, inner);
    public void Indent(int indentLevel, Action<SourceBuilder> inner) => inner.Invoke(new(StringBuilder, Level + indentLevel));

    public void AppendLine() => StringBuilder.AppendLine();
    public void AppendLine(string text) => StringBuilder
        .Append(IndentSpace)
        .AppendLine(text.Replace("\r\n", "\n").Replace("\n", $"\n{IndentSpace}"));

    public void AppendLineRaw(string text) => StringBuilder.AppendLine(text);
    public void AppendRaw(string text) => StringBuilder.Append(text);
}
