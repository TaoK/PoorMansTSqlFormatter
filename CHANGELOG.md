
## Poor Man's T-SQL Formatter change log

### Version 1.5.1

* Github Issue #85: [Enhancement] Merged code cleanup changes by Timothy Klenke
* Github Issue #87: [Enhancement] Added support for SQLite bit-shift and c-style equality operators [thanks Tom Holden for the request]
* Github Issue #86: [Enhancement] Added support for Visual Studio 2012 [thanks Jarred Cleem for the request]
* Github Issue #81: [Enhancement] Added support for C-Style comments ("//") and colon-prefixed parameter/host-variable names for other SQL dialects [thanks Paul Toms for the request]
* Github Issue #90: [Enhancement] Merged more code cleanup / options simplification changes by Timothy Klenke
* Github Issue #47: [Enhancement] Separated option for expansion of IN lists [thanks to a number of people for the request: smartkiwi, defrek, Marvin Eads and Bill Ruehle]

### Version 1.4.4 (SSMS / Visual Studio Add-In only)

* Github Issue #79: [BugFix] Fix SSMS Add-In to work in unexpected languages [thanks Paulo Stradioti for the bug report]
* Github Issue #16: [New Feature] Add Visual Studio support to the Add-In

### Version 1.4.3 (library + poorsql.com + windows Forms only)

* Github Issue #75: [Enhancement] Add parse error highlighting in HTML output [thanks Jeff Clark for the suggestion]

### Version 1.4.2 (library only)

* Github Issue #70: [Enhancement] Eliminate "Client Framework"-incompatible dependency on System.Web [thanks to Brad Wood and Richard King for feedback on this]

### Version 1.4.1

* Github Issue #54: [New Feature] Support for DB2/SQLLite string concatenation operator
* Github Issue #48: [Bugfix] Correction to SQL 2012 add-in install folder 
* Github Issue #52: [Bugfix] Fix to space-indent saving in formatting plugin options (SSMS, Notepad++)
* [Bugfix] Fix examples in commandline help to correspond to existing options [thanks John Landmesser for the bug report]
* Github Issue #74: [Bugfix] Correct parsing & formatting of subqueries & function calls in variable initializers
* Github Issue #73: [Bugfix] Correct parsing & formatting of OPTION clauses
* Github Issue #55: [Bugfix] Correct parsing & formatting of OUTPUT specifiers on proc arguments defined without parens
	

### Version 1.3.1

* Github Issue 23: [New Feature] Support for SSMS 2012 / Denali
* Github Issue 34: [New Feature] Obfuscating formatter / minifier in Winforms app and Web Service
* Github Issue 36: [New Feature] Support for "[noformat][/noformat]" and "[minify][/minify]" block-formatting instructions in standard formatter [thanks dquille for the request]
* Github Issue 37: [Bugfix] SSMS Hotkey binding failed in non-english versions of SSMS [thanks Philipp Schornberg for the bug report]
* Github Issue 38: [Bugfix] Comment-positioning bugs, one of which could result in invalid output SQL! [thanks andywuest for the bug report]
* Github Issue 39: [Bugfix] Formatting Library not usable in .Net 3.5 and later projects, because of Linqbridge conflict on Linq namespace [thanks Sean Bornstein for the bug report]
* Github Issue 40: [Bugfix] Error parsing WITH options containing "ON" in parentheses [thanks Jean-Luc Mellet for the bug report]


### Version 1.2.1

* Github Issue 32: [Feature Request] Enhanced Cmdline Utility to support pipeline input (and output) [thanks William Lin for the request]
    * but default behaviour unchanged, still expect to act on files directly unless piped input provided
    * expected encoding is UTF-8... in powershell, for example, run "$OutputEncoding = [System.Text.Encoding]::UTF8" first.
* Enhanced Cmdline Utility "no files found" message to include extensions detail
* Github Issue 26: Testing enhancements:
    * Changed testing framework to Nunit, included dependencies for building and testing in any environment
    * Simplified testing code, with test sources
    * Added tests for different formatting options / flags, greater coverage
* GitHub Issue 31: Completed Notepad++ Plugin, with formatting options - available trough Notepad++ plugin manager.


### Version 1.1.1

* Translated programs, ssms plugin + website into French and Spanish 
    * thanks to Threeplicate for Amanuens, a free (for open-source projects) localization tool that makes managing translations much easier!
    * and thanks to my wife for proofreading... technical translations are always a mess, but at least I got a second pair of eyes on it!
* Initial Beta of Notepad++ plugin [thanks UFO and Robert Giesecke for the .Net managed plugin template!]
    * See http://sourceforge.net/projects/notepad-plus/forums/forum/482781/topic/4911359 to get the plugin beta
* Github Issue 22: [Bugfix] Indenting of "AS" clause on 2nd or later CTE in a query
* GitHub Issue 21: [Feature Request] Added optional ignore of errors in CmdLine Formatter [thanks Jörg Burdorf for the request]
* GitHub Issue 18: [Bugfix] Corrected positioning of comments (after linebreak) at end of statements [thanks gvarol for the bug report]
* GitHub Issue 19: [Feature Request] Added optional indenting of join ON sections [thanks Pushpendra Rishi for the request]
* GitHub Issue 24: [Bugfix] HTMLEncoding missing in Web Service (eg for poorsql.com) and Winforms demo app [thanks Gokhan Varol for the bug report]
* GitHub Issue 25: [Feature Request] Settings persistence and optional display simplification in Winforms demo app [thanks Gokhan Varol for the request]
* GitHub Issue 33: [Bugfix] Culture-specific uppercasing bug (Turkish) [thanks Recep Guzel]
* GitHub Issue 35: [Enhancement] WITH clause breaking [thanks Lane Duncan for the suggestion]
* Softcoded parsing error message in library (for translation + optional removal of warning)


### Version 1.0.1: 

* Added MERGE clause and statement support
* Added OUTPUT and OUTPUT ... INTO ... clause support
* Corrected INSERT ... EXEC parsing/formatting
* Corrected VALUES ... indenting
* Corrected handling of multiple CTEs in a single statement
* Corrected handling of AS in stored procedure argument data type specification
* Added handling of ISO data type synonyms, with keyword consistency correction to Sql Server datatypes
* Added missing SQL Server 2008 datatypes, Date/Time-related, HierarchyID, and Geography
* Corrected minor indenting bug with single-word clauses at the end of multi-statement container content
* Added Expected SQL Parse Tree and Standard Output Sql tests in test suite - parse Xml format and output SQL format effectively finalized.


### Version 0.9.13

* Added handling of scope resolution operator ("::")
* Corrected handling of colon, as NOT being a valid in-word character
* Added handling of remaining SQL 2008 compound operators (eg "+=")
* Added handling of GRANT/REVOKE/DENY statements
* Added numerous keywords that were not handled (including XML datatype!)
* Added handling of EXCEPT and INTERSECT set operators
* Cleaned up the test files a bit, as they are now referenced publically
* Added general DDL "WITH" clause handling, including "EXECUTE AS"
* Refactoring / reducing direct XmlDocument dependency
* Added basic initial keyword standardization (or arbitrary mapping), eg "LEFT OUTER JOIN" -> "LEFT JOIN"


### Version 0.9.12

* Corrected expanded-comma-list default to not include spaces between commas and subsequent content, with new "SpaceAfterExpandedComma" option - thanks to Loren Halvorson again for the suggestion!
* New option "ExpandBetweenConditions", meaning hopefully clear
* Significant changes to Parse Tree Xml structure, to better handle compound keywords and various containers
* Major refactoring of standard parser and formatter - hopefully much clearer and more maintainable now.
* Corrected handling of whitespace within compound keywords like "BEGIN TRAN"
* Added indenting on arbitrary expression parens
* Added new parens type "selection target" to avoid over-indenting derived tables
* Corrected detection of new "SELECT" statements immediately following "INSERT ... SELECT ..." and "INSERT ... VALUES ..." statements
* Bugfix: web service "WithOptions" method was actually ignoring the options provided
* Corrected INSTEAD OF trigger type parsing
* Added ability in CmdLine utility to use an output path, instead of an output file (same parameter).
* Bugfix: tokenization of decimal values starting with a single "0", eg "0.0", was yielding two separate numbers.
* Implemented basic(!) width-based wrapping
* Implemented coloring-only option in the identityformatter


### Version 0.9.11

Thanks to Loren Halvorson for the feature suggestions!

* Added WinMerge plugin (actual plugin implemented in VB6, because I couldn't figure out how to get the COM Interop to work with WinMerge's plugin-loading system directly)
* Added output file option to CmdLine utility, eg for use with file comparison tools that accept commands to execute pre-comparison.
* Added return code to CmdLine utility, for feedback to calling programs such as file comparison tools.

 
### Version 0.9.10

* Corrected command-line utility to handle "." as path input pattern
* Fixed buggy "UNION" parsing and display formatting
* Removed attempted quickfix "OUTPUT" clause handling - it messed up stored proc OUTPUT parameters
* Fixed buggy interaction of DDL containers (cursors, etc) with control-of-flow statements (begin/end, if/else, etc).
* Added linebreak-sensitive comment positioning: comments will be maintained at the end of lines they were defined on.
* corrected comment positioning under some specific circumstances
* improved "GO" batch separator parsing to match SSMS behaviour, eg allowing single-line comments on the same line 
 

### Version 0.9.9

* Tested library in Mono (but have now added linqbridge since, might have thrown a spanner in the works)
* Added single-word "Commit" and "Rollback" keyword support.
* Added handling for Cursors, both ISO and MS-specific forms
* Added version information in cmdline utility
* Added BULK INSERT handling
* Added SAVE TRANSACTION handling
* Added parsing of (Data) Trigger FOR/AFTER/INSTEAD OF clauses
* Fixed SSMS 2008 support (thanks Tim Costello for my first bug report)
* Fixed semicolon new-statement detection to handle batch separators and ELSE clauses/statments.

 
### Version 0.9.8

* Fixed parsing error with UPDATE statements in IF blocks
* Fixed handling of AS keyword for defining data types
* Fixed handling of nested ELSE clauses
* Added parse-error-handling to command-line utility
* Added easier parse-error-detection and common constructor to formatting manager class
* Fixed merged (ilmerge) output type for command-line utility
* Fixed multiline comment handling to respect T-SQL comment nesting
* Added handling for double quotes, incl. escaping
* Added number parsing/tokenization
* Added binary/hex value parsing/tokenization
* Added money parsing/tokenization
* Added tests for number, currency and binary parsing/tokenization edge cases
* Fixed grouping of DECLARE, SET, and PRINT statements
* Added parsing of Labels as statements
* Added merge in winforms demo app's build process, for single-assembly demo
 

### Version 0.9.7

* Added command-line formatter
    * single assembly/executable
    * supports flags for all the standard formatting options
    * allows for single files or wildcards, at a specified level or recursive
    * defaults to ".sql" extensions only, but allows for adding others
* Corrected parsing error with GOTO statements


### Version 0.9.6

* Added SSMS Add-in
    * Handles Visual Studio standard Ctrl-K, Ctrl-F hotkey combination
    * Adds menu item to "Tools" menu
    * Formats selected text if there is any, or entire document otherwise
    * Warns about parse failures with option to abort
    * Supports SSMS and Express 2005 and 2008


### Version 0.9.5

* Added CTE parsing
* Cleaned up delivery mechanism for full HTML pages
* Added grouping of DECLARE and SET statements
 

### Version 0.9.4

* Added HTML syntax colorizing (exposed in demo app and web service)
 

### Version 0.9.3

* Added formatting of union clauses
* Misc minor formatting enhancements (comments, cross apply, etc)
* Added formatting of BETWEEN conditions
* Refactored Tokenizer output to be a more-efficient List instead of an Xml Document
* Introduced notion of Keywords, added Keyword Uppercasing (incl Operators)


### Version 0.9.2

* Added structured output of Derived Tables (and subqueries)
* Added structured output of Case Statements
* Added more handled keywords
 

### Version 0.9.1

* Added web service demo project
* Added more handled keywords
* Corrected Period handling
* Added Semicolon (statement separator) handling


### Version 0.9.0

Initial release, see README.md for known issues and plans.


