using System.Collections.Generic;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace SourceExpander.Share
{
    public class AllTypeNamesTest
    {
        public struct TypeNameTestInput
        {
            public override string ToString() => Name;
            public string Name;
            public SemanticModel Model;
            public SyntaxTree Tree;

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

        public static IEnumerable<object[]> GetTypeNameTestData()
        {
            yield return new object[] {
                new TypeNameTestInput("Normal", @"
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
"),
                new string[]{
                    "System.Console",
                    "System.MathF",
                    "System.Exception",
                    "nint",
                    "System.IObserver<T>",
                    "System.DateTime",
                    "System.Action",
                    "System.Func<TResult>",
                    "System.SerializableAttribute",
                    "object",
                    "string",
                    "System.TimeSpan",
                    "System.Collections.Generic.List<T>",
                    "System.Linq.Enumerable",
                    "Program",
                    "int"
                },
            };
        }

        [Theory]
        [MemberData(nameof(GetTypeNameTestData))]
        public void TypeName(TypeNameTestInput input, string[] expected)
        {
            RoslynUtil.AllTypeNames(input.Model, input.Tree, default)
                .Should().BeEquivalentTo(expected);
        }
    }
}
