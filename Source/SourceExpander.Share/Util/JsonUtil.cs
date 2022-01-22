#if NETCOREAPP3_0_OR_GREATER
#define SYSTEM_TEXT_JSON
#endif
using System;
using System.IO;
using Microsoft.CodeAnalysis.Text;
#if SYSTEM_TEXT_JSON
using System.Text.Json;
#else
using Newtonsoft.Json;
#endif

namespace SourceExpander
{
    internal static class JsonUtil
    {
        public static string ToJson<T>(T infos)
#if SYSTEM_TEXT_JSON
            => JsonSerializer.Serialize(infos, new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            });
#else
            => JsonConvert.SerializeObject(infos, new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.Default,
            });
#endif

        public static T? ParseJson<T>(SourceText jsonText) => ParseJson<T>(jsonText.ToString());
        public static T? ParseJson<T>(string json)
        {
            try
            {
#if SYSTEM_TEXT_JSON
                return JsonSerializer.Deserialize<T>(json);
#else
                return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { });
#endif
            }
            catch (Exception e)
            {
                throw new ParseJsonException(e);
            }
        }
        public static T? ParseJson<T>(Stream jsonStream)
        {
            try
            {
#if SYSTEM_TEXT_JSON
                return JsonSerializer.Deserialize<T>(jsonStream);
#else
                var serializer = new JsonSerializer();
                using var sr = new StreamReader(jsonStream);
                using var jsonTextReader = new JsonTextReader(sr);
                return serializer.Deserialize<T>(jsonTextReader);
#endif
            }
            catch (Exception e)
            {
                throw new ParseJsonException(e);
            }
        }
    }
}
