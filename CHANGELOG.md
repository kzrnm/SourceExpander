# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [8.2.0] - 2025-04-18
### Changed
- SourceExpander.Embedder: Remove assembly/module Attribute

## [8.1.0] - 2025-01-20
### Changed
- SourceExpander.Embedder: Embed codes even if there are compilation errors

## [8.0.0] - 2025-01-18
### Added
- Add `auto-generated` header
- Add missing `CompilerVisibleProperty`
### Changed
- Update SourceExpander.Console
- Use `global::` in generated code
- **breaking change** SourceExpander.Embedder: Deprecate `EmbeddingSourceClass` in the settings and introduce `EmbeddingSourceClassName`
- **breaking change** SourceExpander.Embedder: Deprecate `ExpandingSymbol` in the settings and introduce `ExpandInLibrary`

## [7.0.0] - 2024-01-30
### Added
- SourceExpander.Generator: Add `unsafe block` warning
- SourceExpander.Generator: Remove `Needs AllowUnsafeBlocks` warning

## [6.0.0] - 2024-01-30
### Added
- SourceExpander.Embedder: Add include/exclude to config

### Changed
- Update libraries
- Accept nullable

### Removed
- Remove SourceExpander.Generating.Common
- Remove diagnostics EMBED0007/EMBED0008

## [5.5.1] - 2023-10-22
### Changed
- SourceExpander.Generator:Add Testing namespace to ExpandingAll

## [5.5.0] - 2023-10-22
### Added
- SourceExpander.Generator:Add ExpandingAll

## [5.4.0] - 2023-10-19
### Added
- SourceExpander.Generator: Add ExpandingPosition

## [5.3.1] - 2023-09-12
### Changed
- SourceExpander.Embedder: Fix `using` in codes expanded by ExpandingSymbol

## [5.3.0] - 2023-09-12
### Added
- SourceExpander.Embedder: Add ExpandingSymbol config
- Update libraries

## [5.2.0] - 2023-01-21
### Added
- SourceExpander.Generator: Add IgnoreAssemblies config

## [5.1.0] - 2022-12-17
### Added
- Accept file scoped namespace

## [5.0.0] - 2022-03-17
### Added
- Generator configuration by build property
- Added EmbeddingFileNameType config
- SourceExpander.Console: Resolve dependency
- SourceExpander.Console: Show embedded libraries list
- SourceExpander.Console: Expand all

## [4.1.1] - 2022-02-26
### Changed
- naminodarie → kzrnm

## [4.1.0] - 2022-02-26
### Added
- Embed defined namespace
- Expand imported but unused namespaces

## [4.0.2] - 2022-01-23
- Downgrade libraries SourceExpander.Embedder.Testing

## [4.0.1] - 2022-01-23
### Added
- SourceExpander.Generating.Common
### Removed
- Remove unused dependency from SourceExpander.Embedder

## [4.0.0] - 2022-01-22
### Added
- Source Generator V2: Incremental Generetor
- SourceExpander.Console

## [3.2.1] - 2021-10-20
### Changed
- Replace github.com/kzrnm with github.com/kzrnm

## [3.2.0] - 2021-06-02
### Changed
- New feature: MetadataExpandingFile

## [3.1.2] - 2021-05-03
### Changed
- Add file path to EMBED0011
- Update libraries
- Add call of CancellationToken.ThrowIfCancellationRequested

## [3.1.1] - 2021-04-06
### Added
- SourceExpander.Embedder, SourceExpander.Generator: Add config file path to diagnostics

## [3.1.0] - 2021-04-06
### Added
- SourceExpander.Embedder: Add `minify-level` to Embedder config
- SourceExpander.Embedder: Add ObsoleteConfigProperty diagnostic
### Removed
- SourceExpander.Embedder: Remove `enable-minify` from Embedder config

## [3.0.2] - 2021-04-03
### Changed
- Change error message
- Optimize SourceFileContainer

## [3.0.0] - 2021-03-21
### Added
- SourceExpander.Embedder: Add NotEmbeddingSourceAttribute
- SourceExpander.Embedder.Analyzer: Warning on expanding embedded source
- Localize diagnostics
### Changed
- SourceExpander.Embedder: Format EmbbedingSourceClass
- SourceExpander.Embedder: Ignore compilation error
### Removed
- Obsolete SourceExpander.Embedder.Analyzer

## [2.6.0] - 2021-02-04
### Added

- SourceExpander.Generator: static emmbeding text

## [2.5.0] - 2020-12-18
### Added

- Remove unused types in using static
- GenerateDocumentationFile
- SourceExpander.Embedder.Testing: Add Properties
- SourceExpander.Embedder: Embedding source as class
- SourceExpander.Embedder.Testing: Utility for Testing

### Changed

- Change Diagnostics level.
- SemanticModel ignore accessibility.

## [2.4.0] - 2020-12-16
### Added

- Validate embedded source code.
- [Experimental]Minify embedded source code.
- Analyze nullable.

## [2.3.4] - 2020-12-14
### Changed
- SourceExpander.Embedder: Fix regression bug. Remove SytaxTrivia

## [2.3.3] - 2020-12-13
### Changed
- Add GitHub link to Expand Code.

## [2.3.2] - 2020-12-13
### Changed
- Update Base32768 from 1.0.7 to 1.0.8.

## [2.3.1] - 2020-12-13
### Changed
- Report EMBEDDER0001, EMBEDDER0002 only top level using directive.

## [2.3.0] - 2020-12-13
### Added

- Add generator config file.
- SourceExpander.Embedder.Analyzer
