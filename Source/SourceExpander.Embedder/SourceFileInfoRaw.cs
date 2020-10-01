using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace SourceExpander
{
    internal class SourceFileInfoRaw
    {
        public SyntaxTree SyntaxTree { get; set; }
        public string FileName { get; set; }
        public IEnumerable<string> TypeNames { get; set; }
        public IEnumerable<string> Usings { get; set; }
        public string CodeBody { get; set; }

        public SourceFileInfoRaw(
            SyntaxTree syntaxTree,
            string fileName,
            IEnumerable<string> typeNames,
            IEnumerable<string> usings,
            string codeBody)
        {
            SyntaxTree = syntaxTree;
            FileName = fileName;
            TypeNames = typeNames;
            Usings = usings;
            CodeBody = codeBody;
        }
    }
}
