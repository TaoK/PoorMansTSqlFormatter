Poor Man's T-SQL Formatter
--------------------------

This is a small free .Net 2.0 library (with demo winforms program, web service, SSMS 
Addin, and Command-line utility) for reformatting T-SQL code.

Features:
 - Simple Xml-based parse tree
 
 - Extensible, with possibility of supporting other SQL dialects (but none implemented)
 
 - Configurable according to SQL formatting preferences
 
 - Handles "procedural" T-SQL; this is not just a SQL statement formatter, but it also 
    formats entire batches, and multi-batch scripts.

 - Optional colorized HTML output
 
 - Fault-tolerant parsing and formatting - if some unknown SQL construct is encountered
    or a keyword is misinterpreted, parsing does not fail (but will simply not colorize
    or indent that portion correctly). If the parsing fails more catastrophically, a 
    "best effort" will be made and warning displayed (or in the case of interactive 
    use, eg in SSMS, the operation can be aborted).

 - Reasonably fast: reformatting 1,500 or so files totalling 4MB takes 30 seconds on a 
    cheap atom-processor (2009) netbook.
	
 - Works in Microsoft .Net framework, as well as Mono. The Winforms Demo App is not (yet?)
    available in Mono, but the library itself is fully functional, as is the command-line
    bulk formatting tool.

Limitations:
 - This is NOT a full SQL-parsing solution: only "coarse" parsing is performed, the 
    minimum necessary for re-formatting.

 - The code is very "procedural" - no effort has been made to organize the code 
    according to object-oriented design principles.

 - The SQL parse tree structure used does not always allow for maintaining all 
    aspects of the original T-SQL code structure: for example, it cannot represent
    a comment inside an "INNER JOIN" compound keyword, like "inner/*test*/join".
    such specific situations will result in ver minor "information loss" during 
    parsing: the comment in this case will be moved to after the compound keyword, 
    so the original ordinal position of the comment is lost. (and such issues are 
    flagged in the formatted SQL)

 - DDL parsing is VERY coarse, the bare minimum to display ordered table column 
    and procedure parameter declarations.
	
Known Issues / Todo:
 - Parsing of DML WITH clauses, for better formatting
 - Formatting/indenting of ranking functions 
 - Better handling of indenting in parentheses, esp. in boolean expressions
 - "Max Line Width" wrapping feature
 - FxCop checking

Longer-term enhancements / additions:
 - Keyword consistency correction feature (eg LEFT OUTER JOIN -> LEFT JOIN)
 - Demo App download, with proper license notices, etc
 - Command-line formatter utility download
 - SSMS Add-in download
 - Compiled library download
 - Compiled mono library + bulk formatting tool download (eg for use on SVN server)
 - Documentation of Xml structure and class usage
 - Pakaging, NuGet and/or OpenWrap
   - Keeping track of versioning and documentation more carefully: http://semver.org/

This application and library is released under the GNU Affero GPL v3: 
http://www.gnu.org/licenses/agpl.txt

The homepage for this project is currently: 
http://www.architectshack.com/PoorMansTSqlFormatter.ashx

This project currently uses one external library, NDesk.Options, for command-line parsing.
The NDesk.Options library is licensed under the MIT/X11 license, and its homepage is here:
http://www.ndesk.org/Options

Please contact me with any questions, concerns, or issues: my email address starts
with tao, and is hosted at klerks dot biz.

Tao Klerks

