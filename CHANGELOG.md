# Change Log - Poor Man's T-SQL Formatter

This changelog aims to follow the structure laid out at [Keep a Changelog](http://keepachangelog.com/), and follow semantic versioning.

## [Unreleased]

### Added 

* "Version Bump" script, for simplifying the release process

### Changed

* Merge Pull Request #188
    * Enhancements to Appveyor CI build, support Pure .Net targets / configuration
    * Fixes to test of cmdline formatter
    * Enhancement to JS output copy to WebDemo
* Simplify JS generation: single file, no reflection data, split debug vs release, with sourcemap in debug only

## [1.6.11] - 2017-11-24

### Added 

* Appveyor build configuration, as per proposal in Pull Request #178 from chcg

### Changed

* Fixed newly split SSMS Extension Installer to set correct registry key at install time (GitHub Issue #187) 
* Restructured changelog to follow [Keep a Changelog](http://keepachangelog.com/)

## [1.6.10] - 2017-11-21

### Added 

* New Atom Editor plugin - in separate project https://github.com/TaoK/poor-mans-t-sql-formatter-atom-package
* New Visual Studio Code plugin - in separate project https://github.com/TaoK/poor-mans-t-sql-formatter-vscode-extension

### Changed

* Fixed rare negative exponent problem, GitHub issue #142
* Fixed "name" keyword detection issue, as per GitHub pull request #141 ("name" is no longer a keyword, "names" now is)
* Fixed recently-introduced issue with "noformat" regions, GitHub Issue #182
* Fixed longstanding formatting consistency issues with noformat and minimize, GitHub issues #183, #184, #185
* Split VS and SSMS VSIX packages to avoid errors & confusion with marketplace VSIX install on SSMS

## [1.6.6] - 2017-11-05

### Added 

* New JS library in NPM: https://www.npmjs.com/package/poor-mans-t-sql-formatter
* New JS-based command-line formatting utility for unixey environmentss on NPM: https://www.npmjs.com/package/poor-mans-t-sql-formatter-cli

### Changed

* Fixed cosmetic "Argument Error" under specific circumstances in VS and SSMS, GitHub Issue #168
* Fixed undo stack issue in SSMS / VS, GitHub Issue #120
* Fixed "format selection always adds extra newlines" issue (untracked)
* Fixed SSMS/VS/NPP plugin translation files / fix outrageous layout glitches in translated forms, GitHub Issue #41
* Fixed tab order in demo app and plugin settings form, GitHub Issue #177

## [1.6.3] - 2017-10-17

### Added 

* Support for Visual Studio 2013-2017, with VSIX package on the Visual Studio Marketplace: https://marketplace.visualstudio.com/vsgallery/8ac2424b-80c9-431b-9049-6e7faa70c244
* Proper support for SSMS 2014-17 with the Extension Installer for SSMS
* Long-term supportability of older VS and SSMS versions with new Add-In installer
* Notepad++ 64-bit package made available for download

## [1.6.2] - 2017-08-13

### Changed

* minor bugfixes to JS library

## [1.6.1] - 2017-07-28

### Added 

* Introduce Bridge.Net for JS transpiling (refactor out use of System.Xml)
* Introduce JS-based demo page
* JS-based formatting on poorsql.com - faster, more reliable, and works offline (AppCache)
* Support for modern Visual Studio Package system (required for Visual Studio 2014 and later)
* Support/workaround for SSMS VSPackage-loading system (required for Sql Server Management Studio 2016 and later)

### Changed

* Changed base IDE to VS2015 Community Edition
* Change DllExport library from unversioned inclusion to NuGet package (UnmanagedExports) for NPP Plugin
* Patched NPP Plugin helper to support x64 (https://github.com/kbilsted/NotepadPlusPlusPluginPack.Net/pull/19)
* Update build configurations to clarify DotNet vs x86 vs x64
* (in retrospect) broke "[noformat]" and "[minify]" region handling 
* SqlTree change: is no longer an XML document! New lighter-weight structure supports JS transpiling. **SEMANTIC VERSIONING VIOLATION**


## [1.5.3] - 2013-10-23

### Changed

* Github Issue #109: (Fix to Github Issue #86): Visual Studio 2012 support was never tested before release, and turned out to be broken

## [1.5.2] - 2013-10-22

### Changed

* Github Issue #107: [Bugfix] Unexplained install failures on SOME machines [thanks Jerrad Elmore for the bug report]

## [1.5.1] - 2013-10-20

### Added

* Github Issue #87: [Enhancement] Added support for SQLite bit-shift and c-style equality operators [thanks Tom Holden for the request]
* Github Issue #86: [Enhancement] Added support for Visual Studio 2012 [thanks Jarred Cleem for the request]
* Github Issue #81: [Enhancement] Added support for C-Style comments ("//") and colon-prefixed parameter/host-variable names for other SQL dialects [thanks Paul Toms for the request]
* Github Issue #51: [Enhancement] Added support for the OUT synonym of OUTPUT (for arguments), and corresponding keyword standardization support
* Github Issue #49: [Enhancement] Added options to control line spacing between clauses and statements [thanks to Farzad Jalali, Sheldon Hull and Benjamin Solomon for the request]

### Changed

* Github Issue #85: [Enhancement] Merged code cleanup changes by Timothy Klenke
* Github Issue #90: [Enhancement] Merged more code cleanup / options simplification changes by Timothy Klenke
* Github Issue #47: [Enhancement] Separated option for expansion of IN lists [thanks to a number of people for the request: smartkiwi, defrek, Marvin Eads and Bill Ruehle]
* Github Issue #95: [Enhancement] Corrected display of chained ELSE IF statements to be more cross-language-standard [thanks to steelwill for the request]
* Github Issue #100: [Bugfix] Corrected crash when the string for format ends with Greater Than or Less Than signs
* Github Issue #97: [Bugfix] Parsing error when single uppercase N is followed by a non-word character [thanks to derekfrye for the bug report]
* Github Issue #91: [Bugfix] Incorrect formatting of "*=" operator [thanks to ralfkret for the bug report]
* Github Issue #96: [Enhancement] Enhanced SSMS, VS and NPP plugins to keep cursor at the approximate previous location when reformatting whole document [thanks to Paul for the request]

## [1.4.4] - 2012-11-11

### Added

* Github Issue #16: [New Feature] Add Visual Studio support to the Add-In

### Changed

* Github Issue #79: [BugFix] Fix SSMS Add-In to work in unexpected languages [thanks Paulo Stradioti for the bug report]

## [1.4.3] - 2012-09-09

### Added

* Github Issue #75: [Enhancement] Add parse error highlighting in HTML output [thanks Jeff Clark for the suggestion]

## [1.4.2] - 2012-09-09

### Changed

* Github Issue #70: [Enhancement] Eliminate "Client Framework"-incompatible dependency on System.Web [thanks to Brad Wood and Richard King for feedback on this]

## [1.4.1] - 2012-09-02

### Added

* Github Issue #54: [New Feature] Support for DB2/SQLLite string concatenation operator

### Changed

* Github Issue #48: [Bugfix] Correction to SQL 2012 add-in install folder 
* Github Issue #52: [Bugfix] Fix to space-indent saving in formatting plugin options (SSMS, Notepad++)
* [Bugfix] Fix examples in commandline help to correspond to existing options [thanks John Landmesser for the bug report]
* Github Issue #74: [Bugfix] Correct parsing & formatting of subqueries & function calls in variable initializers
* Github Issue #73: [Bugfix] Correct parsing & formatting of OPTION clauses
* Github Issue #55: [Bugfix] Correct parsing & formatting of OUTPUT specifiers on proc arguments defined without parens
	

## [1.3.1] - 2012-03-12

### Added

* Github Issue 23: [New Feature] Support for SSMS 2012 / Denali
* Github Issue 34: [New Feature] Obfuscating formatter / minifier in Winforms app and Web Service
* Github Issue 36: [New Feature] Support for "[noformat][/noformat]" and "[minify][/minify]" block-formatting instructions in standard formatter [thanks dquille for the request]

### Changed

* Github Issue 37: [Bugfix] SSMS Hotkey binding failed in non-english versions of SSMS [thanks Philipp Schornberg for the bug report]
* Github Issue 38: [Bugfix] Comment-positioning bugs, one of which could result in invalid output SQL! [thanks andywuest for the bug report]
* Github Issue 39: [Bugfix] Formatting Library not usable in .Net 3.5 and later projects, because of Linqbridge conflict on Linq namespace [thanks Sean Bornstein for the bug report]
* Github Issue 40: [Bugfix] Error parsing WITH options containing "ON" in parentheses [thanks Jean-Luc Mellet for the bug report]

## [1.2.1] - 2012-01-29

### Added

* Github Issue 32: [Feature Request] Enhanced Cmdline Utility to support pipeline input (and output) [thanks William Lin for the request]
    * but default behaviour unchanged, still expect to act on files directly unless piped input provided
    * expected encoding is UTF-8... in powershell, for example, run "$OutputEncoding = [System.Text.Encoding]::UTF8" first.
* GitHub Issue 31: Completed Notepad++ Plugin, with formatting options - available trough Notepad++ plugin manager.

### Changed

* Enhanced Cmdline Utility "no files found" message to include extensions detail
* Github Issue 26: Testing enhancements:
    * Changed testing framework to Nunit, included dependencies for building and testing in any environment
    * Simplified testing code, with test sources
    * Added tests for different formatting options / flags, greater coverage


## [1.1.1] - 2012-01-21

### Added

* Translated programs, ssms plugin + website into French and Spanish 
    * thanks to Threeplicate for Amanuens, a free (for open-source projects) localization tool that makes managing translations much easier!
    * and thanks to my wife for proofreading... technical translations are always a mess, but at least I got a second pair of eyes on it!
* Initial Beta of Notepad++ plugin [thanks UFO and Robert Giesecke for the .Net managed plugin template!]
    * See http://sourceforge.net/projects/notepad-plus/forums/forum/482781/topic/4911359 to get the plugin beta
* GitHub Issue 21: [Feature Request] Added optional ignore of errors in CmdLine Formatter [thanks Jörg Burdorf for the request]
* GitHub Issue 19: [Feature Request] Added optional indenting of join ON sections [thanks Pushpendra Rishi for the request]
* GitHub Issue 25: [Feature Request] Settings persistence and optional display simplification in Winforms demo app [thanks Gokhan Varol for the request]

### Changed

* Github Issue 22: [Bugfix] Indenting of "AS" clause on 2nd or later CTE in a query
* GitHub Issue 18: [Bugfix] Corrected positioning of comments (after linebreak) at end of statements [thanks gvarol for the bug report]
* GitHub Issue 24: [Bugfix] HTMLEncoding missing in Web Service (eg for poorsql.com) and Winforms demo app [thanks Gokhan Varol for the bug report]
* GitHub Issue 33: [Bugfix] Culture-specific uppercasing bug (Turkish) [thanks Recep Guzel]
* GitHub Issue 35: [Enhancement] WITH clause breaking [thanks Lane Duncan for the suggestion]
* Softcoded parsing error message in library (for translation + optional removal of warning)


## [1.0.1] - 2011-08-25

### Added

* Added MERGE clause and statement support
* Added OUTPUT and OUTPUT ... INTO ... clause support
* Added handling of ISO data type synonyms, with keyword consistency correction to Sql Server datatypes
* Added missing SQL Server 2008 datatypes, Date/Time-related, HierarchyID, and Geography
* Added Expected SQL Parse Tree and Standard Output Sql tests in test suite - parse Xml format and output SQL format effectively finalized.

### Changed

* Corrected INSERT ... EXEC parsing/formatting
* Corrected VALUES ... indenting
* Corrected handling of multiple CTEs in a single statement
* Corrected handling of AS in stored procedure argument data type specification
* Corrected minor indenting bug with single-word clauses at the end of multi-statement container content


## [0.9.13] - 2011-08-01

### Added

* Added handling of scope resolution operator ("::")
* Added handling of remaining SQL 2008 compound operators (eg "+=")
* Added handling of GRANT/REVOKE/DENY statements
* Added numerous keywords that were not handled (including XML datatype!)
* Added handling of EXCEPT and INTERSECT set operators
* Added general DDL "WITH" clause handling, including "EXECUTE AS"
* Added basic initial keyword standardization (or arbitrary mapping), eg "LEFT OUTER JOIN" -> "LEFT JOIN"

### Changed

* Corrected handling of colon, as NOT being a valid in-word character
* Cleaned up the test files a bit, as they are now referenced publically
* Refactoring / reducing direct XmlDocument dependency


## [0.9.12] - 2011-07-17

### Added

* New option "ExpandBetweenConditions", meaning hopefully clear
* Added indenting on arbitrary expression parens
* Added new parens type "selection target" to avoid over-indenting derived tables
* Added ability in CmdLine utility to use an output path, instead of an output file (same parameter).
* Implemented basic(!) width-based wrapping
* Implemented coloring-only option in the identityformatter

### Changed

* Corrected expanded-comma-list default to not include spaces between commas and subsequent content, with new "SpaceAfterExpandedComma" option - thanks to Loren Halvorson again for the suggestion!
* Significant changes to Parse Tree Xml structure, to better handle compound keywords and various containers
* Major refactoring of standard parser and formatter - hopefully much clearer and more maintainable now.
* Corrected handling of whitespace within compound keywords like "BEGIN TRAN"
* Corrected detection of new "SELECT" statements immediately following "INSERT ... SELECT ..." and "INSERT ... VALUES ..." statements
* Bugfix: web service "WithOptions" method was actually ignoring the options provided
* Corrected INSTEAD OF trigger type parsing
* Bugfix: tokenization of decimal values starting with a single "0", eg "0.0", was yielding two separate numbers.


## [0.9.11] - 2011-07-12

Thanks to Loren Halvorson for the feature suggestions!

### Added

* Added WinMerge plugin (actual plugin implemented in VB6, because I couldn't figure out how to get the COM Interop to work with WinMerge's plugin-loading system directly)
* Added output file option to CmdLine utility, eg for use with file comparison tools that accept commands to execute pre-comparison.
* Added return code to CmdLine utility, for feedback to calling programs such as file comparison tools.

 
## [0.9.10] - 2011-06-09

### Added

* Added linebreak-sensitive comment positioning: comments will be maintained at the end of lines they were defined on.

### Changed

* Corrected command-line utility to handle "." as path input pattern
* Fixed buggy "UNION" parsing and display formatting
* Removed attempted quickfix "OUTPUT" clause handling - it messed up stored proc OUTPUT parameters
* Fixed buggy interaction of DDL containers (cursors, etc) with control-of-flow statements (begin/end, if/else, etc).
* corrected comment positioning under some specific circumstances
* improved "GO" batch separator parsing to match SSMS behaviour, eg allowing single-line comments on the same line  

## [0.9.9] - 2011-06-07

### Added

* Added single-word "Commit" and "Rollback" keyword support.
* Added handling for Cursors, both ISO and MS-specific forms
* Added version information in cmdline utility
* Added BULK INSERT handling
* Added SAVE TRANSACTION handling
* Added parsing of (Data) Trigger FOR/AFTER/INSTEAD OF clauses

### Changed

* Tested library in Mono (but have now added linqbridge since, might have thrown a spanner in the works)
* Fixed SSMS 2008 support (thanks Tim Costello for my first bug report)
* Fixed semicolon new-statement detection to handle batch separators and ELSE clauses/statments.
 
## [0.9.8] - 2011-05-22

### Added

* Added parse-error-handling to command-line utility
* Added easier parse-error-detection and common constructor to formatting manager class
* Added handling for double quotes, incl. escaping
* Added number parsing/tokenization
* Added binary/hex value parsing/tokenization
* Added money parsing/tokenization
* Added tests for number, currency and binary parsing/tokenization edge cases
* Added parsing of Labels as statements
* Added merge in winforms demo app's build process, for single-assembly demo

### Changed

* Fixed parsing error with UPDATE statements in IF blocks
* Fixed handling of AS keyword for defining data types
* Fixed handling of nested ELSE clauses
* Fixed merged (ilmerge) output type for command-line utility
* Fixed multiline comment handling to respect T-SQL comment nesting
* Fixed grouping of DECLARE, SET, and PRINT statements
 

## [0.9.7] - 2011-05-16

### Added

* Added command-line formatter
    * single assembly/executable
    * supports flags for all the standard formatting options
    * allows for single files or wildcards, at a specified level or recursive
    * defaults to ".sql" extensions only, but allows for adding others

### Changed

* Corrected parsing error with GOTO statements


## [0.9.6] - 2011-05-15

### Added

* SSMS Add-in
    * Handles Visual Studio standard Ctrl-K, Ctrl-F hotkey combination
    * Adds menu item to "Tools" menu
    * Formats selected text if there is any, or entire document otherwise
    * Warns about parse failures with option to abort
    * Supports SSMS and Express 2005 and 2008

## [0.9.5] - 2011-05-03

### Added

* Added CTE parsing
* Added grouping of DECLARE and SET statements
 
### Changed

* Cleaned up delivery mechanism for full HTML pages

## [0.9.4] - 2011-05-02

### Added

* Added HTML syntax colorizing (exposed in demo app and web service) 

## [0.9.3] - 2011-05-01

### Added

* Added formatting of union clauses
* Added formatting of BETWEEN conditions
* Introduced notion of Keywords, added Keyword Uppercasing (incl Operators)

### Changed

* Refactored Tokenizer output to be a more-efficient List instead of an Xml Document
* Misc minor formatting enhancements (comments, cross apply, etc)

## [0.9.2] - 2011-04-28

### Added

* Added structured output of Derived Tables (and subqueries)
* Added structured output of Case Statements
* Added more handled keywords

## [0.9.1] - 2011-04-27

### Added

* Added web service demo project
* Added more handled keywords
* Added Semicolon (statement separator) handling

### Changed

* Corrected Period handling

## [0.9.0] - 2011-04-25

### Added

Initial release, see README.md for known issues and plans.


