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

 - The parsing implementation is not very efficient - little effort has been made to 
    optimize it.

 - The code is very "procedural" - no effort has been made to organize the code 
    according to object-oriented design principles.

 - The SQL parse tree structure used does not always allow for maintaining all 
    aspects of the original T-SQL code structure: for example, it cannot represent
    a comment inside an "INNER JOIN" compound keyword, like "inner/*test*/join".
    such specific situations will result in information loss during parsing: the 
    comment in this case will be moved to after the compound keyword. (such issues
	are flagged in the formatted SQL)

	
Known Issues / Todo:
 - Handle indented selects
   - Derived tables
   - subqueries / correlated subqueries
 - CASE statement structure handling (for breaking/indenting)
 - Comma-delimited list handling (for breaking/indenting)
 - Logical Operator handling (for breaking/indenting)
 - Keyword Capitalization
 - Options in standard formatter and demo
 - HTML output (color highlighting)

   
This application is released under the GNU Affero GPL v3: 
http://www.gnu.org/licenses/agpl.txt

Please contact me with any questions, concerns, or issues: my email address starts
with tao, and is hosted at klerks dot biz.

Tao Klerks

