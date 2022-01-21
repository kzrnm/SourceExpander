using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Kzrnm.Convert.Base32768;
#nullable enable
namespace SourceExpander
{
    internal static class SourceFileInfoUtil
    {
        public static string ToGZipBase32768(string code)
        {
            var writeBytes = new UTF8Encoding(false).GetBytes(code);
            using var msOut = new MemoryStream();
            using (var gz = new GZipStream(msOut, CompressionMode.Compress, true))
                gz.Write(writeBytes, 0, writeBytes.Length);
            msOut.Position = 0;
            return Base32768.Encode(msOut);
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

        public static string[] SortUsings(string[] usings)
        {
            Array.Sort(usings, UsingComparer.Default);
            return usings;
        }
    }
}
