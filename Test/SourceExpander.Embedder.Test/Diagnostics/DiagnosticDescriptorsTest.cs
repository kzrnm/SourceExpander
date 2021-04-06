using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Xunit;

namespace SourceExpander.Embedder.Diagnostics.Test
{
    public class DiagnosticDescriptorsTest
    {
        static readonly IFormatProvider formatProvider = CultureInfo.InvariantCulture;
        [Fact]
        public void EMBED0001()
        {
            DiagnosticDescriptors.EMBED0001_UnknownError("LX")
                .GetMessage(formatProvider)
                .Should()
                .Be("Unknown error: LX");
        }
        [Fact]
        public void EMBED0002()
        {
            DiagnosticDescriptors.EMBED0002_OlderVersion(new Version(2, 0, 0), "Newerlib", new Version(3, 0, 0))
                .GetMessage(formatProvider)
                .Should()
                .Be("Embeder version(2.0.0) is older than embedder of Newerlib(3.0.0)");
        }
        [Fact]
        public void EMBED0003()
        {
            DiagnosticDescriptors.EMBED0003_ParseConfigError("/home/source/SourceExpander.Embedder.Config.json", "any error")
                .GetMessage(formatProvider)
                .Should()
                .Be("Error config file: /home/source/SourceExpander.Embedder.Config.json", "any error");
        }
        [Fact]
        public void EMBED0004()
        {
            DiagnosticDescriptors.EMBED0004_ErrorEmbeddedSource("P.cs", "any error")
                .GetMessage(formatProvider)
                .Should()
                .Be("Error embedded source: File: P.cs, Message: any error");
        }
        [Fact]
        public void EMBED0005()
        {
            DiagnosticDescriptors.EMBED0005_EmbeddedSourceDiff("Uns")
                .GetMessage(formatProvider)
                .Should()
                .Be("Different syntax: near Uns. This is Embedder error, please report this to GitHub repository.");
        }
        [Fact]
        public void EMBED0006()
        {
            DiagnosticDescriptors.EMBED0006_AnotherAssemblyEmbeddedDataError("Other", "SourceExpander.EmbeddedSourceCode", "There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.")
                .GetMessage(formatProvider)
                .Should()
                .Be("Another assembly has invalid embedded data: Other, Key: SourceExpander.EmbeddedSourceCode, Message: There was an error deserializing the object of type SourceExpander.SourceFileInfo[]. Encountered unexpected character '}'.");
        }
        [Fact]
        public void EMBED0007()
        {
            DiagnosticDescriptors.EMBED0007_NullableProject()
                .GetMessage(formatProvider)
                .Should()
                .Be("Nullable option is unsupported");
        }
        [Fact]
        public void EMBED0008()
        {
            DiagnosticDescriptors.EMBED0008_NullableDirective(Location.None)
                .GetMessage(formatProvider)
                .Should()
                .Be("Nullable directive is unsupported");
        }
        [Fact]
        public void EMBED0009()
        {
            DiagnosticDescriptors.EMBED0009_UsingStaticDirective(Location.None)
                .GetMessage(formatProvider)
                .Should()
                .Be("Avoid using static directive because there is a risk of name collision");
        }
        [Fact]
        public void EMBED0010()
        {
            DiagnosticDescriptors.EMBED0010_UsingAliasDirective(Location.None)
                .GetMessage(formatProvider)
                .Should()
                .Be("Avoid using alias directive because there is a risk of name collision");
        }
        [Fact]
        public void EMBED0011()
        {
            DiagnosticDescriptors.EMBED0011_ObsoleteConfigProperty("old-property", "instead-property")
                .GetMessage(formatProvider)
                .Should()
                .Be("Obsolete embedder config property. old-property is obsolete. Use instead-property.");
        }
    }
}
