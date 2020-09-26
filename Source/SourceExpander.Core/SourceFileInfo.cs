using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace SourceExpander
{
    [DebuggerDisplay("{" + nameof(FileName) + "}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SourceFileInfo
    {
        public string FileName { get; }
        public ReadOnlyCollection<string> TypeNames { get; }
        public ReadOnlyCollection<string> Usings { get; }
        public ReadOnlyCollection<string> Dependencies { get; }
        public string CodeBody { get; }
        public string RestoredCode => string.Join("\n", Usings.Append(CodeBody));
        public SourceFileInfo(string fileName, IList<string> typeNames, IList<string> usings, IList<string> dependencies, string code)
        {
            FileName = fileName;
            TypeNames = new ReadOnlyCollection<string>(typeNames);
            Usings = new ReadOnlyCollection<string>(usings);
            Dependencies = new ReadOnlyCollection<string>(dependencies);
            CodeBody = code;
        }
    }
}
