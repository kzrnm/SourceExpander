using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
namespace SourceExpander;

public static class DiagnosticDescriptors
{
    public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [
        XPA0001_NeedsProperty_Descriptor,
        XPA0002_SingleDataType_Descriptor,
        XPA0003_DataMemberAttribute_Descriptor,
    ];

    public static Diagnostic XPA0001_NeedsProperty(string name, Location location)
         => Diagnostic.Create(XPA0001_NeedsProperty_Descriptor, location, name);
    private static readonly DiagnosticDescriptor XPA0001_NeedsProperty_Descriptor = new(
        "XPA0002",
        "Needs CompilerVisibleProperty",
        "Needs <CompilerVisibleProperty Include=\"{0}\"",
        "Error",
        DiagnosticSeverity.Error,
        true);

    public static Diagnostic XPA0002_SingleDataType(Location location)
         => Diagnostic.Create(XPA0002_SingleDataType_Descriptor, location);
    private static readonly DiagnosticDescriptor XPA0002_SingleDataType_Descriptor = new(
        "XPA0002",
        "Config must have exactly one data type",
        "Config must have exactly one data type",
        "Error",
        DiagnosticSeverity.Error,
        true);
    public static Diagnostic XPA0003_DataMemberAttribute(Location location)
         => Diagnostic.Create(XPA0003_DataMemberAttribute_Descriptor, location);
    private static readonly DiagnosticDescriptor XPA0003_DataMemberAttribute_Descriptor = new(
        "XPA0003",
        "Needs System.Runtime.Serialization.DataMemberAttribute",
        "Needs System.Runtime.Serialization.DataMemberAttribute",
        "Error",
        DiagnosticSeverity.Error,
        true);
}
