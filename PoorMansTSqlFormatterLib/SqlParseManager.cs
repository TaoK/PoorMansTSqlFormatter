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
    public class SqlParseManager
    {
        private Interfaces.ISqlTokenizer _tokenizer;
        private Interfaces.ISqlTokenParser _parser;
        private Interfaces.ISqlTreeFormatter _formatter;

        //default to built-in
        public SqlParseManager() : this(new Tokenizers.TSqlStandardTokenizer(), new Parsers.TSqlStandardParser(), new Formatters.TSqlStandardFormatter()) { }
        public SqlParseManager(Interfaces.ISqlTokenizer tokenizer, Interfaces.ISqlTokenParser parser, Interfaces.ISqlTreeFormatter formatter)
        {
            _tokenizer = tokenizer;
            _parser = parser;
            _formatter = formatter;
        }

        public string Format(string inputSQL)
        {
            return _formatter.FormatSQLTree(_parser.ParseSQL(_tokenizer.TokenizeSQL(inputSQL)));
        }

        public static string DefaultFormat(string inputSQL)
        {
            return new SqlParseManager().Format(inputSQL);
        }
    }
}
