using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp;
#nullable enable
namespace SourceExpander
{
    [DebuggerDisplay("{" + nameof(FileName) + "}")]
    [DataContract]
    internal class SourceFileInfo(
        string? fileName,
        IEnumerable<string>? typeNames,
        IEnumerable<string>? usings,
        IEnumerable<string>? dependencies,
        string? codeBody,
        bool @unsafe = false)
    {
        private static string[] Sorted(IEnumerable<string>? collection, IComparer<string> comparer)
        {
            if (collection is null)
                return Array.Empty<string>();
            var array = collection.ToArray();
            Array.Sort(array, comparer);
            return array;
        }

        [DataMember]
        public string CodeBody { get; set; } = codeBody ?? "";
        [DataMember]
        public IEnumerable<string> Dependencies { get; set; } = Sorted(dependencies, UsingComparer.Default);
        [DataMember]
        public string FileName { get; set; } = fileName ?? "";
        [DataMember]
        public IEnumerable<string> TypeNames { get; set; } = Sorted(typeNames, UsingComparer.Default);
        [DataMember]
        public IEnumerable<string> Usings { get; set; } = Sorted(usings, UsingComparer.Default);
        [DataMember(EmitDefaultValue = false)]
        public bool Unsafe { get; set; } = @unsafe;

        public string Restore() => string.Join("\n", (Usings ?? Array.Empty<string>()).Append(CodeBody));
        public CSharpSyntaxTree ToSyntaxTree(CSharpParseOptions options,
            Encoding encoding,
            CancellationToken cancellationToken = default)
            => (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(Restore(),
                options,
                FileName,
                encoding,
                cancellationToken);
    }
}
