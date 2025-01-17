#pragma warning disable IDE0001
namespace SourceExpander.Expanded
{
    public class SourceCode
    {
        public SourceCode(string path, string code)
        {
            Path = path;
            Code = code;
        }
        private static bool TryGet<T>(global::System.Collections.Generic.Dictionary<string, object> dic, string key, out T val)
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
        public static SourceCode FromDictionary(global::System.Collections.Generic.Dictionary<string, object> dic)
        {
            string path;
            string code;
            TryGet(dic, "path", out path);
            TryGet(dic, "code", out code);
            return new SourceCode(path, code);
        }
        public string Path { private set; get; }
        public string Code { private set; get; }
    }
}
