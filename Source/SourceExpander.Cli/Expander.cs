using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Json;
using System.Text;
using SourceExpander.Expanders;

namespace SourceExpander
{
    public abstract class Expander
    {
        protected SourceFileContainer SourceFileContainer { get; }
        protected Expander(SourceFileContainer sourceFileContainer)
        {
            SourceFileContainer = sourceFileContainer;
        }

        private static IEnumerable<SourceFileInfo> GetEmbeddedSourceFiles()
             => AppDomain.CurrentDomain.GetAssemblies()
             .SelectMany(a => a.GetCustomAttributes<AssemblyMetadataAttribute>())
             .SelectMany(ParseEmbeddedJson);

        private static IEnumerable<SourceFileInfo> ParseEmbeddedJson(AssemblyMetadataAttribute metadata)
        {
            if (metadata.Key != "SourceExpander.EmbeddedSourceCode")
                return Array.Empty<SourceFileInfo>();
            using var ms = new MemoryStream(new UTF8Encoding(false).GetBytes(metadata.Value));
            var serializer = new DataContractJsonSerializer(typeof(List<SourceFileInfo>));
            return (List<SourceFileInfo>)serializer.ReadObject(ms);
        }

        public static Expander Create(string code, ExpandMethod expandMethod)
            => Create(code, expandMethod, GetEmbeddedSourceFiles().Append(s_expanderFileInfo));
        internal static Expander Create(string code, ExpandMethod expandMethod, IEnumerable<SourceFileInfo> sourceFileInfos)
            => expandMethod switch
            {
                ExpandMethod.All => new AllExpander(code, new SourceFileContainer(sourceFileInfos)),
                ExpandMethod.NameSyntax => new SimpleMatchExpander(code, new SourceFileContainer(sourceFileInfos)),
                ExpandMethod.Strict => new CompilationExpander(code, new SourceFileContainer(sourceFileInfos)),
                _ => throw new InvalidEnumArgumentException(nameof(expandMethod), (int)expandMethod, expandMethod.GetType()),
            };

        public abstract IEnumerable<string> ExpandedLines();
        public string ExpandedString() => string.Join(Environment.NewLine, ExpandedLines());

        public static void Expand([CallerFilePath] string? inputFilePath = null, string? outputFilePath = null, ExpandMethod expandMethod = ExpandMethod.All)
        {
            if (inputFilePath == null) throw new ArgumentNullException(nameof(inputFilePath));
            var inputFileInfo = new FileInfo(inputFilePath);

            if (!inputFileInfo.Exists) throw new ArgumentException($"Not found: {inputFilePath}", nameof(inputFilePath));

            if (outputFilePath == null)
            {
                var dir = inputFileInfo.DirectoryName;
                if (dir == null) throw new ArgumentException("invalid path", nameof(inputFilePath));
                outputFilePath = Path.Combine(dir, "Combined.csx");
            }
            var code = File.ReadAllText(inputFilePath);
            var expander = Create(code, expandMethod);
            File.WriteAllLines(outputFilePath, expander.ExpandedLines());
        }


        internal static readonly SourceFileInfo s_expanderFileInfo
            = new SourceFileInfo
            {
                FileName = "<Expander>",
                TypeNames = new string[] { nameof(SourceExpander) + "." + nameof(Expander) },
                Usings = new string[] { "using System.Diagnostics;", },
                Dependencies = Array.Empty<string>(),
                CodeBody = "namespace " +
             nameof(SourceExpander) +
             " { public static class " +
             nameof(Expander) +
             " {[Conditional(\"DEBUG\")] public static void " +
             "Expand(string inputFilePath = \"\", " +
             " string outputFilePath = \"\", " +
             nameof(ExpandMethod) + " expandMethod = " + nameof(ExpandMethod) + "." + nameof(ExpandMethod.All) + ") { } } " +
             "public enum " +
             nameof(ExpandMethod) + " { " + string.Join(", ", typeof(ExpandMethod).GetEnumNames()) + " } }"
            };
    }
}
