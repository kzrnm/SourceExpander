using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Testing;

namespace SourceExpander.Generate;

public class LibraryImportTest : EmbedderGeneratorTestBase
{
    [Fact]
    public async Task Generate()
    {
        var embeddedNamespaces = ImmutableArray<string>.Empty;
        var embeddedFiles = ImmutableArray.Create(
             new SourceFileInfo
             (
                 "TestProject>UnixConsole.cs",
                 ["UnixConsole"],
                 ImmutableArray.Create(
                     "using Microsoft.Win32.SafeHandles;",
                     "using System;",
                     "using System.Runtime.InteropServices;"),
                 ImmutableArray<string>.Empty,
                 """public static partial class UnixConsole{public static int Write(ReadOnlySpan<byte>buffer){if(_init){SystemNative_InitializeTerminalAndSignalHandling();_init=false;}return SystemNative_Write(OUT,buffer,buffer.Length);}public static int Read(Span<byte>buffer)=>SystemNative_Read(IN,buffer,buffer.Length);static bool _init=true;const string libNative="libSystem.Native";static readonly SafeFileHandle IN=new(0,false);static readonly SafeFileHandle OUT=new(1,false);[LibraryImport(libNative)]private static partial int SystemNative_Read(SafeHandle fd,Span<byte>buffer,int count);[LibraryImport(libNative)]private static partial int SystemNative_Write(SafeHandle fd,ReadOnlySpan<byte>buffer,int count);[LibraryImport(libNative)]private static partial int SystemNative_InitializeTerminalAndSignalHandling();}"""
             ));

        const string embeddedSourceCode = "[{\"CodeBody\":\"public static partial class UnixConsole{public static int Write(ReadOnlySpan<byte>buffer){if(_init){SystemNative_InitializeTerminalAndSignalHandling();_init=false;}return SystemNative_Write(OUT,buffer,buffer.Length);}public static int Read(Span<byte>buffer)=>SystemNative_Read(IN,buffer,buffer.Length);static bool _init=true;const string libNative=\\\"libSystem.Native\\\";static readonly SafeFileHandle IN=new(0,false);static readonly SafeFileHandle OUT=new(1,false);[LibraryImport(libNative)]private static partial int SystemNative_Read(SafeHandle fd,Span<byte>buffer,int count);[LibraryImport(libNative)]private static partial int SystemNative_Write(SafeHandle fd,ReadOnlySpan<byte>buffer,int count);[LibraryImport(libNative)]private static partial int SystemNative_InitializeTerminalAndSignalHandling();}\",\"Dependencies\":[],\"FileName\":\"TestProject>UnixConsole.cs\",\"TypeNames\":[\"UnixConsole\"],\"Usings\":[\"using Microsoft.Win32.SafeHandles;\",\"using System;\",\"using System.Runtime.InteropServices;\"]}]";

        var test = new Test
        {
            CompilationOptions = new(OutputKind.DynamicallyLinkedLibrary, allowUnsafe: true),
            TestState =
                {
                    AdditionalFiles = { enableMinifyJson },
                    Sources = {
                        (
                            "/home/source/UnixConsole.cs",
                            """
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
public static partial class UnixConsole
{
    public static int Write(ReadOnlySpan<byte> buffer)
    {
        if (_init)
        {
            SystemNative_InitializeTerminalAndSignalHandling();
            _init = false;
        }
        return SystemNative_Write(OUT, buffer, buffer.Length);
    }
    public static int Read(Span<byte> buffer)
        => SystemNative_Read(IN, buffer, buffer.Length);

    static bool _init = true;
    const string libNative = "libSystem.Native";
    static readonly SafeFileHandle IN = new(0, false);
    static readonly SafeFileHandle OUT = new(1, false);
    [LibraryImport(libNative)]
    private static partial int SystemNative_Read(SafeHandle fd, Span<byte> buffer, int count);
    [LibraryImport(libNative)]
    private static partial int SystemNative_Write(SafeHandle fd, ReadOnlySpan<byte> buffer, int count);
    [LibraryImport(libNative)]
    private static partial int SystemNative_InitializeTerminalAndSignalHandling();
}
"""
                        ),
                    },
                    ExpectedDiagnostics =
                    {
                        DiagnosticResult.CompilerError("CS8795").WithSpan("/home/source/UnixConsole.cs", 23, 32, 23, 49).WithArguments("UnixConsole.SystemNative_Read(System.Runtime.InteropServices.SafeHandle, System.Span<byte>, int)"),
                        DiagnosticResult.CompilerError("CS8795").WithSpan("/home/source/UnixConsole.cs", 25, 32, 25, 50).WithArguments("UnixConsole.SystemNative_Write(System.Runtime.InteropServices.SafeHandle, System.ReadOnlySpan<byte>, int)"),
                        DiagnosticResult.CompilerError("CS8795").WithSpan("/home/source/UnixConsole.cs", 27, 32, 27, 80).WithArguments("UnixConsole.SystemNative_InitializeTerminalAndSignalHandling()"),
                    },
                    GeneratedSources =
                    {
                        (typeof(EmbedderGenerator), "EmbeddedSourceCode.Metadata.cs",$"""
                        // <auto-generated/>
                        #pragma warning disable
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedAllowUnsafe","true")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbedderVersion","{EmbedderVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedLanguageVersion","{EmbeddedLanguageVersion}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedNamespaces","{string.Join(",", embeddedNamespaces)}")]
                        [assembly: global::System.Reflection.AssemblyMetadataAttribute("SourceExpander.EmbeddedSourceCode",{embeddedSourceCode.ToLiteral()})]
                        
                        """
                        ),
                    }
                }
        };
        await test.RunAsync();
        Newtonsoft.Json.JsonConvert.DeserializeObject<SourceFileInfo[]>(embeddedSourceCode)
            .ShouldBeEquivalentTo(embeddedFiles);
        System.Text.Json.JsonSerializer.Deserialize<SourceFileInfo[]>(embeddedSourceCode)
            .ShouldBeEquivalentTo(embeddedFiles);
    }
}
