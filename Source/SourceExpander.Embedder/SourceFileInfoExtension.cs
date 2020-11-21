using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SourceExpander
{
    public static class SourceFileInfoExtension
    {
        public static string ToJson(this IEnumerable<SourceFileInfo> infos)
        {
            var serializer = new DataContractJsonSerializer(typeof(IEnumerable<SourceFileInfo>));
            using var ms = new MemoryStream();
            serializer.WriteObject(ms, infos);
            return Encoding.UTF8.GetString(ms.ToArray());
        }
    }
}
