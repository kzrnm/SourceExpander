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
""");
        sb.AppendLine("""
    internal record Builder(
        global::Microsoft.CodeAnalysis.AdditionalText? SourceText
""");
        using (sb.Indent(2))
            foreach (var configParam in ConfigParams)
                sb.AppendLine($", string? {configParam.Name}");
        sb.AppendLine($$"""
    )
    {
        static void SetObject(ref object? field, string? value)
        {
            if(!string.IsNullOrWhiteSpace(value))
                field = value;
        }
        static void SetObject(ref string? field, string? value)
        {
            if(!string.IsNullOrWhiteSpace(value))
                field = value;
        }
        static void SetBool(ref bool field, string? value)
        {
            if(!string.IsNullOrWhiteSpace(value))
                field = !global::System.StringComparer.OrdinalIgnoreCase.Equals(value, "false");
        }
        static void SetBool(ref bool? field, string? value)
        {
            if(!string.IsNullOrWhiteSpace(value))
                field = !global::System.StringComparer.OrdinalIgnoreCase.Equals(value, "false");
        }
        static void SetStringArray(ref string[]? field, string? value)
        {
            if(!string.IsNullOrWhiteSpace(value))
                field = value.Split(';').Select(t => t.Trim()).Where(t => t is {Length: not 0}).ToArray();
        }
        public {{Symbol.Name}} Build(global::System.Threading.CancellationToken cancellationToken)
        {
            try
            {
""");
        using (sb.Indent(4))
        {
            sb.AppendLine($$"""
{{ConfigParams.Type.Name}} data = SourceText?.GetText(cancellationToken)?.ToString() switch
{
    null => new(),
    var source => global::SourceExpander.JsonUtil.ParseJson<{{ConfigParams.Type.Name}}>(source) ?? new(),
};
""");
            sb.AppendLineRaw("#pragma warning disable CS0612");
            foreach (var configParam in ConfigParams)
            {
                if (SymbolEqualityComparer.Default.Equals(configParam.Type, NullableBoolean))
                {
                    sb.AppendLine($$"""SetBool(ref data.{{configParam.Name}}, {{configParam.Name}});""");
                }
                else if (SymbolEqualityComparer.Default.Equals(configParam.Type, NullableString)
                || SymbolEqualityComparer.Default.Equals(configParam.Type, NullableObject))
                {
                    sb.AppendLine($$"""SetObject(ref data.{{configParam.Name}}, {{configParam.Name}});""");
                }
                else if (SymbolEqualityComparer.Default.Equals(configParam.Type, NullableStringArray))
                {
                    sb.AppendLine($$"""SetStringArray(ref data.{{configParam.Name}}, {{configParam.Name}});""");
                }
                else if (!configParam.IsObsolete)
                {
                    throw new System.NotImplementedException("Update ConfigAnalyzer");
                }
            }
            sb.AppendLineRaw("#pragma warning restore CS0612");
            sb.AppendLine();
            sb.AppendLine("return data.ToImmutable();");
        }
        sb.AppendLine("""
            }
            catch (global::System.Exception e)
            {
                throw new global::SourceExpander.ParseJsonException(e);
            }
        }
    }
    internal static Builder LoadBuilder(global::Microsoft.CodeAnalysis.AdditionalText? sourceText, global::Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions analyzerConfigOptions)
    {
""");
        using (sb.Indent(2))
        {
            sb.AppendLine($"const string buildPropHeader = {SyntaxFactory.Literal("build_property.")};");
            sb.AppendLine($"const string header = buildPropHeader + {SyntaxFactory.Literal(Symbol.ContainingAssembly.Name.ToString().Replace('.', '_') + "_")};");

            sb.AppendLine();
            sb.AppendLine("// properties form analyzerConfigOptions");
            sb.AppendLineRaw("#pragma warning disable CS0612");

            foreach (var configParam in ConfigParams)
            {
                sb.AppendLine($$"""
if (analyzerConfigOptions.TryGetValue(header + nameof({{ConfigParams.Type.Name}}.{{configParam.Name}}), out string? {{configParam.Name}}))
{
    {{configParam.Name}} = {{configParam.Name}}.Trim();
}
""");
            }
            sb.AppendLineRaw("#pragma warning restore CS0612");
            sb.AppendLine("return new Builder(SourceText: sourceText");
            foreach (var configParam in ConfigParams)
                sb.AppendLine($", {configParam.Name}: {configParam.Name}");
        }
        sb.AppendLine($$"""
        );
    }
}
""");
        return sb.ToString();
    }

}
