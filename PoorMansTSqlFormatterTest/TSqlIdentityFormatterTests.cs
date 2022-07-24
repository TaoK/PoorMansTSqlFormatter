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

using PoorMansTSqlFormatterLib.Formatters;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.Parsers;
using PoorMansTSqlFormatterLib.Tokenizers;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterTests
{
    [TestFixture]
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
            _tokenFormatter.ErrorOutputPrefix = "";
        }

        [Test, TestCaseSource(typeof(Utils), nameof(Utils.GetInputSqlFileNames))]
        public void ContentUnchangedByIdentityTokenFormatter(string FileName)
        {
            string inputSQL = Utils.GetTestFileContent(FileName, Utils.INPUTSQLFOLDER);
            ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQL);
            string outputSQL = _tokenFormatter.FormatSQLTokens(tokenized);
            Assert.AreEqual(inputSQL, outputSQL);
        }

        [Test, TestCaseSource(typeof(Utils), nameof(Utils.GetInputSqlFileNames))]
        public void ContentUnchangedByIdentityTreeFormatter(string FileName)
        {
            string inputSQL = Utils.GetTestFileContent(FileName, Utils.INPUTSQLFOLDER);
            ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQL);
            Node parsed = _parser.ParseSQL(tokenized);
            string outputSQL = _treeFormatter.FormatSQLTree(parsed);
            Assert.AreEqual(inputSQL, outputSQL);
        }
    }
}
