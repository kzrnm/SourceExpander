using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace SourceExpander.Share
{
    public class AllTypeNamesTest
    {
        public readonly struct TypeNameTestInput
        {
            public override string ToString() => Name;
            public readonly string Name;
            public readonly SemanticModel Model;
            public readonly SyntaxTree Tree;

            public TypeNameTestInput(string name, string sourceCode)
            {
                Name = name;
                Tree = CSharpSyntaxTree.ParseText(sourceCode);
                var compilation = CSharpCompilation.Create(
                    assemblyName: "TestAssembly",
                    syntaxTrees: new[] { Tree },
                    references: TestUtil.defaultMetadatas,
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                Model = compilation.GetSemanticModel(Tree);
            }
        }

        public static IEnumerable<Func<(TypeNameTestInput input, string[] expected)>> GetTypeNameTestData()
        {
            yield return () => (
                new TypeNameTestInput("Normal", """
                using System;
                using System.Linq;
                using static System.Console;
                using Math = System.MathF;

                class MyException : Exception { }
                [System.Serializable]
                class Program
                {
                    IntPtr field;
                    IObserver<DateTime> Property { get; set; }
                    public Action Method(Func<object> func)
                    {
                        var variable = DateTime.Now.TimeOfDay.ToString();
                        variable.Select(c => c - 1).ToList().ToArray();
                        return () => func();
                    }
                }
                """),
                [
                    "System.Console",
                    "System.MathF",
                    "System.Exception",
                    "System.SerializableAttribute",
                    "nint",
                    "System.IObserver<T>",
                    "System.DateTime",
                    "System.Action",
                    "System.Func<TResult>",
                    "object",
                    "string",
                    "System.TimeSpan",
                    "System.Collections.Generic.List<T>",
                    "System.Linq.Enumerable",
                    "Program",
                    "int"
                ]
            );
        }

        [Test]
        [MethodDataSource(nameof(GetTypeNameTestData))]
        public void TypeName(TypeNameTestInput input, string[] expected)
        {
            RoslynUtil.AllTypeNames(input.Model, input.Tree, TestContext.Current!.Execution.CancellationToken).ShouldBe(expected);
        }
    }
}
