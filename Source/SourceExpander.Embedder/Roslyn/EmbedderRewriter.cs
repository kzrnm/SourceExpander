using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace SourceExpander.Roslyn
{
    /// <summary>
    /// Rewrite by <see cref="EmbedderConfig"/>
    /// </summary>
    class EmbedderRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel model;
        private readonly EmbedderConfig config;
        public EmbedderRewriter(SemanticModel model, EmbedderConfig config)
        {
            this.model = model;
            this.config = config;
        }
        public override SyntaxNode? VisitAttribute(AttributeSyntax node)
        {
            if (model.GetTypeInfo(node).Type is { } typeSymbol
                && config.ExcludeAttributes.Contains(typeSymbol.ToString()))
                return null;
            return base.VisitAttribute(node);
        }

        public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
        {
            if (base.VisitAttributeList(node) is AttributeListSyntax attrs && attrs.Attributes.Count > 0)
                return attrs;
            return null;
        }
    }
}
