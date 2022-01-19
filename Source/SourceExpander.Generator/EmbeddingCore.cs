﻿// <auto-generated>
// This code was generated by a t4. Do not change this code.
// </auto-generated>

namespace SourceExpander
{
public static class EmbeddingCore{
    public const string SourceCodeClassCode = "using System.Collections.Generic;\n#pragma warning disable IDE0018,IDE0034,IDE0038\nnamespace SourceExpander.Expanded\n{\n\tpublic class SourceCode\n\t{\n\t\tpublic SourceCode(string path, string code)\n\t\t{\n\t\t\tPath = path;\n\t\t\tCode = code;\n\t\t}\n\t\tprivate static bool TryGet<T>(Dictionary<string, object> dic, string key, out T val)\n\t\t{\n\t\t\tobject obj;\n\t\t\tif (dic.TryGetValue(key, out obj) && obj is T)\n\t\t\t{\n\t\t\t\tval = (T)obj;\n\t\t\t\treturn true;\n\t\t\t}\n\t\t\tval = default(T);\n\t\t\treturn false;\n\t\t}\n\t\tpublic static SourceCode FromDictionary(Dictionary<string, object> dic)\n\t\t{\n\t\t\tstring path;\n\t\t\tstring code;\n\t\t\tTryGet(dic, \"path\", out path);\n\t\t\tTryGet(dic, \"code\", out code);\n\t\t\treturn new SourceCode(path, code);\n\t\t}\n\t\tpublic string Path { get; }\n\t\tpublic string Code { get; }\n\t}\n}\n";
}
}