using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public static class CompileTimeTypeMaker
    {
        private const string COMPILETIME_FILE = "CompileTime:";

        public static bool IsCompileTimeType(this SyntaxTree tree) => tree.FilePath.Contains(COMPILETIME_FILE);
        public static IEnumerable<CSharpSyntaxTree> CreateSyntaxes(CSharpParseOptions parseOptions)
            => Sources.Select(tt => (CSharpSyntaxTree)CSharpSyntaxTree.ParseText(
                 text: tt.sourceText,
                 options: parseOptions,
                 path: COMPILETIME_FILE + tt.hintName));

        public static readonly ImmutableArray<(string hintName, SourceText sourceText)> Sources
            = ImmutableArray.Create<(string hintName, SourceText sourceText)>(
                ("NotEmbeddingSourceAttribute", NotEmbeddingText)
            );
        public static int SourceCount => Sources.Length;

        private static SourceText NotEmbeddingText => SourceText.From(@"using System;

namespace SourceExpander
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Enum |
        AttributeTargets.Interface |
        AttributeTargets.Delegate,
        Inherited = false, AllowMultiple = false)]
    [System.Diagnostics.Conditional(""COMPILE_TIME_ONLY"")]
    internal sealed class NotEmbeddingSourceAttribute : Attribute
    {
        public NotEmbeddingSourceAttribute() { }
    }
}", Encoding.UTF8);
    }
}
