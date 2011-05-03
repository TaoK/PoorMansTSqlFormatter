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

namespace PoorMansTSqlFormatterLib
{
    public class SqlFormattingManager
    {
        //default to built-in
        public SqlFormattingManager() : this(new Tokenizers.TSqlStandardTokenizer(), new Parsers.TSqlStandardParser(), new Formatters.TSqlStandardFormatter()) { }

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
            return Formatter.FormatSQLTree(Parser.ParseSQL(Tokenizer.TokenizeSQL(inputSQL)));
        }

        public static string DefaultFormat(string inputSQL)
        {
            return new SqlFormattingManager().Format(inputSQL);
        }
    }
}
