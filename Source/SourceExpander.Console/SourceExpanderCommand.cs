using System.Text.Encodings.Web;
using System.Text.Json;

internal partial class SourceExpanderCommand : ConsoleAppBase
{
    private static JsonSerializerOptions DefaultSerializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };
}
