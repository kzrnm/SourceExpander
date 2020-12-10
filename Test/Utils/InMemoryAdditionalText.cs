using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    internal class InMemoryAdditionalText : AdditionalText
    {
        public InMemoryAdditionalText(string path, string source)
        {
            Path = path;
            sourceText = SourceText.From(source, Encoding.UTF8);
        }
        public override string Path { get; }
        private readonly SourceText sourceText;
        public override SourceText GetText(CancellationToken cancellationToken = default) => sourceText;
    }
}
