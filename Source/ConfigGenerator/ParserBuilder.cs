using System.Runtime.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander;

internal class ParserBuilder
{
    public ParserBuilder(Compilation compilation, INamedTypeSymbol symbol, ConfigParams configParams)
    {
        Symbol = symbol;
        ConfigParams = configParams;
        NullableT = compilation.GetSpecialType(SpecialType.System_Nullable_T);
        NullableBoolean = NullableT.Construct(compilation.GetSpecialType(SpecialType.System_Boolean));
        NullableString = compilation.GetSpecialType(SpecialType.System_String);
        NullableObject = compilation.GetSpecialType(SpecialType.System_Object);
        NullableStringArray = compilation.CreateArrayTypeSymbol(NullableString);
    }
    INamedTypeSymbol Symbol { get; }
    ConfigParams ConfigParams { get; }

    INamedTypeSymbol NullableT { get; }
    ITypeSymbol NullableBoolean { get; }
    ITypeSymbol NullableString { get; }
    ITypeSymbol NullableObject { get; }
    ITypeSymbol NullableStringArray { get; }
    public string Build()
    {
        var sb = new SourceBuilder();
        if (!Symbol.ContainingNamespace.IsGlobalNamespace)
            sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine($"namespace {Symbol.ContainingNamespace.Name};");
        sb.AppendLine($$"""
#pragma warning disable CS0219,CS0168
partial record {{Symbol.Name}}
{
    internal record Parser(
        string? SourceText
""");
        sb.AppendLine($$"""
    )
    {
        
    }
    public static {{Symbol.Name}} Parse(string? sourceText, global::Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions analyzerConfigOptions)
    {
        try
        {
"""
);
        using (sb.Indent(3))
        {
            sb.AppendLine($$"""
{{ConfigParams.Type.Name}} data = sourceText switch
{
    null => new(),
    _ => global::SourceExpander.JsonUtil.ParseJson<{{ConfigParams.Type.Name}}>(sourceText) ?? new(),
};
""");
            sb.AppendLine($"const string buildPropHeader = {SyntaxFactory.Literal("build_property.")};");
            sb.AppendLine($"const string header = buildPropHeader + {SyntaxFactory.Literal(Symbol.ContainingAssembly.Name.ToString().Replace('.', '_') + "_")};");

            sb.AppendLine();
            sb.AppendLine($"string? v;");
            sb.AppendLine("// Parse properties");

            bool isFirstObsolete = true;

            foreach (var configParam in ConfigParams)
            {
                if (configParam.IsObsolete && isFirstObsolete)
                {
                    isFirstObsolete = false;
                    sb.AppendLineRaw("#pragma warning disable CS0612");
                }

                sb.AppendLine($$"""
if (analyzerConfigOptions.TryGetValue(header + nameof(data.{{configParam.Name}}), out v) && !string.IsNullOrWhiteSpace(v))
{
""");
                using (sb.Indent())
                {
                    if (SymbolEqualityComparer.Default.Equals(configParam.Type, NullableBoolean))
                    {
                        sb.AppendLine($$"""
data.{{configParam.Name}} = !global::System.StringComparer.OrdinalIgnoreCase.Equals(v, "false");
""");
                    }
                    else if (SymbolEqualityComparer.Default.Equals(configParam.Type, NullableString)
                    || SymbolEqualityComparer.Default.Equals(configParam.Type, NullableObject))
                    {
                        sb.AppendLine($$"""
data.{{configParam.Name}} = v;
""");
                    }
                    else if (SymbolEqualityComparer.Default.Equals(configParam.Type, NullableStringArray))
                    {
                        sb.AppendLine($$"""
data.{{configParam.Name}} = v.Split(';').Select(t => t.Trim()).ToArray();
""");
                    }
                    else if (!configParam.IsObsolete)
                    {
                        throw new System.NotImplementedException();
                    }
                }
                sb.AppendLine("}");
            }
            if (!isFirstObsolete)
            {
                sb.AppendLineRaw("#pragma warning restore CS0612");
            }

            sb.AppendLine();
            sb.AppendLine("return data.ToImmutable();");
        }
        sb.AppendLine(
"""
        }
        catch (global::System.Exception e)
        {
            throw new global::SourceExpander.ParseJsonException(e);
        }
    }
}
""");
        return sb.ToString();
    }

}
