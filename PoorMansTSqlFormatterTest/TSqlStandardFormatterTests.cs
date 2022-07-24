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

using PoorMansTSqlFormatterLib;
using PoorMansTSqlFormatterLib.Formatters;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.Parsers;
using PoorMansTSqlFormatterLib.ParseStructure;
using PoorMansTSqlFormatterLib.Tokenizers;

namespace PoorMansTSqlFormatterTests
{
    [TestFixture]
    public class TSqlStandardFormatterTests
    {
        ISqlTokenizer _tokenizer;
        ISqlTokenParser _parser;
        Dictionary<string, TSqlStandardFormatter> _formatters;

        public TSqlStandardFormatterTests()
        {
            _tokenizer = new TSqlStandardTokenizer();
            _parser = new TSqlStandardParser();
            _formatters = new Dictionary<string, TSqlStandardFormatter>(StringComparer.OrdinalIgnoreCase);
        }

        private TSqlStandardFormatter GetFormatter(string configString)
        {
            TSqlStandardFormatter? outFormatter;
            if (!_formatters.TryGetValue(configString, out outFormatter))
            {
                var options = new TSqlStandardFormatterOptions(configString);
                outFormatter = new TSqlStandardFormatter(options);
                _formatters.Add(configString, outFormatter);
            }
            return outFormatter;
        }

        [Test, TestCaseSource(typeof(Utils), nameof(Utils.GetInputSqlFileNames))]
        public void StandardFormatReparsingReformatting(string FileName)
        {
            string inputSQL = Utils.GetTestFileContent(FileName, Utils.INPUTSQLFOLDER);
            TSqlStandardFormatter _treeFormatter = GetFormatter("");
            ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQL);
            Node parsed = _parser.ParseSQL(tokenized);
            string outputSQL = _treeFormatter.FormatSQLTree(parsed);

            var inputToSecondPass = outputSQL;
            if (inputToSecondPass.StartsWith(Utils.ERROR_FOUND_WARNING))
                inputToSecondPass = inputToSecondPass.Replace(Utils.ERROR_FOUND_WARNING, "");

            ITokenList tokenizedAgain = _tokenizer.TokenizeSQL(inputToSecondPass);
            Node parsedAgain = _parser.ParseSQL(tokenizedAgain);
            string formattedAgain = _treeFormatter.FormatSQLTree(parsedAgain);

            if (!inputSQL.Contains(Utils.REFORMATTING_INCONSISTENCY_WARNING))
            {
                Assert.That(formattedAgain, Is.EqualTo(outputSQL), "first-pass formatted vs reformatted");
                Utils.StripWhiteSpaceFromSqlTree(parsed);
                Utils.StripWhiteSpaceFromSqlTree(parsedAgain);
                Assert.That(parsedAgain.ToXmlDoc().OuterXml.ToUpper(), Is.EqualTo(parsed.ToXmlDoc().OuterXml.ToUpper()), "first parse xml vs reparse xml");
            }
        }

        public static IEnumerable<string> GetStandardFormatSqlFileNames()
        {
            return Utils.FolderFileNameIterator(Utils.GetTestContentFolder("StandardFormatSql"));
        }

        [Test, TestCaseSource(nameof(GetStandardFormatSqlFileNames))]
        public void StandardFormatExpectedOutput(string FileName)
        {
            string expectedSql = Utils.GetTestFileContent(FileName, Utils.STANDARDFORMATSQLFOLDER);
            string inputSql = Utils.GetTestFileContent(Utils.StripFileConfigString(FileName), Utils.INPUTSQLFOLDER);
            TSqlStandardFormatter _treeFormatter = GetFormatter(Utils.GetFileConfigString(FileName));

            ITokenList tokenized = _tokenizer.TokenizeSQL(inputSql);
            Node parsed = _parser.ParseSQL(tokenized);
            string formatted = _treeFormatter.FormatSQLTree(parsed);

            Assert.That(formatted, Is.EqualTo(expectedSql));
        }

    }
}
