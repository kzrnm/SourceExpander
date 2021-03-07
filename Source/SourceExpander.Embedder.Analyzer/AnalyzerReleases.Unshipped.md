; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/master/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md
### New Rules
Rule ID | Category | Severity | Notes
--------|----------|----------|-------
EMBEDDER0005 | Compilation | Warning | Embedded code cannot be used in the same assembly

### Changed Rules
Rule ID | New Category | New Severity | Old Category | Old Severity | Notes
--------|--------------|--------------|--------------|--------------|-------
EMBEDDER0001 | Using Directive | Info | Using Directive | Warning | Embedded code cannot be used in the same assembly