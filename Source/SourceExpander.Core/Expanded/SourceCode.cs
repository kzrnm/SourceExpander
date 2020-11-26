using System.Collections.Generic;
#pragma warning disable IDE0018,IDE0034,IDE0038
namespace SourceExpander.Expanded
{
    public class SourceCode
    {
        public SourceCode(string path, string code)
        {
            Path = path;
            Code = code;
        }
        private static bool TryGet<T>(Dictionary<string, object> dic, string key, out T val)
        {
            object obj;
            if (dic.TryGetValue(key, out obj) && obj is T)
            {
                val = (T)obj;
                return true;
            }
            val = default(T);
            return false;
        }
        public static SourceCode FromDictionary(Dictionary<string, object> dic)
        {
            string path;
            string code;
            TryGet<string>(dic, "path", out path);
            TryGet<string>(dic, "code", out code);
            return new SourceCode(path, code);
        }
        public string Path { get; }
        public string Code { get; }
    }
}
