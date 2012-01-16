/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0, written in C#. 
Copyright (C) 2011 Tao Klerks

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using System.Xml;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace PoorMansTSqlFormatterLib
{
    //These COM-related attributes exist JUST so that we can use this class from VB6 - there is no need to
    // expose these classes to COM in order for this library to be used in a .Net project.
    [Guid("A7FD140A-C3C3-4233-95DB-A64B50C8DF2B")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    [ProgId("PoorMansTSqlFormatter.SqlFormattingManager")]
    public class SqlFormattingManager : _SqlFormattingManager
    {
        //default to built-in
        public SqlFormattingManager() : this(new Tokenizers.TSqlStandardTokenizer(), new Parsers.TSqlStandardParser(), new Formatters.TSqlStandardFormatter()) { }

        //most common use-case, define only formatter
        public SqlFormattingManager(Interfaces.ISqlTreeFormatter formatter) : this(new Tokenizers.TSqlStandardTokenizer(), new Parsers.TSqlStandardParser(), formatter) { }

        public SqlFormattingManager(Interfaces.ISqlTokenizer tokenizer, Interfaces.ISqlTokenParser parser, Interfaces.ISqlTreeFormatter formatter)
        {
            Tokenizer = tokenizer;
            Parser = parser;
            Formatter = formatter;
        }

        public Interfaces.ISqlTokenizer Tokenizer { get; set; }
        public Interfaces.ISqlTokenParser Parser { get; set; }
        public Interfaces.ISqlTreeFormatter Formatter { get; set; }

        public string Format(string inputSQL)
        {
            bool error = false;
            return Format(inputSQL, ref error);
        }

        public string Format(string inputSQL, ref bool errorEncountered)
        {
            XmlDocument sqlTree = Parser.ParseSQL(Tokenizer.TokenizeSQL(inputSQL));
            errorEncountered = (sqlTree.SelectSingleNode(string.Format("/{0}/@{1}[.=1]", Interfaces.SqlXmlConstants.ENAME_SQL_ROOT, Interfaces.SqlXmlConstants.ANAME_ERRORFOUND)) != null);
            return Formatter.FormatSQLTree(sqlTree);
        }

        public static string DefaultFormat(string inputSQL)
        {
            return new SqlFormattingManager().Format(inputSQL);
        }

        public static string DefaultFormat(string inputSQL, ref bool errorsEncountered)
        {
            return new SqlFormattingManager().Format(inputSQL, ref errorsEncountered);
        }
    }

    //This COM interface exists JUST so that we can use this class from VB6 - there is no need to expose 
    // these classes to COM in order for this library to be used in a .Net project.
    [Guid("A7FD140A-C3C3-4233-95DB-A64B50C8DF2A")]
    [CLSCompliant(false), ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface _SqlFormattingManager
    {
        string Format(string inputSQL);
    }
}
