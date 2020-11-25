using System.Collections.Generic;
namespace SourceExpander.Expanded
{
    public class SourceCode
    {
        private SourceCode(string path, string code)
        {
            Path = path;
            Code = code;
        }
        public static SourceCode FromDictionary(Dictionary<string, object> dic)
        {
            static bool TryGet<T>(Dictionary<string, object> dic, string key, out T val)
            {
                if (dic.TryGetValue(key, out var obj) && obj is T v)
                {
                    val = v;
                    return true;
                }
                val = default;
                return false;
            }
            TryGet<string>(dic, "path", out var path);
            TryGet<string>(dic, "code", out var code);
            return new SourceCode(path, code);
        }
        public string Path { get; }
        public string Code { get; }
    }
}
