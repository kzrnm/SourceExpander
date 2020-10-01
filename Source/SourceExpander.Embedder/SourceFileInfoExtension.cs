using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SourceExpander
{
    public static class SourceFileInfoExtension
    {
        public static string ToInitializeString(this SourceFileInfo info)
        {
            return "new SourceFileInfo{ " + string.Join(",", GetPropertiesDefine(info)) + " }";
            static IEnumerable<string> GetPropertiesDefine(SourceFileInfo info)
            {
                foreach (var property in typeof(SourceFileInfo).GetProperties())
                {
                    if (!(property.CanWrite && property.CanRead)) continue;

                    var value = property.GetValue(info) switch
                    {
                        string val => Quote(val),
                        IEnumerable<string> val => QuoteArray(val),
                        _ => throw new InvalidOperationException("invalid SourceFileInfo"),
                    };
                    yield return $"{property.Name}={value}";
                }
            }
            static string Quote(string str) => $"@\"{str.Replace("\"", "\"\"")}\"";
            static string QuoteArray(IEnumerable<string> strs) => $"new string[]{{{string.Join(",", strs.Select(Quote))}}}";
        }
    }
}
