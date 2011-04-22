Poor Man's T-SQL Formatter
--------------------------

This is a small free .Net 2.0 library (with demo program) for reformatting T-SQL code.

Features:
 - Simple Xml-based parse tree
 - Extensible, with possibility of supporting other SQL dialects (but none implemented)
 - Configurable according to SQL formatting preferences

Limitations:
 - This is NOT a full SQL-parsing solution: only "coarse" parsing is performed, the 
    minimum necessary for re-formatting.
 - The parsing implementation is not very efficient - no effort has been made to 
    optimize it.
 - The code is very "procedural" - no effort has been made to organize the code 
    according to object-oriented design principles.

Known Issues / Todo:
 - Finalize compound-term clause handling, such as "INNER JOIN"
 - Lots of refactoring required
 - CASE statement structure handling (for breaking/indenting)
 - Comma-delimited list handling (for breaking/indenting)
 - Logical Operator handling (for breaking/indenting)
 - Keyword Capitalization
 - HTML output (color highlighting)

   
This application is released under GPLv3: http://www.gnu.org/licenses/gpl.html

Please contact me with any questions, concerns, or issues: my email address starts
with tao, and is hosted at klerks dot biz.

Tao Klerks

