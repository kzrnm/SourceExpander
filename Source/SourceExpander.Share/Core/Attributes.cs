using System;
using System.Diagnostics;

namespace SourceExpander;

[Conditional("COMPILE_ONLY")]
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal sealed class GeneratorConfigAttribute : Attribute;
