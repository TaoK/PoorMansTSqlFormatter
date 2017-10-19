/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

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


#if !SIMPLIFIEDFW
using System;
using System.Runtime.InteropServices;
#endif
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib
{
#if !SIMPLIFIEDFW
    //These COM-related attributes exist JUST so that we can use this class from VB6 - there is no need to
    // expose these classes to COM in order for this library to be used in a .Net project.
    [Guid("A7FD140A-C3C3-4233-95DB-A64B50C8DF2B")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComVisible(true)]
    [ProgId("PoorMansTSqlFormatter.SqlFormattingManager")]
#endif
    public class SqlFormattingManager : _SqlFormattingManager
    {
        //default to built-in
        public SqlFormattingManager() : this(new Tokenizers.TSqlStandardTokenizer(), new Parsers.TSqlStandardParser(), new Formatters.TSqlStandardFormatter()) { }

        //most common use-case, define only formatter
        public SqlFormattingManager(ISqlTreeFormatter formatter) : this(new Tokenizers.TSqlStandardTokenizer(), new Parsers.TSqlStandardParser(), formatter) { }

        public SqlFormattingManager(ISqlTokenizer tokenizer, ISqlTokenParser parser, ISqlTreeFormatter formatter)
        {
            Tokenizer = tokenizer;
            Parser = parser;
            Formatter = formatter;
        }

        public ISqlTokenizer Tokenizer { get; set; }
        public ISqlTokenParser Parser { get; set; }
        public ISqlTreeFormatter Formatter { get; set; }

        public string Format(string inputSQL)
        {
            bool error = false;
            return Format(inputSQL, ref error);
        }

        public string Format(string inputSQL, ref bool errorEncountered)
        {
            Node sqlTree = Parser.ParseSQL(Tokenizer.TokenizeSQL(inputSQL));
            errorEncountered = (sqlTree.GetAttributeValue(SqlStructureConstants.ANAME_ERRORFOUND) == "1");
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
#if !SIMPLIFIEDFW
    [Guid("A7FD140A-C3C3-4233-95DB-A64B50C8DF2A")]
    [CLSCompliant(false), ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
#endif
    public interface _SqlFormattingManager
    {
        string Format(string inputSQL);
    }
}
