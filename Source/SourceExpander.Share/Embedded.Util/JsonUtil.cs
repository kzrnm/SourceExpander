using System.IO;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json;

namespace SourceExpander
{
    public static class JsonUtil
    {
        public static string ToJson<T>(T infos) => JsonConvert.SerializeObject(infos, new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            StringEscapeHandling = StringEscapeHandling.Default,
        });

        public static T ParseJson<T>(SourceText jsonText) => ParseJson<T>(jsonText.ToString());
        public static T ParseJson<T>(string json) => JsonConvert.DeserializeObject<T>(json);
        public static T ParseJson<T>(Stream jsonStream)
        {
            var serializer = new JsonSerializer();
            using var sr = new StreamReader(jsonStream);
            using var jsonTextReader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jsonTextReader)!;
        }
    }
}
