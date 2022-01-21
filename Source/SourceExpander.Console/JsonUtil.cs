using System;
using System.IO;
using System.Text.Json;

namespace SourceExpander
{
    internal class JsonUtil
    {
        public static T ParseJson<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json)!;
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
                return JsonSerializer.Deserialize<T>(jsonStream)!;
            }
            catch (Exception e)
            {
                throw new ParseJsonException(e);
            }
        }
    }
}
