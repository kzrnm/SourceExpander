using System;

namespace SourceExpander
{
    internal sealed class ParseJsonException : Exception
    {
        public ParseJsonException(Exception inner) : base(inner.Message, inner)
        { }
    }
}
