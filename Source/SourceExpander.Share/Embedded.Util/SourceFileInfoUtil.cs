using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Kzrnm.Convert.Base32768;
#nullable enable
namespace SourceExpander
{
    public static class SourceFileInfoUtil
    {
        public static string ToGZipBase32768(string code)
        {
            using var msIn = new MemoryStream(new UTF8Encoding(false).GetBytes(code));
            using var msOut = new MemoryStream();
            using (var gz = new GZipStream(msOut, CompressionMode.Compress))
                msIn.CopyTo(gz);
            return Base32768.Encode(msOut.ToArray());
        }
        public static string FromGZipBase32768(string compressed)
        {
            using var ms = FromGZipBase32768ToStream(compressed);
            return new UTF8Encoding(false).GetString(ms.ToArray());
        }
        public static MemoryStream FromGZipBase32768ToStream(string compressed)
        {
            using var msIn = new MemoryStream(Base32768.Decode(compressed));
            var msOut = new MemoryStream();
            using (var gz = new GZipStream(msIn, CompressionMode.Decompress))
                gz.CopyTo(msOut);
            msOut.Position = 0;
            return msOut;
        }
#if NETSTANDARD2_0
        internal static List<SourceFileInfo> ParseEmbeddedJson(string json)
        {
            using var ms = new MemoryStream(new UTF8Encoding(false).GetBytes(json));
            return ParseEmbeddedJson(ms);
        }
        internal static List<SourceFileInfo> ParseEmbeddedJson(Stream stream)
        {
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(typeof(List<SourceFileInfo>));
            return (List<SourceFileInfo>)serializer.ReadObject(stream);
        }
#else
        internal static List<SourceFileInfo> ParseEmbeddedJson(string json)
            => System.Text.Json.JsonSerializer.Deserialize<List<SourceFileInfo>>(json);

        internal static List<SourceFileInfo> ParseEmbeddedJson(Stream stream)
            => System.Text.Json.JsonSerializer.DeserializeAsync<List<SourceFileInfo>>(stream).Result;
#endif
    }
}
