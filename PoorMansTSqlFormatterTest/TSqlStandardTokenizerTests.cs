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

using NUnit.Framework;
using PoorMansTSqlFormatterLib.Interfaces;
using PoorMansTSqlFormatterLib.Tokenizers;
using System.Collections.Generic;
using System.Linq;

namespace PoorMansTSqlFormatterTests
{
    [TestFixture]
    public class TSqlStandardTokenizerTests
    {
        ISqlTokenizer _tokenizer;

        public TSqlStandardTokenizerTests()
        {
            _tokenizer = new TSqlStandardTokenizer();
        }

        [Test]
        public void MarkerPositionRecorded_plain()
        {
            TestMarkerPosition("select 1 from somewhere select 1 from somewhere else", 35);
        }

        [Test]
        public void MarkerPositionRecorded_linebreaks()
        {
            TestMarkerPosition("select 1\r\nfrom somewhere\r\n\r\nselect 1\r\nfrom somewhere else", 40);
        }

        [Test]
        public void MarkerPositionRecorded_linefeeds()
        {
            TestMarkerPosition("select 1\nfrom somewhere\n\nselect 1\nfrom somewhere else", 36);
        }

        private void TestMarkerPosition(string inputSQLNoLineBreaks, int inputPosition)
        {
            ITokenList tokenized = _tokenizer.TokenizeSQL(inputSQLNoLineBreaks, inputPosition);
            Assert.AreEqual(SqlTokenType.OtherNode, tokenized.MarkerToken.Type, "token type");
            Assert.AreEqual("from", tokenized.MarkerToken.Value, "token value");
            Assert.AreEqual(2, tokenized.MarkerPosition);
        }
    }
}
