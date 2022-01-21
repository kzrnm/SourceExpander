using System;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace SourceExpander
{
    internal static partial class JsonUtil
    {
        public static JsonConverterCollection Converters { get; } = new();

        public static string ToJson<T>(T infos)
            => JsonConvert.SerializeObject(infos, new JsonSerializerSettings
            {
                Converters = Converters,
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.Default,
            });

        public static T ParseJson<T>(SourceText jsonText) => ParseJson<T>(jsonText.ToString());
        public static T ParseJson<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings
                {
                    Converters = Converters,
                })!;
            }
            catch (Exception e)
            {
                throw new ParseJsonException(e);
            }
        }
        public static T ParseJson<T>(Stream jsonStream)
        {
            try
            {
                var serializer = new JsonSerializer();
                foreach (var conv in Converters)
                    serializer.Converters.Add(conv);
                using var sr = new StreamReader(jsonStream);
                using var jsonTextReader = new JsonTextReader(sr);
                return serializer.Deserialize<T>(jsonTextReader)!;
            }
            catch (Exception e)
            {
                throw new ParseJsonException(e);
            }
        }
    }

    internal sealed class ParseJsonException : Exception
    {
        public ParseJsonException(Exception inner) : base(inner.Message, inner)
        { }
    }
}
