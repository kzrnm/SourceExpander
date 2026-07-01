using System;
using System.Collections.Generic;
using System.Text;

namespace SourceExpander;

class SourceBuilder
{
    StringBuilder StringBuilder { get; }
    int Level { get; set; }
    string IndentSpace
    {
        get
        {
            while (Level >= _cacheIndentSpaces.Count)
                _cacheIndentSpaces.Add(new(' ', _cacheIndentSpaces.Count * 4));
            return _cacheIndentSpaces[Level];
        }
    }
    static readonly List<string> _cacheIndentSpaces = [
        "",
        "    ",
        "        ",
        "            ",
        "                ",
        "                    ",
        "                        ",
    ];
    private SourceBuilder(StringBuilder stringBuilder, int level)
    {
        StringBuilder = stringBuilder;
        Level = level;
    }
    public SourceBuilder(int level) : this(new(), level) { }
    public SourceBuilder() : this(new(), 0) { }

    public override string ToString() => StringBuilder.ToString().Replace("\r\n", "\n");

    public IDisposable Indent() => Indent(1);
    public IDisposable Indent(int level) => new IndentBlock(this, level);

    public void AppendLine() => StringBuilder.AppendLine();
    public void AppendLine(string text) => StringBuilder
        .Append(IndentSpace)
        .AppendLine(text.Replace("\r\n", "\n").Replace("\n", $"\n{IndentSpace}"));

    public void AppendLineRaw(string text) => StringBuilder.AppendLine(text);
    public void AppendRaw(string text) => StringBuilder.Append(text);

    class IndentBlock : IDisposable
    {
        SourceBuilder? parent;
        readonly int level;
        public IndentBlock(SourceBuilder parent, int level)
        {
            parent.Level += level;
            this.parent = parent;
            this.level = level;
        }
        public void Dispose()
        {
            parent?.Level -= level;
            parent = null;
        }
    }
}
