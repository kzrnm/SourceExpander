using System;
using System.Linq;
using System.Runtime.Serialization;
#pragma warning disable CA1819
namespace SourceExpander
{
    [DataContract]
    public class EmbedderConfig
    {
        public EmbedderConfig(string[]? excludeAttributes = null)
        {
            ExcludeAttributes = excludeAttributes?.ToArray() ?? Array.Empty<string>();
        }

        [DataMember(Name = "exclude-attributes")]
        public string[] ExcludeAttributes { set; get; }
    }
}
