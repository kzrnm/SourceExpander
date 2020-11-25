using System;
using System.Text;

namespace SourceExpander
{
    public static class DictionaryLiteralExtension
    {
        public static StringBuilder AppendDicElement(this StringBuilder sb, string keyLiteral, string valueLiteral)
            => sb.Append('{').Append(keyLiteral).Append(',').Append(valueLiteral).Append("},");

        public static StringBuilder AppendDicElement(this StringBuilder sb, string keyLiteral, Action<StringBuilder> appendValueLiteralAction)
        {
            sb.Append('{').Append(keyLiteral).Append(',');
            appendValueLiteralAction(sb);
            return sb.Append("},");
        }
    }
}
