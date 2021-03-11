using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public static class CompileTimeTypeMaker
    {
        public static readonly ImmutableArray<(string hintName, SourceText sourceText)> Sources
            = ImmutableArray.Create<(string hintName, SourceText sourceText)>(
                ("NotEmbeddingSourceAttribute.cs", NotEmbeddingText)
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
    [NotEmbeddingSource]
    internal sealed class NotEmbeddingSourceAttribute : Attribute
    {
        public NotEmbeddingSourceAttribute() { }
    }
}", Encoding.UTF8);
    }
}
