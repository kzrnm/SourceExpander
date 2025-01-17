using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceExpander;

[Generator]
public class ConfigGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            "SourceExpander.GeneratorConfigAttribute",
            (_, _) => true,
            (context, token) => context)
            .Combine(
                context.AdditionalTextsProvider
                .Where(a => a.Path.EndsWith(".props"))
                .Select((f, token) =>
                {
                    using var fs = new FileStream(f.Path, FileMode.Open, FileAccess.Read);
                    return XElement.Load(fs);
                })
                .Where(p => p.Name.LocalName == "Project")
                .SelectMany((p, token) => p.Element("ItemGroup")
                    ?.Elements("CompilerVisibleProperty")
                    ?.Attributes("Include")
                    ?.Select(e => e.Value) ?? [])
                .Collect()
                .Select((p, token) => p.ToImmutableHashSet())
            );
        context.RegisterSourceOutput(source, Emit);
    }

    static void Emit(SourceProductionContext context, (GeneratorAttributeSyntaxContext Left, ImmutableHashSet<string> Right) value)
        => Emit(context, value.Left, value.Right);

    static void Emit(
        SourceProductionContext context,
        GeneratorAttributeSyntaxContext attributeSyntaxContext,
        ImmutableHashSet<string> compilerVisibleProperties
    )
    {
        if (attributeSyntaxContext.TargetSymbol is not INamedTypeSymbol symbol) return;
        if (!symbol.GetAttributes().Any(a => a.AttributeClass?.ToString() == "SourceExpander.GeneratorConfigAttribute"))
            return;

        INamedTypeSymbol dataType;
        try
        {
            dataType = symbol.GetMembers()
                .OfType<INamedTypeSymbol>()
                .Where(t =>
                    t.GetAttributes().Any(a => a.AttributeClass?.ToString() == "System.Runtime.Serialization.DataContractAttribute")
                    && t.GetMembers()
                    .OfType<IMethodSymbol>()
                    .Any(m => m.Name == "ToImmutable"
                            && SymbolEqualityComparer.Default.Equals(m.ReturnType, symbol)))
                .Single();
        }
        catch
        {
            context.ReportDiagnostic(
                DiagnosticDescriptors.XPA0002_SingleDataType(symbol.DeclaringSyntaxReferences[0].GetSyntax(context.CancellationToken).GetLocation()));
            return;
        }

        Emit(context, attributeSyntaxContext, compilerVisibleProperties, GetParams(dataType));

        ConfigParams GetParams(INamedTypeSymbol dataType)
        {
            var builder = ImmutableArray.CreateBuilder<ConfigParam>();
            foreach (var member in dataType.GetMembers().OfType<IPropertySymbol>())
            {
                var isObsolete = member.GetAttributes().Any(a => a.AttributeClass?.ToString() == "System.ObsoleteAttribute");
                var dataMember = member.GetAttributes()
                    .FirstOrDefault(a => a.AttributeClass?.ToString() == "System.Runtime.Serialization.DataMemberAttribute");
                if (dataMember == null)
                    continue;
                if (dataMember.NamedArguments.FirstOrDefault(kv => kv.Key == "Name").Value.Value is not string jsonName)
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.XPA0003_DataMemberAttribute(member.DeclaringSyntaxReferences[0].GetSyntax().GetLocation()));
                    continue;
                }

                builder.Add(new(
                    member.Name,
                    jsonName,
                    member.Type,
                    (PropertyDeclarationSyntax)member.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken: context.CancellationToken),
                    isObsolete));
            }
            builder.Sort((a, b) => a.IsObsolete.CompareTo(b.IsObsolete));
            return new(dataType, builder.ToImmutable());
        }
    }

    static void Emit(
        SourceProductionContext context,
        GeneratorAttributeSyntaxContext attributeSyntaxContext,
        ImmutableHashSet<string> compilerVisibleProperties,
        ConfigParams configParams
    )
    {
        if (attributeSyntaxContext.SemanticModel.Compilation is not { } compilation)
            return;
        var assemblyName = $"{attributeSyntaxContext.SemanticModel.Compilation?.AssemblyName?.Replace(".", "_")}";
        if (!assemblyName.EndsWith("Roslyn3"))
        {
            foreach (var configParam in configParams)
            {
                var propertyName = $"{assemblyName}_{configParam.Name}";
                if (!configParam.IsObsolete && !compilerVisibleProperties.Contains(propertyName))
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.XPA0001_NeedsProperty(propertyName, configParam.Syntax.Identifier.GetLocation()));
                }
                if (configParam.Type.NullableAnnotation is not NullableAnnotation.Annotated)
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.XPA0004_MustBeNullable(propertyName, configParam.Syntax.Identifier.GetLocation()));
                }
            }
        }

        context.AddSource($"{attributeSyntaxContext.TargetSymbol}.Convert.Parse.g.cs",
            new ParseMethodBuilder(
                compilation,
                (INamedTypeSymbol)attributeSyntaxContext.TargetSymbol,
                configParams).Build());
    }
}
