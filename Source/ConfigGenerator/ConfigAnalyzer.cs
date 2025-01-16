using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace SourceExpander
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ConfigAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
            => DiagnosticDescriptors.SupportedDiagnostics;
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.RegisterSymbolAction(Analyze, SymbolKind.NamedType);
        }

        static void Analyze(SymbolAnalysisContext context)
        {
            if (context.IsGeneratedCode) return;
            if (context.Symbol is not INamedTypeSymbol symbol) return;
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

            static IEnumerable<XElement> GetPropsXmls(SymbolAnalysisContext context)
                => context.Options.AdditionalFiles.Where(f => f.Path.EndsWith(".props"))
                .Select(f =>
                {
                    using var fs = new FileStream(f.Path, FileMode.Open, FileAccess.Read);
                    return XElement.Load(fs);
                });
            ImmutableArray<ConfigParams> GetParams(INamedTypeSymbol dataType)
            {
                var builder = ImmutableArray.CreateBuilder<ConfigParams>();
                foreach (var member in dataType.GetMembers().OfType<IPropertySymbol>())
                {
                    if (member.GetAttributes().Any(a => a.AttributeClass?.ToString() == "System.ObsoleteAttribute"))
                        continue;
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

                    builder.Add(new(member.Name, jsonName, member.Type, (PropertyDeclarationSyntax)member.DeclaringSyntaxReferences[0].GetSyntax(cancellationToken: context.CancellationToken)));
                }
                return builder.ToImmutable();
            }

            var compilerVisibleProperties = GetPropsXmls(context)
                .Where(p => p.Name.LocalName == "Project")
                .SelectMany(p => p.Element("ItemGroup")
                    ?.Elements("CompilerVisibleProperty")
                    ?.Attributes("Include")
                    ?.Select(e => e.Value)).ToImmutableHashSet();

            foreach (var configParam in GetParams(dataType))
            {
                var propertyName = $"{context.Compilation?.AssemblyName?.Replace(".", "_")}_{configParam.Name}";
                if (!compilerVisibleProperties.Contains(propertyName))
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.XPA0001_NeedsProperty(propertyName, configParam.Syntax.Identifier.GetLocation()));
                }
            }
        }
        record ConfigParams(string Name, string JsonName, ITypeSymbol Type, PropertyDeclarationSyntax Syntax);
    }
}
