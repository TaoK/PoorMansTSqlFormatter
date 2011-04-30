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
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using PoorMansTSqlFormatterLib.Formatters;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.Parsers;
using PoorMansTSqlFormatterLib.Tokenizers;

namespace PoorMansTSqlFormatterTests
{
    [TestClass]
    public class TSqlIdentityFormatterTest
    {
        ISqlTokenizer _tokenizer;
        ISqlTokenParser _parser;
        ISqlTokenFormatter _tokenFormatter;
        ISqlTreeFormatter _treeFormatter;

        public TSqlIdentityFormatterTest()
        {
            _tokenizer = new TSqlStandardTokenizer();
            _parser = new TSqlStandardParser();
            _tokenFormatter = new TSqlIdentityFormatter();
            _treeFormatter = (ISqlTreeFormatter)_tokenFormatter;
        }

        string TestDataFolder { get { return Utils.GetTestContentFolder("InputSql"); } }

        [TestMethod]
        public void CheckThatValidSqlRemainsUnchangedByIdentityTokenFormatter()
        {
            foreach (string inputSQL in Utils.FolderTextFileIterator(TestDataFolder))
            {
                ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQL);
                string outputSQL = _tokenFormatter.FormatSQLTokens(tokenized);
                if (!inputSQL.Contains("THIS TEST FILE IS NOT VALID SQL"))
                    Assert.AreEqual<string>(outputSQL, inputSQL, "input and output should be the same, as this is a valid SQL file");
            }
        }

        [TestMethod]
        public void CheckThatValidSqlRemainsUnchangedByIdentityTreeFormatter()
        {
            foreach (string inputSQL in Utils.FolderTextFileIterator(TestDataFolder))
            {
                ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQL);
                XmlDocument parsed = _parser.ParseSQL(tokenized);
                string outputSQL = _treeFormatter.FormatSQLTree(parsed);
                if (!inputSQL.Contains("THIS TEST FILE IS NOT VALID SQL"))
                    Assert.AreEqual<string>(outputSQL, inputSQL, "input and output should be the same, as this is a valid SQL file");
            }
        }
    }
}
