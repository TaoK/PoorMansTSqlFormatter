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
    public class ParserTests
    {
        ISqlTokenizer _tokenizer;
        ISqlTokenParser _parser;

        public ParserTests()
        {
            _tokenizer = new TSqlStandardTokenizer();
            _parser = new TSqlStandardParser();
        }

        string ParsedDataFolder { get { return Utils.GetTestContentFolder("ParsedSql"); } }
        string InputDataFolder { get { return Utils.GetTestContentFolder("InputSql"); } }

        [Test]
        public void CheckThatParseTreeMatchesExpectedParseTree()
        {
            foreach (FileInfo xmlFile in new DirectoryInfo(ParsedDataFolder).GetFiles())
            {
                XmlDocument expectedXmlDoc = new XmlDocument();
                expectedXmlDoc.PreserveWhitespace = true;
                expectedXmlDoc.Load(xmlFile.FullName);
                string inputSql = File.ReadAllText(Path.Combine(InputDataFolder, xmlFile.Name));

                ITokenList tokenized = _tokenizer.TokenizeSQL(inputSql);
                XmlDocument parsed = _parser.ParseSQL(tokenized);

                Assert.AreEqual(expectedXmlDoc.OuterXml, parsed.OuterXml, string.Format("Parse Tree Xml does not match for file {0}", xmlFile.Name));
            }
        }
    }
}
