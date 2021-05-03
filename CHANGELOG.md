# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
