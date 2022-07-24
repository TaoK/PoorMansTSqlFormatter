﻿/*
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

using System.Xml;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.Parsers;
using PoorMansTSqlFormatterLib.Tokenizers;
using PoorMansTSqlFormatterLib.ParseStructure;
using PoorMansTSqlFormatterLib;

namespace PoorMansTSqlFormatterTests
{
    [TestFixture]
    public class ParserTests
    {
        ISqlTokenizer _tokenizer;
        ISqlTokenParser _parser;

        public ParserTests()
        {
            _tokenizer = new TSqlStandardTokenizer();
            _parser = new TSqlStandardParser();
        }

        public static IEnumerable<string> GetParsedSqlFileNames()
        {
            return Utils.FolderFileNameIterator(Utils.GetTestContentFolder(Utils.PARSEDSQLFOLDER));
        }

        [Test, TestCaseSource(nameof(GetParsedSqlFileNames))]
        public void ExpectedParseTree(string FileName)
        {
            XmlDocument expectedXmlDoc = new XmlDocument();
            expectedXmlDoc.PreserveWhitespace = true;
            expectedXmlDoc.Load(Path.Combine(Utils.GetTestContentFolder(Utils.PARSEDSQLFOLDER), FileName));
            string inputSql = Utils.GetTestFileContent(FileName, Utils.INPUTSQLFOLDER);

            ITokenList tokenized = _tokenizer.TokenizeSQL(inputSql);
            Node parsed = _parser.ParseSQL(tokenized);

            Assert.That(parsed.ToXmlDoc().OuterXml, Is.EqualTo(expectedXmlDoc.OuterXml));
        }

    }
}
