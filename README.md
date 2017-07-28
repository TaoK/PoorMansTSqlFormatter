
## Poor Man's T-SQL Formatter

This is a small free .Net 2.0 and JS library (with demo winforms program, web service,
SSMS and Visual Studio Addin, Command-line utility, Notepad++ plugin, and WinMerge 
plugin) for reformatting T-SQL code.



### Features

* Simple Xml-style parse tree
* Extensible, with possibility of supporting other SQL dialects (but none implemented)
* Configurable according to SQL formatting preferences
* Handles "procedural" T-SQL; this is not just a SQL statement formatter, but it also 
    formats entire batches, and multi-batch scripts.
* Optional colorized HTML output
* Fault-tolerant parsing and formatting - if some unknown SQL construct is encountered
    or a keyword is misinterpreted, parsing does not fail (but will simply not colorize
    or indent that portion correctly). If the parsing fails more catastrophically, a 
    "best effort" will be made and warning displayed (or in the case of interactive 
    use, eg in SSMS, the operation can be aborted).
* Reasonably fast: reformatting 1,500 or so files totalling 4MB takes 30 seconds on a 
    cheap atom-processor (2009) netbook.
* Works in Microsoft .Net framework, as well as Mono. The Winforms Demo App is not (yet?)
    available in Mono, but the library itself is fully functional, as is the command-line
    bulk formatting tool.
* JS library (transpiled from C#) is fully functional for browser or other (eg Node.js) 
    contexts.


### General Limitations

* This is NOT a full SQL-parsing solution: only "coarse" parsing is performed, the 
    minimum necessary for re-formatting.
* The standard formatter does not always maintain the order of comments in the code;
    a comment inside an "INNER JOIN" compound keyword, like "inner/\*test\*/join", would
    get moved out, to "INNER JOIN /\*test\*/". The original data is maintaned in the 
    parse tree, but the standard formatter shuffles comments in cases like this for 
    clarity.
* DDL parsing, in particular, is VERY coarse - the bare minimum to display ordered table 
    column and procedure parameter declarations.
* No effort has been made to support compatibility level 70 (SQL Server 7)
* Where there is ambiguity between different compatibility levels (eg cross apply 
    parens in compatibility level 90 vs table hints without "WITH" keyword in 
    compatibility level 80), no approach has been decided. For now, table hints 
    without WITH are considered to be arguments to a function.
* Settings may not be correctly maintained across major upgrades of SSMS and Visual Studio
 
### Known Issues / Todo

* Handling of DDL Triggers (eg "FOR LOGON")
* Formatting/indenting of ranking functions 
* FxCop checking
* And other stuff that is tracked in the GitHub issues list


### Longer-term enhancements / additions

* Compiled mono library + bulk formatting tool download (eg for use on SVN server)
* Documentation of Xml structure and class usage
    * Keeping track of versioning and documentation more carefully: http://semver.org/

### License & Credits

This application and library is released under the GNU Affero GPL v3: 
http://www.gnu.org/licenses/agpl.txt

The homepage for this project is currently: 
http://www.architectshack.com/PoorMansTSqlFormatter.ashx

This project uses several external libraries:

* NDesk.Options, for command-line parsing: The NDesk.Options library is licensed under 
    the MIT/X11 license, and its homepage is here: http://www.ndesk.org/Options
* LinqBridge, for convenience, supporting extension methods and Linq-to-Objects 
    despite this being a .Net 2.0 library. LinqBridge is licensed under the BSD 3-clause 
    license, and its homepage is here: http://code.google.com/p/linqbridge/
* NUnit, for automated testing. NUnit is licensed under a custom open-source license
    based on the zlib/libpng license, and its homepage is: http://www.nunit.org/
* UnmanagedExports (DLLExport), for exporting .Net code to Notepad++ plugin environment
* Notepad++ C# plugin template, based on work by Robert Giesecke and UFO, 
    available from the [notepad++ plugin development forum](https://sourceforge.net/projects/notepad-plus/forums/forum/482781).
* ILRepack, by François Valdy, for assembly-merging, available from the [github project page](https://github.com/gluck/il-repack).
* Bridge.Net, by Object.Net, for C#-to-JS transpiling, available from http://bridge.net

Special thanks to contributors that have given their time to make this library better:

* Timothy Klenke

Also thanks to Adam Pawsey, who maintains the [NuGet package](http://nuget.org/packages/PoorMansTSQLFormatter/).

Many of the features in this project result from feedback by multiple people, including
but not limited to:

* Loren Halvorson
* Recep Guzel
* Lane Duncan
* Gokhan Varol
* Pushpendra Rishi
* Jonathan Fahey
* Tim Costello
* Jörg Burdorf
* William Lin
* Brad Wood
* Richard King
* Jeff Clark
* Jarred Cleem
* Paul Toms
* Tom Holden
* Marvin Eads
* Bill Ruehle
* Farzad Jalali
* Sheldon Hull
* Benjamin Solomon


Translation work on this project was originally facilitated by [Amanuens](http://amanuens.com/), the online translation platform that is now sadly defunct.

Please contact me with any questions, concerns, or issues: my email address starts
with tao, and is hosted at klerks dot biz.

Tao Klerks

