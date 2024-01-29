; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 6.0.0

### Removed Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EMBED0007 | Usage | Warning | Nullable option is unsupported
EMBED0008 | Usage | Warning | Nullable directive is unsupported

## Release 3.1.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EMBED0011 | Config | Warning | DiagnosticDescriptors

## Release 3.0.0

### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EMBED0001 | Error | Warning | Unknown error
EMBED0002 | Usage | Warning | Embeder version is older
EMBED0003 | Error | Error | Error config file
EMBED0004 | Error | Warning | Error embedded source
EMBED0005 | Error | Error | Different syntax
EMBED0006 | Error | Warning | Another assembly has invalid embedded data
EMBED0007 | Usage | Warning | Nullable option is unsupported
EMBED0008 | Usage | Warning | Nullable directive is unsupported
EMBED0009 | Usage | Info | Avoid using static directive
EMBED0010 | Usage | Info | Avoid using alias directive
