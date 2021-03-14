﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
#nullable enable
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
            TypeNames = Sorted(typeNames, UsingComparer.Default);
            Usings = Sorted(usings, UsingComparer.Default);
            Dependencies = Sorted(dependencies, UsingComparer.Default);
            CodeBody = codeBody ?? "";
        }

        private static string[] Sorted(IEnumerable<string>? collection, IComparer<string> comparer)
        {
            if (collection is null)
                return Array.Empty<string>();
            var array = collection.ToArray();
            Array.Sort(array, comparer);
            return array;
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
