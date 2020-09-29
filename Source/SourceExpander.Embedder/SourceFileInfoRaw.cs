using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander.Embedder
{
    [DebuggerDisplay("SourceFileInfoRaw: {" + nameof(FilePath) + "}")]
    public class SourceFileInfoRaw
    {
        public string FilePath { set; get; }
        public string OrigCode { get; }
        public ReadOnlyCollection<string> Usings { get; }
        public string CodeBody { get; }
        public SyntaxTree SyntaxTree { get; }

        public ReadOnlyCollection<string>? TypeNames { private set; get; }
        public ReadOnlyCollection<string>? Dependencies { private set; get; }
        public SourceFileInfoRaw(string filePath, string code)
        {
            var tree = CSharpSyntaxTree.ParseText(code);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var usings = root.Usings.Select(u => u.ToString().Trim()).ToArray();

            var remover = new UsingDirectiveRemover();
            var newRoot = (CompilationUnitSyntax)remover.Visit(root);

            SyntaxTree = tree;
            FilePath = filePath;
            OrigCode = code;
            Usings = new ReadOnlyCollection<string>(usings);
            CodeBody = MinifySpace(newRoot.ToString());
        }
        public static SourceFileInfoRaw ParseFile(string path)
        {
            var code = File.ReadAllText(path);
            return new SourceFileInfoRaw(path, code);
        }

        public void ResolveType(Compilation compilation)
        {
            var semanticModel = compilation.GetSemanticModel(SyntaxTree);
            var root = (CompilationUnitSyntax)SyntaxTree.GetRoot();
            TypeNames = new ReadOnlyCollection<string>(
                root.Members
                .SelectMany(s => s.DescendantNodes())
                .OfType<BaseTypeDeclarationSyntax>()
                .Select(syntax => semanticModel.GetDeclaredSymbol(syntax)?.ToDisplayString())
                .OfType<string>()
                .Distinct()
                .ToArray());
        }

        private static string MinifySpace(string str) => Regex.Replace(str, " +", " ");
    }
}
