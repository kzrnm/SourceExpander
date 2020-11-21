using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Text;
using Kzrnm.Convert.Base32768;

namespace SourceExpander
{
    public static class SourceFileInfoUtil
    {
        public static List<SourceFileInfo>? GetAttributeSourceFileInfos(KeyValuePair<string, string> attr)
        {
            var key = attr.Key;
            var val = attr.Value;
            if (!key.StartsWith("SourceExpander.EmbeddedSourceCode"))
                return null;
            var exts = new HashSet<string>(key.Substring("SourceExpander.EmbeddedSourceCode".Length).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries));
            if (exts.Contains("GZipBase32768"))
                return ParseEmbeddedJson(FromGZipBase32768Stream(val));
            return ParseEmbeddedJson(val);
        }
        private static List<SourceFileInfo> ParseEmbeddedJson(string json)
        {
            using var ms = new MemoryStream(new UTF8Encoding(false).GetBytes(json));
            return ParseEmbeddedJson(ms);
        }
        private static List<SourceFileInfo> ParseEmbeddedJson(Stream stream)
        {
            var serializer = new DataContractJsonSerializer(typeof(List<SourceFileInfo>));
            return (List<SourceFileInfo>)serializer.ReadObject(stream);
        }

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
            using var ms = FromGZipBase32768Stream(compressed);
            return new UTF8Encoding(false).GetString(ms.ToArray());
        }
        public static MemoryStream FromGZipBase32768Stream(string compressed)
        {
            using var msIn = new MemoryStream(Base32768.Decode(compressed));
            var msOut = new MemoryStream();
            using (var gz = new GZipStream(msIn, CompressionMode.Decompress))
                gz.CopyTo(msOut);
            msOut.Position = 0;
            return msOut;
        }
    }
}
