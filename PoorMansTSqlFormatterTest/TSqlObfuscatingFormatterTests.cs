﻿/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

Additional Contributors:
 * Timothy Klenke, 2012

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
    public class TSqlObfuscatingFormatterTests
    {

        ISqlTokenizer _tokenizer;
        ISqlTokenParser _parser;
        TSqlStandardFormatter _standardFormatter;
        TSqlObfuscatingFormatter _obfuscatingFormatter;

        public TSqlObfuscatingFormatterTests()
        {
            _tokenizer = new TSqlStandardTokenizer();
            _parser = new TSqlStandardParser();
            _standardFormatter = new TSqlStandardFormatter(new TSqlStandardFormatterOptions
                {
                    TrailingCommas = true,
                    KeywordStandardization = true
                });
            _obfuscatingFormatter = new TSqlObfuscatingFormatter();
        }

        [Test, TestCaseSource(typeof(Utils), nameof(Utils.GetInputSqlFileNames))]
        public void ObfuscatingFormatReformatMatch(string FileName)
        {
            string inputSQL = Utils.GetTestFileContent(FileName, Utils.INPUTSQLFOLDER);
            ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQL);
            Node parsedOriginal = _parser.ParseSQL(tokenized);
            string obfuscatedSql = _obfuscatingFormatter.FormatSQLTree(parsedOriginal);

            var inputToSecondPass = obfuscatedSql;
            if (inputToSecondPass.StartsWith(Utils.ERROR_FOUND_WARNING))
                inputToSecondPass = inputToSecondPass.Replace(Utils.ERROR_FOUND_WARNING, "");

            ITokenList tokenizedAgain = _tokenizer.TokenizeSQL(inputToSecondPass);
            Node parsedAgain = _parser.ParseSQL(tokenizedAgain);
            string unObfuscatedSql = _standardFormatter.FormatSQLTree(parsedAgain);

            Utils.StripCommentsFromSqlTree(parsedOriginal);
            string standardFormattedSql = _standardFormatter.FormatSQLTree(parsedOriginal);

            Assert.That(unObfuscatedSql, Is.EqualTo(standardFormattedSql), "standard-formatted vs obfuscatd and reformatted");
        }
    }
}
