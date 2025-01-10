using System.Collections.Generic;

namespace SourceExpander
{
    /// <summary>
    /// Result of the 'dependency' subcommand
    /// </summary>
    /// <param name="FileName"></param>
    /// <param name="Dependencies"></param>
    /// <param name="TypeNames"></param>
    public record DependencyResult(string FileName, IEnumerable<string> Dependencies, IEnumerable<string> TypeNames);
}
