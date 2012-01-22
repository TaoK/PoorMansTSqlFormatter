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
using NUnit.Framework;
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
    [TestFixture]
    public class TSqlStandardFormatterTests
    {
        ISqlTokenizer _tokenizer;
        ISqlTokenParser _parser;
        TSqlStandardFormatter _treeFormatter;

        public TSqlStandardFormatterTests()
        {
            _tokenizer = new TSqlStandardTokenizer();
            _parser = new TSqlStandardParser();
            _treeFormatter = new TSqlStandardFormatter();
            _treeFormatter.HTMLColoring = false;
        }

        string InputDataFolder { get { return Utils.GetTestContentFolder("InputSql"); } }
        string FormattedDataFolder { get { return Utils.GetTestContentFolder("StandardFormatSql"); } }

        [Test]
        public void CheckThatReformattingOutputSqlYieldsSameSql()
        {
            foreach (string inputSQL in Utils.FolderTextFileIterator(InputDataFolder))
            {
                ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQL);
                XmlDocument parsed = _parser.ParseSQL(tokenized);
                string outputSQL = _treeFormatter.FormatSQLTree(parsed);
                ITokenList tokenizedAgain = _tokenizer.TokenizeSQL(outputSQL);
                XmlDocument parsedAgain = _parser.ParseSQL(tokenizedAgain);
                string formattedAgain = _treeFormatter.FormatSQLTree(parsedAgain);
                if (!inputSQL.Contains("KNOWN SQL REFORMATTING INCONSISTENCY") && !inputSQL.Contains("THIS TEST FILE IS NOT VALID SQL"))
                    Assert.AreEqual(outputSQL, formattedAgain, "reformatted SQL should be the same as first pass of formatting");
            }
        }

        [Test]
        public void CheckThatReparsingOutputSqlYieldsEquivalentTree()
        {
            foreach (string inputSQL in Utils.FolderTextFileIterator(InputDataFolder))
            {
                ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQL);
                XmlDocument parsed = _parser.ParseSQL(tokenized);
                string outputSQL = _treeFormatter.FormatSQLTree(parsed);
                ITokenList tokenizedAgain = _tokenizer.TokenizeSQL(outputSQL);
                XmlDocument parsedAgain = _parser.ParseSQL(tokenizedAgain);
                Utils.StripWhiteSpaceFromSqlTree(parsed);
                Utils.StripWhiteSpaceFromSqlTree(parsedAgain);
                if (!inputSQL.Contains("KNOWN SQL REFORMATTING INCONSISTENCY") && !inputSQL.Contains("THIS TEST FILE IS NOT VALID SQL"))
                    Assert.AreEqual(parsed.OuterXml.ToUpper(), parsedAgain.OuterXml.ToUpper(), "parsed SQL trees should be the same");
            }
        }

        [Test]
        public void CheckThatStandardOutputSqlMatchesExpectedStandardOutputSql()
        {
            foreach (FileInfo expectedFormatFile in new DirectoryInfo(FormattedDataFolder).GetFiles())
            {
                string expectedSql = File.ReadAllText(expectedFormatFile.FullName);
                string inputSql = File.ReadAllText(Path.Combine(InputDataFolder, expectedFormatFile.Name));

                ITokenList tokenized = _tokenizer.TokenizeSQL(inputSql);
                XmlDocument parsed = _parser.ParseSQL(tokenized);
                string formatted = _treeFormatter.FormatSQLTree(parsed);

                Assert.AreEqual(expectedSql, formatted, string.Format("Formatted Sql does not match expected result for file {0}", expectedFormatFile.Name));
            }
        }

    }
}
