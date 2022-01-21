using System;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace SourceExpander
{
    internal static class JsonUtil
    {
        public static string ToJson<T>(T infos)
            => JsonConvert.SerializeObject(infos, new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                StringEscapeHandling = StringEscapeHandling.Default,
            });

        public static T ParseJson<T>(SourceText jsonText) => ParseJson<T>(jsonText.ToString());
        public static T ParseJson<T>(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { })!;
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
}
