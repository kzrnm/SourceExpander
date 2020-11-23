namespace SourceExpander.Expanded
{
    public class SourceCode
    {
        public SourceCode(string path = null, string code = null)
        {
            Path = path;
            Code = code;
        }
        public string Path { get; }
        public string Code { get; }
    }
}
