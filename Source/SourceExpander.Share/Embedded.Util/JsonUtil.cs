using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace SourceExpander
{
    public static class JsonUtil
    {
        public static string ToJson<T>(T infos)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, infos);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static T ParseJson<T>(SourceText jsonText, CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream(jsonText.Length);
            using var sw = new StreamWriter(ms, jsonText.Encoding ?? Encoding.UTF8);
            jsonText.Write(sw, cancellationToken);
            ms.Position = 0;
            return ParseJson<T>(ms);
        }

        public static T ParseJson<T>(string json)
        {
            using var ms = new MemoryStream(new UTF8Encoding(false).GetBytes(json));
            return ParseJson<T>(ms);
        }
        public static T ParseJson<T>(Stream jsonStream)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(jsonStream);
        }
    }
}
