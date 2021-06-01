using System;
using System.Collections.Generic;
#nullable enable
namespace SourceExpander
{
    public class UsingComparer : IComparer<string>
    {
        public static readonly UsingComparer Default = new();
        public int Compare(string x, string y) => StringComparer.Ordinal.Compare(x.TrimEnd(';'), y.TrimEnd(';'));
    }
}
